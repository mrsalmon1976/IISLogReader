using AutoMapper;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Responses.Negotiation;
using Nancy.Testing;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Data.Models;
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
using IISLogReader.BLL.Commands.Project;
using IISLogReader.BLL.Data.Repositories;
using System.IO;

namespace Test.IISLogReader.Modules
{
    [TestFixture]
    public class ProjectModuleTest
    {
        private IDbContext _dbContext;
        private ICreateProjectCommand _createProjectCommand;
        private IProjectValidator _projectValidator;
        private IProjectRepository _projectRepo;
        private ILogFileRepository _logFileRepo;

        [SetUp]
        public void ProjectModuleTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _createProjectCommand = Substitute.For<ICreateProjectCommand>();
            _projectValidator = Substitute.For<IProjectValidator>();
            _projectRepo = Substitute.For<IProjectRepository>();
            _logFileRepo = Substitute.For<ILogFileRepository>();

            Mapper.Initialize((cfg) =>
            {
                cfg.CreateMap<ProjectFormViewModel, ProjectModel>();
            });
        }

        [TearDown]
        public void ProjectModuleTest_TearDown()
        {
            Mapper.Reset();
        }

        #region Files

        [TestCase("abc")]
        [TestCase("111abc")]
        public void Files_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.Files(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _logFileRepo.DidNotReceive().GetByProject(Arg.Any<int>());

        }

        [Test]
        public void Files_ValidProjectId_GetsLogFilesFromDatabase()
        {
            int projectId = new Random().Next(1, 1000);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );
            LogFileModel log1 = DataHelper.CreateLogFileModel();
            LogFileModel log2 = DataHelper.CreateLogFileModel();
            LogFileModel log3 = DataHelper.CreateLogFileModel();
            _logFileRepo.GetByProject(projectId).Returns(new LogFileModel[] { log1, log2, log3 });

            // execute
            var url = Actions.Project.Files(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _logFileRepo.Received(1).GetByProject(projectId);

            IEnumerable<LogFileModel> result = JsonConvert.DeserializeObject<IEnumerable<LogFileModel>>(response.Body.AsString());
            Assert.AreEqual(3, result.Count());

        }

        #endregion

        #region Save Tests

        [Test]
        public void Save_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _projectValidator.Validate(Arg.Any<ProjectModel>()).Returns(new ValidationResult());

            foreach (string claim in Claims.AllClaims)
            {

                currentUser.Claims = new string[] { claim };

                // execute
                var response = browser.Post(Actions.Project.Save, (with) =>
                {
                    with.HttpRequest();
                    with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                    //with.FormValue("id", connectionId.ToString());
                });

                // assert
                if (claim == Claims.ProjectSave)
                {
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                }
                else
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
                }
            }

        }

        [Test]
        public void Save_InvalidProject_ReturnsFailure()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectSave };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _projectValidator.Validate(Arg.Any<ProjectModel>()).Returns(new ValidationResult("error"));

            // execute
            var response = browser.Post(Actions.Project.Save, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Name", "password");
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the result
            BasicResult result = JsonConvert.DeserializeObject<BasicResult>(response.Body.AsString());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Length);

            // the project should not have been added
            _createProjectCommand.DidNotReceive().Execute(Arg.Any<ProjectModel>());

        }

        [Test]
        public void Save_ValidProject_Saves()
        {
            // setup
            const string projectName = "TestProject";

            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectSave };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _projectValidator.Validate(Arg.Any<ProjectModel>()).Returns(new ValidationResult());

            // execute
            var response = browser.Post(Actions.Project.Save, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Name", projectName);
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the result
            ValidationResult result = JsonConvert.DeserializeObject<ValidationResult>(response.Body.AsString());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Messages.Count);

            // the project should have been added
            _createProjectCommand.Received(1).Execute(Arg.Any<ProjectModel>());
            _dbContext.Received(1).BeginTransaction();
            _dbContext.Received(1).Commit();
        }

        #endregion

        #region View Tests

        [TestCase("_z")]
        [TestCase("a1")]
        [TestCase("ccc")]
        public void View_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.View(projectId);
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        }


        [Test]
        public void View_ProjectDoesNotExist_Returns404()
        {
            int projectId = new Random().Next(1, 100);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.View(projectId);
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            _projectRepo.Received(1).GetById(projectId);

        }

        [Test]
        public void View_NotLoggedIn_Returns401()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
                    .RequestStartup((container, pipelines, context) => {
                    })
                );

            // execute
            var url = Actions.Project.View(1);
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
            });

            // assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

        }

        [Test]
        public void View_ProjectIsValid_ReturnsModel()
        {

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            ProjectModel project = DataHelper.CreateProjectModel();
            var url = Actions.Project.View(project.Id);

            _projectRepo.GetById(project.Id).Returns(project);

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _projectRepo, _logFileRepo))
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
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _projectRepo.Received(1).GetById(project.Id);

            response.Body["#projectId"]
                .ShouldExistOnce()
                .And.ShouldContainAttribute("value", project.Id.ToString());

            response.Body[".page-header"]
                .ShouldExistOnce()
                .And.ShouldContain(project.Name);
                
        }

        #endregion

    }
}
