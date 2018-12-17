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
using IISLogReader.BLL.Lookup;
using SystemWrapper.IO;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class ProcessLogFileCommandTest
    {
        private IProcessLogFileCommand _processLogFileCommand;

        private IDbContext _dbContext;
        private ILogFileRepository _logFileRepo;
        private ICreateRequestBatchCommand _createRequestBatchCommand;
        private IJobRegistrationService _jobRegistrationService;
        private IFileWrap _fileWrap; 

        [SetUp]
        public void ProcessLogFileCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _logFileRepo = Substitute.For<ILogFileRepository>();
            _createRequestBatchCommand = Substitute.For<ICreateRequestBatchCommand>();
            _jobRegistrationService = Substitute.For<IJobRegistrationService>();
            _fileWrap = Substitute.For<IFileWrap>();

            _processLogFileCommand = new ProcessLogFileCommand(_dbContext, _logFileRepo, _createRequestBatchCommand, _jobRegistrationService, _fileWrap);
        }

        [TearDown]
        public void ProcessLogFileCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.log");
        }

        [Test]
        public void Execute_InvalidFileFormat_MarksFileAsError()
        {
            LogFileModel logFile = DataHelper.CreateLogFileModel();
            LogFileModel savedLogFile = null;

            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            File.WriteAllText(filePath, "This is not a valid IIS file");

            _logFileRepo.GetById(logFile.Id).Returns(logFile);

            _dbContext.When(x => x.ExecuteNonQuery(Arg.Any<String>(), Arg.Any<LogFileModel>())).Do((c) => {
                savedLogFile = c.ArgAt<LogFileModel>(1);
            });

            // execute
            _processLogFileCommand.Execute(logFile.Id, filePath);

            // assert
            _createRequestBatchCommand.DidNotReceive().Execute(Arg.Any<int>(), Arg.Any<IEnumerable<W3CEvent>>());
            _jobRegistrationService.DidNotReceive().RegisterProcessLogFileJob(Arg.Any<int>(), Arg.Any<string>());
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<String>(), Arg.Any<LogFileModel>());

            Assert.IsNotNull(savedLogFile);
            Assert.AreEqual(LogFileStatus.Error, savedLogFile.Status);
            Assert.IsTrue(savedLogFile.ErrorMsg.Contains("File is not a valid IIS"));
        }


        [Test]
        public void Execute_ValidLogFile_ProcessesFileAndRegistersJob()
        {
            LogFileModel logFile = DataHelper.CreateLogFileModel();

            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);

            // save the text file
            using (Stream stream = TestAsset.ReadTextStream(TestAsset.LogFile))
            {
                var fileStream = File.Create(filePath);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Close();
            }


            _logFileRepo.GetById(logFile.Id).Returns(logFile);

            // execute
            _processLogFileCommand.Execute(logFile.Id, filePath);

            // assert
            _createRequestBatchCommand.Received(1).Execute(logFile.Id, Arg.Any<IEnumerable<W3CEvent>>());
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
            _jobRegistrationService.Received(1).RegisterResetProcessedLogFileJob(logFile.Id);
            _fileWrap.Received(1).Delete(filePath);

        }



    }
}
