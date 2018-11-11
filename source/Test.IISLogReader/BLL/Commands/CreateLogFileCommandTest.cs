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

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class CreateLogFileCommandTest
    {
        private ICreateLogFileCommand _createLogFileCommand;

        private IDbContext _dbContext;
        private ILogFileValidator _logFileValidator;

        [SetUp]
        public void CreateLogFileCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _logFileValidator = Substitute.For<ILogFileValidator>();

            _createLogFileCommand = new CreateLogFileCommand(_dbContext, _logFileValidator);
        }

        [TearDown]
        public void CreateLogFileCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_ValidationFails_ThrowsException()
        {
            LogFileModel model = DataHelper.CreateLogFileModel();

            _logFileValidator.Validate(model).Returns(new ValidationResult("error"));

            // execute
            TestDelegate del = () => _createLogFileCommand.Execute(model);
            
            // assert
            Assert.Throws<ValidationException>(del);

            // we shouldn't have even tried to do the insert
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_RecordInserted()
        {
            LogFileModel model = DataHelper.CreateLogFileModel();

            _logFileValidator.Validate(model).Returns(new ValidationResult());

            // execute
            _createLogFileCommand.Execute(model);

            // assert
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that the insert actually works
        /// </summary>
        [Test]
        public void Execute_IntegrationTest_SQLite()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
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
                ILogFileValidator logFileValidator = new LogFileValidator();
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, logFileValidator);
                LogFileModel savedLogFile = createLogFileCommand.Execute(logFile);

                Assert.Greater(savedLogFile.Id, 0);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM LogFiles");
                Assert.Greater(rowCount, 0);

                string fileName = dbContext.ExecuteScalar<string>("SELECT FileName FROM LogFiles WHERE Id = @Id", savedLogFile);
                Assert.AreEqual(savedLogFile.FileName, fileName);
                Assert.IsFalse(savedLogFile.IsProcessed);

            }

        }



    }
}
