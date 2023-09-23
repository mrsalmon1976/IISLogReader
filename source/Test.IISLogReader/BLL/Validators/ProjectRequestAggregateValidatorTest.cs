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
    public class ProjectRequestAggregateValidatorTest
    {
        private IProjectRequestAggregateValidator _projectRequestAggregateValidator;

        [SetUp]
        public void ProjectRequestAggregateValidatorTest_SetUp()
        {
            _projectRequestAggregateValidator = new ProjectRequestAggregateValidator();
        }

        [TestCase(-1000)]
        [TestCase(-1)]
        [TestCase(0)]
        public void Validate_InvalidProjectId_ReturnsFailure(int projectId)
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();
            model.ProjectId = projectId;

            ValidationResult result = _projectRequestAggregateValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Project id"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidAggregateTarget_ReturnsFailure(string aggregateTarget)
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();
            model.AggregateTarget = aggregateTarget;

            ValidationResult result = _projectRequestAggregateValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Aggregate target"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidAggregateTargetButIsIgnored_ReturnsTrue(string aggregateTarget)
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();
            model.AggregateTarget = aggregateTarget;
            model.IsIgnored = true;

            ValidationResult result = _projectRequestAggregateValidator.Validate(model);

            Assert.IsTrue(result.Success);
        }


        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidRegularExpression_ReturnsFailure(string regularExpression)
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();
            model.RegularExpression = regularExpression;

            ValidationResult result = _projectRequestAggregateValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Regular expression"));
        }

        [Test]
        public void Validate_AllFieldsValid_ReturnsSuccess()
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();

            ValidationResult result = _projectRequestAggregateValidator.Validate(model);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Count);
        }


    }
}
