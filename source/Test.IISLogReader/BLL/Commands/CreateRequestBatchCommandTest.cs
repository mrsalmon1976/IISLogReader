using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Data.Models;
using IISLogReader.BLL.Data.Stores;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Commands.Project;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Exceptions;
using System.IO;
using Tx.Windows;
using Test.IISLogReader.TestAssets;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class CreateRequestBatchCommandTest
    {
        private ICreateRequestBatchCommand _createRequestBatchCommand;

        private IDbContext _dbContext;
        private IRequestValidator _requestValidator;

        [SetUp]
        public void CreateRequestBatchCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _requestValidator = Substitute.For<IRequestValidator>();

            _createRequestBatchCommand = new CreateRequestBatchCommand(_dbContext, _requestValidator);
        }

        [TearDown]
        public void CreateRequestBatchCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_ValidationFails_ThrowsException()
        {
            _requestValidator.Validate(Arg.Any<RequestModel>()).Returns(new ValidationResult("error"));

            using (StreamReader logStream = new StreamReader(TestAsset.ReadTextStream(TestAsset.LogFile)))
            {
                var logEvents = W3CEnumerable.FromStream(logStream).ToList();

                // execute
                TestDelegate del = () => _createRequestBatchCommand.Execute(0, logEvents);

                // assert
                Assert.Throws<ValidationException>(del);

                // we shouldn't have even tried to do the insert
                _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
            }
        }

        [Test]
        public void Execute_ValidationSucceeds_BatchInserted()
        {
            _requestValidator.Validate(Arg.Any<RequestModel>()).Returns(new ValidationResult());

            using (StreamReader logStream = new StreamReader(TestAsset.ReadTextStream(TestAsset.LogFile)))
            {
                var logEvents = W3CEnumerable.FromStream(logStream).ToList();
                int eventCount = logEvents.Count;
                Assert.Greater(eventCount, 1);

                // execute
                _createRequestBatchCommand.Execute(1, logEvents);

                // assert
                _requestValidator.Received(eventCount).Validate(Arg.Any<RequestModel>());
                _dbContext.Received(eventCount).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
            }
        }

        /// <summary>
        /// Tests that the insert actually works
        /// </summary>
        [Test]
        public void Execute_IntegrationTest_SQLite()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            List<W3CEvent> logEvents = null;

            using (StreamReader logStream = new StreamReader(TestAsset.ReadTextStream(TestAsset.LogFile)))
            {
                logEvents = W3CEnumerable.FromStream(logStream).ToList();
            }

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                // create the project first so we have one
                ProjectModel project = DataHelper.CreateProjectModel();
                IProjectValidator projectValidator = new ProjectValidator();
                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, projectValidator);
                ProjectModel savedProject = createProjectCommand.Execute(project);

                // create the log file
                LogFileModel logFile = DataHelper.CreateLogFileModel();
                logFile.ProjectId = savedProject.Id;
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, new LogFileValidator());
                LogFileModel savedLogFile = createLogFileCommand.Execute(logFile);

                // create the request batch
                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                createRequestBatchCommand.Execute(savedLogFile.Id, logEvents);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Requests");
                Assert.AreEqual(logEvents.Count, rowCount);

            }

        }



    }
}
