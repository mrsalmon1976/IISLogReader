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
using Tx.Windows;
using Test.IISLogReader.TestAssets;
using IISLogReader.BLL.Services;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class DeleteProjectCommandTest
    {
        private IDeleteProjectCommand _deleteProjectCommand;

        private IDbContext _dbContext;

        [SetUp]
        public void DeleteProjectCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();

            _deleteProjectCommand = new DeleteProjectCommand(_dbContext);
        }

        [TearDown]
        public void DeleteProjectCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }


        [Test]
        public void Execute_ValidationSucceeds_StatementsExecuted()
        {
            // execute
            _deleteProjectCommand.Execute(1);

            // assert
            _dbContext.Received(4).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
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
                dbContext.BeginTransaction();

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                IDeleteProjectCommand deleteProjectCommand = new DeleteProjectCommand(dbContext);


                // create the project first so we have one
                ProjectModel project = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project);

                // create the log file
                LogFileModel logFile = DataHelper.CreateLogFileModel(project.Id);
                DataHelper.InsertLogFileModel(dbContext, logFile);

                // create the request batch
                createRequestBatchCommand.Execute(logFile.Id, logEvents);

                //  run the delete command and check the end tables - should be 0 records
                deleteProjectCommand.Execute(project.Id);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM ProjectRequestAggregates");
                Assert.AreEqual(0, rowCount);

                rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Requests");
                Assert.AreEqual(0, rowCount);

                rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM LogFiles");
                Assert.AreEqual(0, rowCount);

                rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Projects");
                Assert.AreEqual(0, rowCount);


            }

        }



    }
}
