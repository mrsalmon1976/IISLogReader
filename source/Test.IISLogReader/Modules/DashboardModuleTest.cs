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
using IISLogReader.BLL.Data.Stores;
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
using IISLogReader.ViewModels.Project;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Repositories;
using System.IO;
using System.Net.Http;

namespace Test.IISLogReader.Modules
{
    [TestFixture]
    public class DashboardModuleTest
    {
        private IDbContext _dbContext;

        [SetUp]
        public void LogFileModuleTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
        }

        [TearDown]
        public void LogFileModuleTest_TearDown()
        {
            Mapper.Reset();
        }

        #region Default

        [Test]
        public void Default_HasProjectEditClaim_ShowsAdminTextAndButtons()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new DashboardModule())
                    .RootPathProvider(new TestRootPathProvider())
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                        context.ViewBag.CurrentUserName = currentUser.UserName;
                        context.ViewBag.Scripts = new List<string>();
                        context.ViewBag.Claims = new List<string>();
                        context.ViewBag.Projects = new List<ProjectModel>();
                    })
                );

            // execute
            var response = browser.Get(Actions.Dashboard.Default, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response.Body["#dashboard-text"].AllShouldContain("Group your log files");
            response.Body["#dashboard-buttons-row"].ShouldExist();
        }

        [Test]
        public void Default_HasProjectEditClaim_DoesNotRenderAdminTextAndButtons()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new DashboardModule())
                    .RootPathProvider(new TestRootPathProvider())
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                        context.ViewBag.CurrentUserName = currentUser.UserName;
                        context.ViewBag.Scripts = new List<string>();
                        context.ViewBag.Claims = new List<string>();
                        context.ViewBag.Projects = new List<ProjectModel>();
                    })
                );

            // execute
            var response = browser.Get(Actions.Dashboard.Default, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response.Body["#dashboard-text"].AllShouldContain("Speak to an administrator");
            response.Body["#dashboard-buttons-row"].ShouldNotExist();
        }


        #endregion







    }
}
