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
    public class UpdateUserPasswordCommandTest
    {
        private IUpdateUserPasswordCommand _updateUserPasswordCommand;

        private IDbContext _dbContext;
        private IUserRepository _userRepo;
        private IPasswordProvider _passwordProvider;

        [SetUp]
        public void UpdateUserPasswordCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _userRepo = Substitute.For<IUserRepository>();
            _passwordProvider = Substitute.For<IPasswordProvider>();

            _updateUserPasswordCommand = new UpdateUserPasswordCommand(_dbContext, _userRepo, _passwordProvider);
        }

        [TearDown]
        public void UpdateUserPasswordCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [TestCase(null)]
        [TestCase("")]
        public void Execute_PasswordNotSupplied_ValidationErrorThrown(string password)
        {
            LogFileModel model = DataHelper.CreateLogFileModel();

            // execute
            TestDelegate del = () => _updateUserPasswordCommand.Execute("test", password);

            // assert
            Assert.Throws<ValidationException>(del);

            // we shouldn't have even tried to do the insert
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_PasswordUpdated()
        {
            UserModel user = DataHelper.CreateUserModel();
            _userRepo.GetByUserName(user.UserName).Returns(user);
            string pwd = Guid.NewGuid().ToString();
            string salt = Guid.NewGuid().ToString();
            _passwordProvider.GenerateSalt().Returns(salt);

            // execute
            _updateUserPasswordCommand.Execute(user.UserName, pwd);

            // assert
            _passwordProvider.Received(1).HashPassword(pwd, salt);
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
                dbContext.BeginTransaction();

                // create the user
                UserModel user = DataHelper.CreateUserModel();
                IUserRepository userRepo = new UserRepository(dbContext);
                IUserValidator userValidator = new UserValidator(userRepo);

                IPasswordProvider passwordProvider = new PasswordProvider();
                ICreateUserCommand createUserCommand = new CreateUserCommand(dbContext, userValidator, passwordProvider);
                UserModel savedUser = createUserCommand.Execute(user.UserName, user.Password, user.Role);

                // reset the password
                string newPassword = Guid.NewGuid().ToString();
                IUpdateUserPasswordCommand updateCommand = new UpdateUserPasswordCommand(dbContext, userRepo, passwordProvider);
                updateCommand.Execute(savedUser.UserName, newPassword);

                // now fetch it again and check the password is good
                savedUser = userRepo.GetByUserName(user.UserName);
                Assert.IsTrue(passwordProvider.CheckPassword(newPassword, savedUser.Password));
            }

        }



    }
}
