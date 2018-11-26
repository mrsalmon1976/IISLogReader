using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.IISLogReader.BLL.Validators
{
    [TestFixture]
    public class RequestValidatorTest
    {
        private IRequestValidator _requestValidator;

        [SetUp]
        public void RequestValidatorTest_SetUp()
        {
            _requestValidator = new RequestValidator();
        }

        [TestCase(-1000)]
        [TestCase(-1)]
        [TestCase(0)]
        public void Validate_InvalidLogFileId_ReturnsFailure(int logFileId)
        {
            RequestModel model = DataHelper.CreateRequestModel();
            model.LogFileId = logFileId;

            ValidationResult result = _requestValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Log file id"));
        }

        [Test]
        public void Validate_AllFieldsValid_ReturnsSuccess()
        {
            RequestModel model = DataHelper.CreateRequestModel();

            ValidationResult result = _requestValidator.Validate(model);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Count);
        }


    }
}
