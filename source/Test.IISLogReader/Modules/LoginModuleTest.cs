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
using IISLogReader.Modules;
using IISLogReader.Navigation;
using IISLogReader.ViewModels;
using IISLogReader.ViewModels.Login;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Repositories;

namespace Test.IISLogReader.Modules
{
    [TestFixture]
    public class LoginModuleTest
    {
        private IUserRepository _userRepo;
        private IPasswordProvider _passwordProvider;

        [SetUp]
        public void LoginModuleTest_SetUp()
        {
            _passwordProvider = Substitute.For<IPasswordProvider>();
            _userRepo = Substitute.For<IUserRepository>();
        }

        #region LoginGet Tests

        [Test]
        public void LoginGet_UserLoggedIn_RedirectsToDashboard()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = CreateBrowser(currentUser);

            // execute
            var response = browser.Get(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            response.ShouldHaveRedirectedTo(Actions.Dashboard.Default);
        }

        [Test]
        public void LoginGet_NoReturnUrl_DefaultsToDashboard()
        {
            // setup
            var browser = CreateBrowser(null);

            // execute
            var response = browser.Get(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                //with.FormsAuth(Guid.NewGuid(), new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            response.Body["#returnurl"]
                .ShouldExistOnce()
                .And.ShouldContainAttribute("value", Actions.Dashboard.Default);
        }

        [Test]
        public void LoginGet_WithReturnUrl_SetsReturnUrlFormValue()
        {
            // setup
            var browser = CreateBrowser(null);

            // execute
            var response = browser.Get(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                with.Query("returnurl", "/test");
                //with.FormsAuth(Guid.NewGuid(), new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            response.Body["#returnurl"]
                .ShouldExistOnce()
                .And.ShouldContainAttribute("value", "/test");
        }

        #endregion

        #region LoginPost Tests

        [Test]
        public void LoginPost_NoUserName_LoginFailsWithoutCheck()
        {
            // setup
            var browser = CreateBrowser(null);

            //var browser = new Browser(with => with.Module(new LoginModule(_userStore, _passwordProvider)));

            // execute
            var response = browser.Post(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                with.FormValue("Password", "password");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _passwordProvider.DidNotReceive().CheckPassword(Arg.Any<string>(), Arg.Any<string>());

            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void LoginPost_NoPassword_LoginFailsWithoutCheck()
        {
            // setup
            var browser = CreateBrowser(null);

            // execute
            var response = browser.Post(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                with.FormValue("UserName", "admin");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _passwordProvider.DidNotReceive().CheckPassword(Arg.Any<string>(), Arg.Any<string>());

            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void LoginPost_UserNotFound_LoginFails()
        {
            // setup
            var browser = CreateBrowser(null);

            // execute
            var response = browser.Post(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                with.FormValue("UserName", "admin");
                with.FormValue("Password", "password");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _passwordProvider.DidNotReceive().CheckPassword(Arg.Any<string>(), Arg.Any<string>());

            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void LoginPost_UserFoundButPasswordIncorrect_LoginFails()
        {
            // setup
            UserModel user = new UserModel()
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Password = "dsdsdds"
            };
            _userRepo.GetByUserName(user.UserName).Returns(user);

            _passwordProvider.CheckPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var browser = CreateBrowser(null);

            // execute
            var response = browser.Post(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                with.FormValue("UserName", "admin");
                with.FormValue("Password", "password");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _passwordProvider.Received(1).CheckPassword(Arg.Any<string>(), Arg.Any<string>());

            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);

            _userRepo.Received(1).GetByUserName(user.UserName);
            _passwordProvider.Received(1).CheckPassword("password", user.Password);
        }

        [Test]
        public void LoginPost_ValidLogin_LoginSucceeds()
        {
            // setup
            UserModel user = new UserModel()
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Password = "password"
            };
            _userRepo.GetByUserName(user.UserName).Returns(user);

            _passwordProvider.CheckPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var browser = new Browser((bootstrapper) =>
                            bootstrapper.Module(new LoginModule(_userRepo, _passwordProvider))
                                .RootPathProvider(new TestRootPathProvider())
                                .RequestStartup((container, pipelines, context) =>
                                {
                                    container.Register<IUserRepository>(Substitute.For<IUserRepository>());
                                    container.Register<IUserMapper, UserMapper>();
                                    var formsAuthConfiguration = new FormsAuthenticationConfiguration()
                                    {
                                        RedirectUrl = "~/login",
                                        UserMapper = container.Resolve<IUserMapper>(),
                                    };
                                    FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
                                })
                            );

            // execute
            var response = browser.Post(Actions.Login.Default, (with) =>
            {
                with.HttpRequest();
                with.FormValue("UserName", "admin");
                with.FormValue("Password", "password");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.IsNotNull(response.Headers["Location"]);
            Assert.IsNotEmpty(response.Headers["Location"]);
            Assert.IsEmpty(response.Body.AsString());
            _passwordProvider.Received(1).CheckPassword("password", user.Password);
            _userRepo.Received(1).GetByUserName(user.UserName);
        }

        #endregion

        #region Private Methods

        private Browser CreateBrowser(UserIdentity currentUser)
        {
            var browser = new Browser((bootstrapper) =>
                            bootstrapper.Module(new LoginModule(_userRepo, _passwordProvider))
                                .RootPathProvider(new TestRootPathProvider())
                                .RequestStartup((container, pipelines, context) => {
                                    context.CurrentUser = currentUser;
                                })
                            );
            return browser;
        }

        #endregion


    }
}
