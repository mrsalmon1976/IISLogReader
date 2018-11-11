using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Data.Stores;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Exceptions;
using System.IO;
using IISLogReader.BLL.Services;
using Tx.Windows;
using Test.IISLogReader.TestAssets;
using IISLogReader.BLL.Repositories;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class ResetRequestAggregatesCommandTest
    {
        private IResetRequestAggregatesCommand _resetRequestAggregateCommand;

        private IDbContext _dbContext;
        private ILogFileRepository _logFileRepo;
        private IRequestRepository _requestRepo;
        private IProjectRequestAggregateRepository _projectRequestAggregateRepo;
        private IRequestAggregationService _requestAggregationService;

        [SetUp]
        public void ResetRequestAggregatesCommand_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _logFileRepo = Substitute.For<ILogFileRepository>();
            _requestRepo = Substitute.For<IRequestRepository>();
            _projectRequestAggregateRepo = Substitute.For<IProjectRequestAggregateRepository>();
            _requestAggregationService = Substitute.For<IRequestAggregationService>();

            _resetRequestAggregateCommand = new ResetRequestAggregatesCommand(_dbContext, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService);
        }

        [TearDown]
        public void ResetRequestAggregatesCommand_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_NoRequests_AggregatesNotLoadedAndLogFileMarkedAsProcessed()
        {
            int logFileId = new Random().Next(1, 1000);

            // setup
            IEnumerable<RequestModel> requests = Enumerable.Empty<RequestModel>();
            _requestRepo.GetByLogFile(logFileId).Returns(requests);

            // execute
            _resetRequestAggregateCommand.Execute(logFileId);

            // assert
            _requestRepo.Received(1).GetByLogFile(logFileId);
            _logFileRepo.DidNotReceive().GetById(Arg.Any<int>());
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_RequestsFound_AggregatesLoadedAndLogFileMarkedAsProcessed()
        {
            int logFileId = new Random().Next(1, 1000);

            // setup
            RequestModel[] requests = { DataHelper.CreateRequestModel(logFileId), DataHelper.CreateRequestModel(logFileId) };
            _requestRepo.GetByLogFile(logFileId).Returns(requests);

            LogFileModel logFile = DataHelper.CreateLogFileModel();
            logFile.Id = logFileId;
            _logFileRepo.GetById(logFileId).Returns(logFile);

            // execute
            _resetRequestAggregateCommand.Execute(logFileId);

            // assert
            _requestRepo.Received(1).GetByLogFile(logFileId);
            _logFileRepo.Received(1).GetById(logFileId);
            _projectRequestAggregateRepo.Received(1).GetByProject(logFile.ProjectId);
        }

        [Test]
        public void Execute_AggregatesSameAsUriStem_RequestNotUpdated()
        {
            int logFileId = new Random().Next(1, 1000);

            // setup
            RequestModel requestModel = DataHelper.CreateRequestModel(logFileId);
            requestModel.UriStem = "TEST";

            RequestModel[] requests = { requestModel };
            _requestRepo.GetByLogFile(logFileId).Returns(requests);

            LogFileModel logFile = DataHelper.CreateLogFileModel();
            logFile.Id = logFileId;
            _logFileRepo.GetById(logFileId).Returns(logFile);

            _requestAggregationService.GetAggregatedUriStem(Arg.Any<string>(), Arg.Any<IEnumerable<ProjectRequestAggregateModel>>()).Returns(requestModel.UriStem);

            // execute
            _resetRequestAggregateCommand.Execute(logFileId);

            // assert
            _requestAggregationService.Received(1).GetAggregatedUriStem(Arg.Any<string>(), Arg.Any<IEnumerable<ProjectRequestAggregateModel>>());
            _dbContext.Received(2).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_AggregatesDifferentToUriStem_RequestUpdated()
        {
            int logFileId = new Random().Next(1, 1000);

            // setup
            RequestModel[] requests = { DataHelper.CreateRequestModel(logFileId), DataHelper.CreateRequestModel(logFileId) };
            _requestRepo.GetByLogFile(logFileId).Returns(requests);

            LogFileModel logFile = DataHelper.CreateLogFileModel();
            logFile.Id = logFileId;
            _logFileRepo.GetById(logFileId).Returns(logFile);

            _requestAggregationService.GetAggregatedUriStem(Arg.Any<string>(), Arg.Any<IEnumerable<ProjectRequestAggregateModel>>()).Returns(Guid.NewGuid().ToString());

            // execute
            _resetRequestAggregateCommand.Execute(logFileId);

            // assert
            _requestRepo.Received(1).GetByLogFile(logFileId);
            _logFileRepo.Received(1).GetById(logFileId);
            _projectRequestAggregateRepo.Received(1).GetByProject(logFile.ProjectId);
            _requestAggregationService.Received(2).GetAggregatedUriStem(Arg.Any<string>(), Arg.Any<IEnumerable<ProjectRequestAggregateModel>>());
            _dbContext.Received(3).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }


    }
}
