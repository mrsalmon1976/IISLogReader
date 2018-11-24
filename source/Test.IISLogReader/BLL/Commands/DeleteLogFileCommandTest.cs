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
using Tx.Windows;
using Test.IISLogReader.TestAssets;
using IISLogReader.BLL.Services;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class DeleteLogFileCommandTest
    {
        private IDeleteLogFileCommand _deleteLogFileCommand;

        private IDbContext _dbContext;

        [SetUp]
        public void DeleteLogFileCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();

            _deleteLogFileCommand = new DeleteLogFileCommand(_dbContext);
        }

        [TearDown]
        public void DeleteLogFileCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }


        [Test]
        public void Execute_ValidationSucceeds_StatementsExecuted()
        {
            // execute
            _deleteLogFileCommand.Execute(1);

            // assert
            _dbContext.Received(2).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
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

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, new LogFileValidator());
                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                IDeleteLogFileCommand deleteLogFileCommand = new DeleteLogFileCommand(dbContext);


                // create the project first so we have one
                ProjectModel project = DataHelper.CreateProjectModel();

                // create 2 the log files
                LogFileModel logFile1 = DataHelper.CreateLogFileModel();
                logFile1.ProjectId = project.Id;
                createLogFileCommand.Execute(logFile1);

                LogFileModel logFile2 = DataHelper.CreateLogFileModel();
                logFile2.ProjectId = project.Id;
                createLogFileCommand.Execute(logFile2);

                // create the request batch
                createRequestBatchCommand.Execute(logFile1.Id, logEvents);
                createRequestBatchCommand.Execute(logFile2.Id, logEvents);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Requests WHERE LogFileId = @LogFileId", new { LogFileId = logFile1.Id });
                Assert.AreEqual(logEvents.Count, rowCount);

                //  run the delete command 
                deleteLogFileCommand.Execute(logFile1.Id);

                // there should be no requests for logFile1, but requests for logFile2 should still exist
                rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Requests WHERE LogFileId = @LogFileId", new { LogFileId = logFile1.Id });
                Assert.AreEqual(0, rowCount);
                rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Requests WHERE LogFileId = @LogFileId", new { LogFileId = logFile2.Id });
                Assert.AreEqual(logEvents.Count, rowCount);

                rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM LogFiles");
                Assert.AreEqual(1, rowCount);

            }

        }



    }
}
