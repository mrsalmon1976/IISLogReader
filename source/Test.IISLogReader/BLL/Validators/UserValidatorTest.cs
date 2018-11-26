using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Repositories;

namespace Test.IISLogReader.BLL.Validators
{
    [TestFixture]
    public class UserValidatorTest
    {
        private IUserValidator _userValidator;

        private IUserRepository _userRepo;

        [SetUp]
        public void UserValidatorTest_SetUp()
        {
            _userRepo = Substitute.For<IUserRepository>();

            _userValidator = new UserValidator(_userRepo);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidUserName_ReturnsFailure(string userName)
        {
            UserModel model = DataHelper.CreateUserModel();
            model.UserName = userName;

            ValidationResult result = _userValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("User name"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidUserPassword_ReturnsFailure(string password)
        {
            UserModel model = DataHelper.CreateUserModel();
            model.Password = password;

            ValidationResult result = _userValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Password"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void Validate_InvalidRole_ReturnsFailure(string role)
        {
            UserModel model = DataHelper.CreateUserModel();
            model.Role = role;

            ValidationResult result = _userValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("Role"));
        }

        [Test]
        public void Validate_UserAlreadyExists_ReturnsFailure()
        {
            UserModel model = DataHelper.CreateUserModel();

            _userRepo.GetByUserName(model.UserName).Returns(new UserModel());

            ValidationResult result = _userValidator.Validate(model);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsTrue(result.Messages[0].Contains("already exists"));
        }

        [Test]
        public void Validate_AllFieldsValid_ReturnsSuccess()
        {
            UserModel model = DataHelper.CreateUserModel();

            ValidationResult result = _userValidator.Validate(model);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Count);
        }


    }
}
