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
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Services;
using IISLogReader.BLL.Utils;
using IISLogReader.BLL.Lookup;
using System.Data;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class CreateLogFileCommandTest
    {
        private ICreateLogFileCommand _createLogFileCommand;

        private IDbContext _dbContext;
        private ILogFileValidator _logFileValidator;
        private ILogFileRepository _logFileRepo;
        private IJobRegistrationService _jobRegistrationService;
        private IFileUtils _fileUtils;

        [SetUp]
        public void CreateLogFileCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _logFileValidator = Substitute.For<ILogFileValidator>();
            _logFileRepo = Substitute.For<ILogFileRepository>();
            _jobRegistrationService = Substitute.For<IJobRegistrationService>();
            _fileUtils = Substitute.For<IFileUtils>();

            _createLogFileCommand = new CreateLogFileCommand(_dbContext, _logFileValidator, _logFileRepo, _jobRegistrationService, _fileUtils);
        }

        [TearDown]
        public void CreateLogFileCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_FileWithHashAlreadyExists_ThrowsException()
        {
            int projectId = new Random().Next(1, 1000);
            string filePath = Path.Combine(AppContext.BaseDirectory, "test.log");
            FileDetail fileDetail = new FileDetail();
            fileDetail.Hash = Guid.NewGuid().ToString();

            _fileUtils.GetFileHash(filePath).Returns(fileDetail);

            LogFileModel model = DataHelper.CreateLogFileModel();
            _logFileRepo.GetByHash(projectId, fileDetail.Hash).Returns(model);

            // execute
            TestDelegate del = () => _createLogFileCommand.Execute(projectId, filePath);

            // assert
            Assert.Throws<ValidationException>(del);

            _fileUtils.Received(1).GetFileHash(filePath);
            _logFileRepo.Received(1).GetByHash(projectId, fileDetail.Hash);

            // we shouldn't have even tried to validate or do the insert
            _logFileValidator.DidNotReceive().Validate(Arg.Any<LogFileModel>());
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationFails_ThrowsException()
        {
            int projectId = new Random().Next(1, 1000);
            string filePath = Path.Combine(AppContext.BaseDirectory, "test.log");
            FileDetail fileDetail = new FileDetail();
            fileDetail.Hash = Guid.NewGuid().ToString();

            _fileUtils.GetFileHash(filePath).Returns(fileDetail);

            _logFileValidator.Validate(Arg.Any<LogFileModel>()).Returns(new ValidationResult("error"));

            // execute
            TestDelegate del = () => _createLogFileCommand.Execute(projectId, filePath);

            // assert
            Assert.Throws<ValidationException>(del);

            _fileUtils.Received(1).GetFileHash(filePath);
            _logFileRepo.Received(1).GetByHash(projectId, fileDetail.Hash);
            _logFileValidator.Received(1).Validate(Arg.Any<LogFileModel>());

            // we shouldn't have tried to do the insert
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }


        [Test]
        public void Execute_ValidationSucceeds_RecordInsertedAndJobRegistered()
        {
            int projectId = new Random().Next(1, 1000);
            string filePath = Path.Combine(AppContext.BaseDirectory, "test.log");
            FileDetail fileDetail = new FileDetail();
            fileDetail.Length = new Random().Next(1000, 10000);
            fileDetail.Hash = Guid.NewGuid().ToString();
            fileDetail.Name = Guid.NewGuid().ToString();

            _fileUtils.GetFileHash(filePath).Returns(fileDetail);

            _logFileValidator.Validate(Arg.Any<LogFileModel>()).Returns(new ValidationResult());

            // execute
            LogFileModel result = _createLogFileCommand.Execute(projectId, filePath);

            // assert
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
            _dbContext.Received(1).ExecuteScalar<int>(Arg.Any<string>());
            _jobRegistrationService.Received(1).RegisterProcessLogFileJob(result.Id, filePath);

            Assert.AreEqual(projectId, result.ProjectId);
            Assert.AreEqual(fileDetail.Hash, result.FileHash);
            Assert.AreEqual(fileDetail.Length, result.FileLength);
            Assert.AreEqual(fileDetail.Name, result.FileName);
            Assert.AreEqual(-1, result.RecordCount);
            Assert.AreEqual(LogFileStatus.Processing, result.Status);

        }

        /// <summary>
        /// Tests that the insert actually works
        /// </summary>
        [Test]
        public void Execute_IntegrationTest_SQLite()
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            using (SQLiteDbContext dbContext = new SQLiteDbContext(dbPath))
            {
                dbContext.Initialise();
                dbContext.BeginTransaction();

                // create the project first so we have one
                ProjectModel project = DataHelper.CreateProjectModel();
                IProjectValidator projectValidator = new ProjectValidator();
                ILogFileRepository logFileRepo = new LogFileRepository(dbContext);

                int projectId = new Random().Next(1, 1000);
                string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".log");
                FileDetail fileDetail = new FileDetail();
                fileDetail.Length = new Random().Next(1000, 10000);
                fileDetail.Hash = Guid.NewGuid().ToString();
                fileDetail.Name = Guid.NewGuid().ToString();
                _fileUtils.GetFileHash(filePath).Returns(fileDetail);

                DataHelper.InsertProjectModel(dbContext, project);

                // create the log file
                LogFileModel logFile = DataHelper.CreateLogFileModel();
                logFile.ProjectId = project.Id;
                ILogFileValidator logFileValidator = new LogFileValidator();
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, logFileValidator, logFileRepo, _jobRegistrationService, _fileUtils);
                LogFileModel savedLogFile = createLogFileCommand.Execute(project.Id, filePath);

                Assert.Greater(savedLogFile.Id, 0);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM LogFiles");
                Assert.Greater(rowCount, 0);

                string fileName = dbContext.ExecuteScalar<string>("SELECT FileName FROM LogFiles WHERE Id = @Id", savedLogFile);
                Assert.AreEqual(savedLogFile.FileName, fileName);
                Assert.AreEqual(LogFileStatus.Processing, savedLogFile.Status);

            }

        }



    }
}
