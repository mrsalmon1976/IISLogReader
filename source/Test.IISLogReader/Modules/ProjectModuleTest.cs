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
using IISLogReader.ViewModels.Project;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Repositories;
using System.IO;
using System.Net.Http;

namespace Test.IISLogReader.Modules
{
    [TestFixture]
    public class ProjectModuleTest
    {
        private IDbContext _dbContext;
        private ICreateProjectCommand _createProjectCommand;
        private IDeleteProjectCommand _deleteProjectCommand;
        private IProjectValidator _projectValidator;
        private IProjectRepository _projectRepo;
        private ILogFileRepository _logFileRepo;
        private IRequestRepository _requestRepo;
        private IProjectRequestAggregateRepository _projectRequestAggregateRepo;

        [SetUp]
        public void ProjectModuleTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _createProjectCommand = Substitute.For<ICreateProjectCommand>();
            _deleteProjectCommand = Substitute.For<IDeleteProjectCommand>();
            _projectValidator = Substitute.For<IProjectValidator>();
            _projectRepo = Substitute.For<IProjectRepository>();
            _logFileRepo = Substitute.For<ILogFileRepository>();
            _requestRepo = Substitute.For<IRequestRepository>();
            _projectRequestAggregateRepo = Substitute.For<IProjectRequestAggregateRepository>();

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

        #region Aggregates

        [TestCase("abc")]
        [TestCase("111abc")]
        public void Aggregates_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.Aggregates(projectId);
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
        public void Aggregates_ValidProjectId_GetsAggregatesFromDatabase()
        {
            int projectId = new Random().Next(1, 1000);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );
            ProjectRequestAggregateModel pra1 = DataHelper.CreateProjectRequestAggregateModel();
            ProjectRequestAggregateModel pra2 = DataHelper.CreateProjectRequestAggregateModel();
            ProjectRequestAggregateModel pra3 = DataHelper.CreateProjectRequestAggregateModel();
            _projectRequestAggregateRepo.GetByProject(projectId).Returns(new ProjectRequestAggregateModel[] { pra1, pra2, pra3 });

            // execute
            var url = Actions.Project.Aggregates(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _projectRequestAggregateRepo.Received(1).GetByProject(projectId);

            IEnumerable<ProjectRequestAggregateModel> result = JsonConvert.DeserializeObject<IEnumerable<ProjectRequestAggregateModel>>(response.Body.AsString());
            Assert.AreEqual(3, result.Count());

        }

        #endregion

        #region AvgLoadTimes

        [TestCase("abc")]
        [TestCase("111abc")]
        public void AvgLoadTimes_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.AvgLoadTimes(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _requestRepo.DidNotReceive().GetPageLoadTimes(Arg.Any<int>());

        }

        [Test]
        public void AvgLoadTimes_ValidProjectId_GetsAverageLoadTimesFromDatabase()
        {
            int projectId = new Random().Next(1, 1000);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );
            RequestPageLoadTimeModel loadTime1 = DataHelper.CreateRequestPageLoadTimeModel();
            RequestPageLoadTimeModel loadTime2 = DataHelper.CreateRequestPageLoadTimeModel();
            RequestPageLoadTimeModel loadTime3 = DataHelper.CreateRequestPageLoadTimeModel();
            _requestRepo.GetPageLoadTimes(projectId).Returns(new RequestPageLoadTimeModel[] { loadTime1, loadTime2, loadTime3 });

            // execute
            var url = Actions.Project.AvgLoadTimes(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _requestRepo.Received(1).GetPageLoadTimes(projectId);

            IEnumerable<RequestPageLoadTimeModel> result = JsonConvert.DeserializeObject<IEnumerable<RequestPageLoadTimeModel>>(response.Body.AsString());
            Assert.AreEqual(3, result.Count());

        }

        #endregion

        #region Delete

        [Test]
        public void Delete_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _projectValidator.Validate(Arg.Any<ProjectModel>()).Returns(new ValidationResult());
            _createProjectCommand.Execute(Arg.Any<ProjectModel>()).Returns(DataHelper.CreateProjectModel());

            TestHelper.ValidateAuth(currentUser, browser, Actions.Project.Delete(1), HttpMethod.Post, Claims.ProjectEdit);
        }


        [TestCase("abc")]
        [TestCase("111abc")]
        public void Delete_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.Delete(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _deleteProjectCommand.DidNotReceive().Execute(Arg.Any<int>());

        }

        [Test]
        public void Delete_ValidProjectId_DeletesProject()
        {
            int projectId = new Random().Next(1, 1000);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.Delete(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _deleteProjectCommand.Received(1).Execute(projectId);

        }

        #endregion

        #region Files

        [TestCase("abc")]
        [TestCase("111abc")]
        public void Files_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _projectValidator.Validate(Arg.Any<ProjectModel>()).Returns(new ValidationResult());
            _createProjectCommand.Execute(Arg.Any<ProjectModel>()).Returns(DataHelper.CreateProjectModel());

            TestHelper.ValidateAuth(currentUser, browser, Actions.Project.Save, HttpMethod.Post, Claims.ProjectEdit);
        }

        [Test]
        public void Save_InvalidProject_ReturnsFailure()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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
            SaveResultModel result = JsonConvert.DeserializeObject<SaveResultModel>(response.Body.AsString());
            Assert.AreEqual("0", result.Id);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Messages.Length);

            // the project should not have been added
            _createProjectCommand.DidNotReceive().Execute(Arg.Any<ProjectModel>());

        }

        [Test]
        public void Save_ValidProject_Saves()
        {
            // setup
            ProjectModel project = DataHelper.CreateProjectModel();
            project.Name = "TestProject";

            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            _createProjectCommand.Execute(Arg.Any<ProjectModel>()).Returns(project);
            _projectValidator.Validate(Arg.Any<ProjectModel>()).Returns(new ValidationResult());

            // execute
            var response = browser.Post(Actions.Project.Save, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("Name", project.Name);
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the result
            SaveResultModel result = JsonConvert.DeserializeObject<SaveResultModel>(response.Body.AsString());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(project.Id.ToString(), result.Id);
            Assert.AreEqual(0, result.Messages.Length);

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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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

        [Test]
        public void View_IsProjectEditor_ProjectEditControlsRendered()
        {

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            ProjectModel project = DataHelper.CreateProjectModel();
            var url = Actions.Project.View(project.Id);

            _projectRepo.GetById(project.Id).Returns(project);

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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

            response.Body["#project-buttons-row"].ShouldExistOnce();
            response.Body["#tab-settings-pill"].ShouldExistOnce();
            response.Body["#tab-settings"].ShouldExistOnce();
        }

        [Test]
        public void View_IsNotProjectEditor_ProjectEditControlsNotRendered()
        {

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            ProjectModel project = DataHelper.CreateProjectModel();
            var url = Actions.Project.View(project.Id);

            _projectRepo.GetById(project.Id).Returns(project);

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo))
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

            response.Body["#project-buttons-row"].ShouldNotExist();
            response.Body["#tab-settings-pill"].ShouldNotExist();
            response.Body["#tab-settings"].ShouldNotExist();
        }

        #endregion

    }
}
