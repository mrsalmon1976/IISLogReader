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
using IISLogReader.BLL.Security;
using IISLogReader.BLL.Repositories;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class CreateUserCommandTest
    {
        private ICreateUserCommand _createUserCommand;

        private IDbContext _dbContext;
        private IUserValidator _userValidator;
        private IPasswordProvider _passwordProvider;

        [SetUp]
        public void CreateUserCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _userValidator = Substitute.For<IUserValidator>();
            _passwordProvider = Substitute.For<IPasswordProvider>();

            _createUserCommand = new CreateUserCommand(_dbContext, _userValidator, _passwordProvider);
        }

        [TearDown]
        public void CreateUserCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_ValidationFails_ThrowsException()
        {
            UserModel model = DataHelper.CreateUserModel();

            _userValidator.Validate(Arg.Any<UserModel>()).Returns(new ValidationResult("error"));

            // execute
            TestDelegate del = () => _createUserCommand.Execute(model.UserName, model.Password, model.Role);
            
            // assert
            Assert.Throws<ValidationException>(del);

            // we shouldn't have even tried to do the insert
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
            _passwordProvider.DidNotReceive().HashPassword(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void Execute_ValidationSucceeds_RecordInserted()
        {
            UserModel model = DataHelper.CreateUserModel();

            _userValidator.Validate(Arg.Any<UserModel>()).Returns(new ValidationResult());

            // execute
            _createUserCommand.Execute(model.UserName, model.Password, model.Role);

            // assert
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_PasswordIsHashed()
        {
            UserModel model = DataHelper.CreateUserModel();
            string salt = Guid.NewGuid().ToString();
            string hashedPassword = Guid.NewGuid().ToString();

            _userValidator.Validate(Arg.Any<UserModel>()).Returns(new ValidationResult());
            _passwordProvider.GenerateSalt().Returns(salt);
            _passwordProvider.HashPassword(model.Password, salt).Returns(hashedPassword);

            // execute
            UserModel result = _createUserCommand.Execute(model.UserName, model.Password, model.Role);

            // assert
            _passwordProvider.Received(1).GenerateSalt();
            _passwordProvider.Received(1).HashPassword(model.Password, salt);
            Assert.AreEqual(model.UserName, result.UserName);
            Assert.AreEqual(hashedPassword, result.Password);
            Assert.AreEqual(model.Role, result.Role);
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

                // create the user
                UserModel user = DataHelper.CreateUserModel();

                IUserRepository userRepo = new UserRepository(dbContext);
                IUserValidator userValidator = new UserValidator(userRepo);
                IPasswordProvider passwordProvider = new PasswordProvider();

                ICreateUserCommand createUserCommand = new CreateUserCommand(dbContext, userValidator, passwordProvider);
                createUserCommand.Execute(user.UserName, user.Password, user.Role);

                UserModel savedUser = userRepo.GetByUserName(user.UserName);

                Assert.IsNotNull(savedUser);
                Assert.AreEqual(user.Role, savedUser.Role);
                Assert.IsTrue(passwordProvider.CheckPassword(user.Password, savedUser.Password));

            }

        }



    }
}
