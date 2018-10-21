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

namespace Test.IISLogReader.BLL.Validators
{
    [TestFixture]
    public class ProjectValidatorTest
    {
        private IProjectValidator _projectValidator;

        [SetUp]
        public void ProjectValidatorTest_SetUp()
        {
            _projectValidator = new ProjectValidator();
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidUserName_ReturnsFailure(string projectName)
        {
            ProjectModel model = DataHelper.CreateProjectModel();
            model.Name = projectName;

            ValidationResult result = _projectValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Project name"));
        }

        [Test]
        public void Validate_AllFieldsValid_ReturnsSuccess()
        {
            ProjectModel model = DataHelper.CreateProjectModel();

            ValidationResult result = _projectValidator.Validate(model);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Count);
        }


    }
}
