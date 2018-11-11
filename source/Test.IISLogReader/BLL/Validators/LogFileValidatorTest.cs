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

namespace Test.IISLogReader.BLL.Validators
{
    [TestFixture]
    public class LogFileValidatorTest
    {
        private ILogFileValidator _logFileValidator;

        [SetUp]
        public void LogFileValidatorTest_SetUp()
        {
            _logFileValidator = new LogFileValidator();
        }

        [TestCase(-1000)]
        [TestCase(-1)]
        [TestCase(0)]
        public void Validate_InvalidProjectId_ReturnsFailure(int projectId)
        {
            LogFileModel model = DataHelper.CreateLogFileModel();
            model.ProjectId = projectId;

            ValidationResult result = _logFileValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Project id"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidFileName_ReturnsFailure(string logFileName)
        {
            LogFileModel model = DataHelper.CreateLogFileModel();
            model.FileName = logFileName;

            ValidationResult result = _logFileValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("File name"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidFileHash_ReturnsFailure(string fileHash)
        {
            LogFileModel model = DataHelper.CreateLogFileModel();
            model.FileHash = fileHash;

            ValidationResult result = _logFileValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("File hash"));
        }

        [TestCase(-1000)]
        [TestCase(-1)]
        [TestCase(0)]
        public void Validate_InvalidFileLength_ReturnsFailure(int fileLength)
        {
            LogFileModel model = DataHelper.CreateLogFileModel();
            model.FileLength = fileLength;

            ValidationResult result = _logFileValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("File length"));
        }

        [TestCase(-1000)]
        [TestCase(-1)]
        [TestCase(0)]
        public void Validate_InvalidRecordCount_ReturnsFailure(int recourdCount)
        {
            LogFileModel model = DataHelper.CreateLogFileModel();
            model.RecordCount = recourdCount;

            ValidationResult result = _logFileValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Record count"));
        }


        [Test]
        public void Validate_AllFieldsValid_ReturnsSuccess()
        {
            LogFileModel model = DataHelper.CreateLogFileModel();

            ValidationResult result = _logFileValidator.Validate(model);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Count);
        }


    }
}
