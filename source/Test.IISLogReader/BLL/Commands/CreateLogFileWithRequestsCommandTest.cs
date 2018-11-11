using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using System.IO;
using IISLogReader.BLL.Repositories;
using Test.IISLogReader.TestAssets;
using IISLogReader.BLL.Exceptions;
using Tx.Windows;
using System.Security.Cryptography;
using IISLogReader.BLL.Services;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class CreateLogFileWithRequestsCommandTest
    {
        private ICreateLogFileWithRequestsCommand _createLogFileWithRequestsCommand;

        private IDbContext _dbContext;
        private ILogFileRepository _logFileRepo;
        private ICreateLogFileCommand _createLogFileCommand;
        private ICreateRequestBatchCommand _createRequestBatchCommand;
        private IJobRegistrationService _jobRegistrationService;

        [SetUp]
        public void AddProjectFileCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _logFileRepo = Substitute.For<ILogFileRepository>();
            _createLogFileCommand = Substitute.For<ICreateLogFileCommand>();
            _createRequestBatchCommand = Substitute.For<ICreateRequestBatchCommand>();
            _jobRegistrationService = Substitute.For<IJobRegistrationService>();

            _createLogFileWithRequestsCommand = new CreateLogFileWithRequestsCommand(_dbContext, _logFileRepo, _createLogFileCommand, _createRequestBatchCommand, _jobRegistrationService);
        }

        [TearDown]
        public void AddProjectFileCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.log");
        }

        [Test]
        public void Execute_InvalidFileFormat_ThrowsFileFormatException()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            File.WriteAllText(filePath, "This is not a valid IIS file");

            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                // execute
                TestDelegate del = () => _createLogFileWithRequestsCommand.Execute(projectId, fileName, stream);

                // assert
                Assert.Throws<FileFormatException>(del);

                // we shouldn't have even tried to load the project by hash
                _logFileRepo.DidNotReceive().GetByHash(Arg.Any<int>(), Arg.Any<string>());
            }
        }

        [Test]
        public void Execute_FileAlreadyAddedToProject_ThrowsValidationException()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            File.WriteAllText(filePath, "This is not a valid IIS file");

            LogFileModel logFileModel = DataHelper.CreateLogFileModel();
            logFileModel.ProjectId = projectId;

            _logFileRepo.GetByHash(projectId, Arg.Any<string>()).Returns(logFileModel);

            using (Stream stream = TestAsset.ReadTextStream(TestAsset.LogFile))
            {
                // execute
                TestDelegate del = () => _createLogFileWithRequestsCommand.Execute(projectId, fileName, stream);

                // assert
                Assert.Throws<ValidationException>(del);

                // we shouldn't have even tried to load the project by hash
                _logFileRepo.Received(1).GetByHash(projectId, Arg.Any<string>());
                _createLogFileCommand.DidNotReceive().Execute(Arg.Any<LogFileModel>());
            }
        }

        [Test]
        public void Execute_ValidFile_ExecutesCommands()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";

            LogFileModel logFileModel = DataHelper.CreateLogFileModel();
            _createLogFileCommand.Execute(Arg.Any<LogFileModel>()).Returns(logFileModel);

            using (Stream stream = TestAsset.ReadTextStream(TestAsset.LogFile))
            {
                // execute
                _createLogFileWithRequestsCommand.Execute(projectId, fileName, stream);

                // assert
                _logFileRepo.Received(1).GetByHash(projectId, Arg.Any<string>());
                _createLogFileCommand.Received(1).Execute(Arg.Any<LogFileModel>());
                _createRequestBatchCommand.Received(1).Execute(logFileModel.Id, Arg.Any<IEnumerable<W3CEvent>>());
                _jobRegistrationService.Received(1).RegisterAggregateRequestJob(logFileModel.Id);
            }
        }

        [Test]
        public void Execute_ValidFile_SavesLogFilePropertiesCorrectly()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string fileHash = Guid.NewGuid().ToString();

            LogFileModel logFileModel = DataHelper.CreateLogFileModel();
            _createLogFileCommand.Execute(Arg.Any<LogFileModel>()).Returns(logFileModel);

            // set up the intecept so we can check values
            LogFileModel savedLogFileModel = null;
            _createLogFileCommand.When(x => x.Execute(Arg.Any<LogFileModel>())).Do((c) => { savedLogFileModel = c.ArgAt<LogFileModel>(0); });

            using (Stream stream = TestAsset.ReadTextStream(TestAsset.LogFile))
            {
                // execute
                _createLogFileWithRequestsCommand.Execute(projectId, fileName, stream);

                using (var md5 = MD5.Create())
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var hash = md5.ComputeHash(stream);
                    fileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }

                // assert
                Assert.IsNotNull(savedLogFileModel);
                Assert.AreEqual(projectId, savedLogFileModel.ProjectId);
                Assert.AreEqual(fileHash, savedLogFileModel.FileHash);
                Assert.AreEqual(stream.Length, savedLogFileModel.FileLength);
                Assert.AreEqual(fileName, savedLogFileModel.FileName);
                Assert.Greater(savedLogFileModel.RecordCount, 1);

            }
        }


    }
}
