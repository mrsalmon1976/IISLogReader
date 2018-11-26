using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
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

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class SetLogFileUnprocessedCommandTest
    {
        private ISetLogFileUnprocessedCommand _setLogFileUnprocessedCommand;

        private IDbContext _dbContext;
        private IJobRegistrationService _jobRegistrationService;

        [SetUp]
        public void SetLogFileUnprocessedCommand_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _jobRegistrationService = Substitute.For<IJobRegistrationService>();

            _setLogFileUnprocessedCommand = new SetLogFileUnprocessedCommand(_dbContext, _jobRegistrationService);
        }

        [TearDown]
        public void SetLogFileUnprocessedCommand_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_OnExecution_RunsQueriesAndRegistersJob()
        {
            int logFileId = new Random().Next(100, 1000);

            // execute
            _setLogFileUnprocessedCommand.Execute(logFileId);

            // assert
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
            _jobRegistrationService.Received(1).RegisterAggregateRequestJob(logFileId);
        }


        /// <summary>
        /// Tests that the insert actually works
        /// </summary>
        [Test]
        public void Execute_IntegrationTest_SQLite()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            List<W3CEvent> logEvents1 = null;
            List<W3CEvent> logEvents2 = new List<W3CEvent>();

            using (StreamReader logStream = new StreamReader(TestAsset.ReadTextStream(TestAsset.LogFile)))
            {
                logEvents1 = W3CEnumerable.FromStream(logStream).ToList();
                logEvents2.AddRange(logEvents1.GetRange(0, 10));
            }

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, new LogFileValidator());
                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                ISetLogFileUnprocessedCommand setLogFileUnprocessedCommand = new SetLogFileUnprocessedCommand(dbContext, _jobRegistrationService);

                // create the project first so we have one
                ProjectModel project = DataHelper.CreateProjectModel();
                ProjectModel savedProject = createProjectCommand.Execute(project);

                // create the log files
                LogFileModel logFile1 = DataHelper.CreateLogFileModel();
                logFile1.ProjectId = savedProject.Id;
                logFile1.IsProcessed = true;
                createLogFileCommand.Execute(logFile1);

                LogFileModel logFile2 = DataHelper.CreateLogFileModel();
                logFile2.ProjectId = savedProject.Id;
                logFile2.IsProcessed = true;
                createLogFileCommand.Execute(logFile2);


                // create the request batch
                createRequestBatchCommand.Execute(logFile1.Id, logEvents1);
                createRequestBatchCommand.Execute(logFile2.Id, logEvents2);

                // check that the log file is processed
                int processedCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM LogFiles WHERE ProjectId = @ProjectId AND IsProcessed = 1", new { ProjectId = savedProject.Id });
                Assert.AreEqual(2, processedCount);

                // execute for a single log file
                setLogFileUnprocessedCommand.Execute(logFile1.Id);

                processedCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM LogFiles WHERE ProjectId = @ProjectId AND IsProcessed = 1", new { ProjectId = savedProject.Id });
                Assert.AreEqual(1, processedCount);




            }

        }



    }
}
