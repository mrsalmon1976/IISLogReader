using AutoMapper;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Responses.Negotiation;
using Nancy.Testing;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using IISLogReader.BLL.Validators;
using IISLogReader.Modules;
using IISLogReader.Navigation;
using IISLogReader.ViewModels;
using IISLogReader.ViewModels.Login;
using IISLogReader.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Exceptions;

namespace Test.IISLogReader.Modules
{
    [TestFixture]
    public class UserModuleTest
    {
        private IUserRepository _userRepo;
        private IUserValidator _userValidator;
        private IPasswordProvider _passwordProvider;
        private ICreateUserCommand _createUserCommand;
        private IUpdateUserPasswordCommand _updateUserPasswordCommand;
        private IDeleteUserCommand _deleteUserCommand;

        [SetUp]
        public void UserModuleTest_SetUp()
        {
            _userRepo = Substitute.For<IUserRepository>();
            _userValidator = Substitute.For<IUserValidator>();
            _passwordProvider = Substitute.For<IPasswordProvider>();
            _createUserCommand = Substitute.For<ICreateUserCommand>();
            _updateUserPasswordCommand = Substitute.For<IUpdateUserPasswordCommand>();
            _deleteUserCommand = Substitute.For<IDeleteUserCommand>();

            Mapper.Initialize((cfg) =>
            {
                cfg.CreateMap<UserViewModel, UserModel>();
            });
        }

        [TearDown]
        public void UserModuleTest_TearDown()
        {
            Mapper.Reset();
        }

        #region ChangePassword Tests

        [TestCase("")]
        [TestCase("no")]
        public void ChangePassword_InvalidPassword_ReturnsError(string password)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var response = browser.Post(Actions.User.ChangePassword, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Password", password);
                with.FormValue("ConfirmPassword", "ConfirmPasswordIsOk");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Length); 
        }

        [Test]
        public void ChangePassword_PasswordDoesNotMatchConfirm_ReturnsError()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var response = browser.Post(Actions.User.ChangePassword, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Password", "IsValidPassword");
                with.FormValue("ConfirmPassword", "ButDoesNotMatch");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Length);
            Assert.IsTrue(result.Messages[0].Contains("do not match"));
        }

        [Test]
        public void ChangePassword_PasswordValid_UpdatesAndSaves()
        {
            const string newPassword = "IsValidPassword";
            const string oldPassword = "blahblahblah";
            string salt = Guid.NewGuid().ToString();
            string newHashedPassword = Guid.NewGuid().ToString();

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            UserModel user = new UserModel()
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName,
                Password = oldPassword
            };
            List<UserModel> users = new List<UserModel>() { user };
            _userRepo.GetAll().Returns(users);

            _passwordProvider.GenerateSalt().Returns(salt);
            _passwordProvider.HashPassword(newPassword, salt).Returns(newHashedPassword);

            // execute
            var response = browser.Post(Actions.User.ChangePassword, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Password", newPassword);
                with.FormValue("ConfirmPassword", newPassword);
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the result
            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Length);

            // make sure the user was updated and saved
            _updateUserPasswordCommand.Received(1).Execute(user.UserName, newPassword);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _userRepo.GetAll().Returns(new List<UserModel>() { });
            _userValidator.Validate(Arg.Any<UserModel>()).Returns(new ValidationResult());

            TestHelper.ValidateAuth(currentUser, browser, Actions.User.Delete, HttpMethod.Post, Claims.UserDelete);
        }

        [Test]
        public void Delete_Execute_DeletesUser()
        {
            Guid userId = Guid.NewGuid();

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.UserDelete };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var response = browser.Post(Actions.User.Delete, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Id", userId.ToString());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            _deleteUserCommand.Received(1).Execute(userId);

        }

        #endregion

        #region List Tests

        [Test]
        public void List_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _userRepo.GetAll().Returns(new List<UserModel>() { });

            TestHelper.ValidateAuth(currentUser, browser, Actions.User.List, HttpMethod.Get, Claims.UserList);
        }
        #endregion

        #region Save Tests

        [Test]
        public void Save_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _userRepo.GetAll().Returns(new List<UserModel>() { });
            _userValidator.Validate(Arg.Any<UserModel>()).Returns(new ValidationResult());

            TestHelper.ValidateAuth(currentUser, browser, Actions.User.Save, HttpMethod.Post, Claims.UserAdd);
        }

        [Test]
        public void Save_InvalidUser_ReturnsFailure()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.UserAdd };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _createUserCommand.When(x => x.Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Throw(new ValidationException("test"));

            // execute
            var response = browser.Post(Actions.User.Save, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Password", "password");
                with.FormValue("ConfirmPassword", "password");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the result
            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Length);
            _createUserCommand.Received(1).Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());

        }

        [Test]
        public void Save_PasswordsDoNotMatch_ReturnsFailure()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.UserAdd };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _userValidator.Validate(Arg.Any<UserModel>()).Returns(new ValidationResult());

            // execute
            var response = browser.Post(Actions.User.Save, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("ConfirmPassword", Guid.NewGuid().ToString());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the result
            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Messages[0].Contains("does not match"));
            _createUserCommand.DidNotReceive().Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());

        }

        [Test]
        public void Save_ValidUser_Saves()
        {
            // setup
            const string userName = "TestUser";
            const string password = "password1";

            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.UserAdd };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new UserModule(_userRepo, _createUserCommand, _updateUserPasswordCommand, _deleteUserCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );
            _userRepo.GetAll().Returns(new List<UserModel>());

            _userValidator.Validate(Arg.Any<UserModel>()).Returns(new ValidationResult());

            // execute
            var response = browser.Post(Actions.User.Save, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("UserName", userName);
                with.FormValue("Password", password);
                with.FormValue("ConfirmPassword", password);
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the result
            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Length);
            _createUserCommand.Received(1).Execute(userName, password, Arg.Any<string>());

        }

        #endregion



    }
}
