using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Security;
using IISLogReader.BLL.Services;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.IISLogReader.BLL.Services
{
    [TestFixture]
    public class UserServiceTest
    {
        private IUserService _userService;
        private IUserRepository _userRepo;
        private ICreateUserCommand _createUserCommand;

        [SetUp]
        public void UserServiceTest_SetUp()
        {
            _userRepo = Substitute.For<IUserRepository>();
            _createUserCommand = Substitute.For<ICreateUserCommand>();
            _userService = new UserService(_userRepo, _createUserCommand);
        }

        #region InitialiseAdminUser Tests

        [Test]
        public void InitialiseAdminUser_AdminUserExists_ReturnsExistingUser()
        {
            UserModel user = DataHelper.CreateUserModel();
            user.UserName = UserService.AdminUserName;

            _userRepo.GetByUserName(UserService.AdminUserName).Returns(user);

            // execute
            UserModel result = _userService.InitialiseAdminUser();
            Assert.IsNotNull(result);

            _userRepo.Received(1).GetByUserName(user.UserName);
            _createUserCommand.DidNotReceive().Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void InitialiseAdminUser_AdminUserDoesNotExist_ReturnsNewUser()
        {
            UserModel user = new UserModel();
            user.UserName = UserService.AdminUserName;
            user.Password = UserService.AdminDefaultPassword;
            user.Role = Roles.Admin;

            _createUserCommand.Execute(user.UserName, user.Password, user.Role).Returns(user);

            // execute
            UserModel result = _userService.InitialiseAdminUser();
            Assert.IsNotNull(result);

            _userRepo.Received(1).GetByUserName(user.UserName);
            _createUserCommand.Received(1).Execute(user.UserName, user.Password, user.Role);
        }


        #endregion
    }
}
