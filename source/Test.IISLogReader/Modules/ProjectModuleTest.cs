using AutoMapper;
using Nancy;
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
using System;
using System.Collections.Generic;
using System.Linq;
using IISLogReader.ViewModels.Project;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Repositories;
using System.IO;
using System.Net.Http;
using IISLogReader.ViewModels.LogFile;
using System.Threading.Tasks;
using IISLogReader.BLL.Services;
using System.Security.Policy;
using Nancy.Security;

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
        private IRequestAggregationService _requestAggregationService;

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
            _requestAggregationService = Substitute.For<IRequestAggregationService>();

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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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

            IEnumerable<LogFileViewModel> result = JsonConvert.DeserializeObject<IEnumerable<LogFileViewModel>>(response.Body.AsString());
            Assert.AreEqual(3, result.Count());

        }

        #endregion

        #region Overview

        [TestCase("abc")]
        [TestCase("111abc")]
        public void Overview_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.Overview(projectId);
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _requestRepo.DidNotReceive().GetTotalRequestCountAsync(Arg.Any<int>());

        }

        [Test]
        public void Overview_ValidProjectId_TotalRequestCountCorrectlyLoaded()
        {
            int projectId = new Random().Next(1, 1000);
            long totalRequestCount = new Random().Next(100, 100000);

            _requestAggregationService.GetRequestStatusCodeSummary(Arg.Any<IEnumerable<RequestStatusCodeCount>>()).Returns(new RequestStatusCodeSummary());
            _requestRepo.GetTotalRequestCountAsync(projectId).Returns(Task.FromResult(totalRequestCount));

            // setup
            var url = Actions.Project.Overview(projectId);

            // execute
            var response = ExecuteGet(url);

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _requestRepo.Received(1).GetTotalRequestCountAsync(projectId).GetAwaiter().GetResult();

            ProjectOverviewViewModel result = JsonConvert.DeserializeObject<ProjectOverviewViewModel>(response.Body.AsString());
            Assert.That(result.TotalRequestCount, Is.EqualTo(totalRequestCount));

        }

        [Test]
        public void Overview_ValidProjectId_RequestSuccessCountCorrectlyLoaded()
        {
            int projectId = new Random().Next(1, 1000);
            long successCount = new Random().Next(100, 100000);

            RequestStatusCodeSummary requestStatusCodeSummary = new RequestStatusCodeSummary();
            requestStatusCodeSummary.SuccessCount = successCount;

            _requestAggregationService.GetRequestStatusCodeSummary(Arg.Any<IEnumerable<RequestStatusCodeCount>>()).Returns(requestStatusCodeSummary);

            // setup
            var url = Actions.Project.Overview(projectId);

            // execute
            var response = ExecuteGet(url);

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _requestRepo.Received(1).GetStatusCodeSummaryAsync(projectId).GetAwaiter().GetResult();

            ProjectOverviewViewModel result = JsonConvert.DeserializeObject<ProjectOverviewViewModel>(response.Body.AsString());
            Assert.That(result.SuccessRequestCount, Is.EqualTo(successCount));

        }

        [Test]
        public void Overview_ValidProjectId_RequestClientErrorCountCorrectlyLoaded()
        {
            int projectId = new Random().Next(1, 1000);
            long clientErrorCount = new Random().Next(100, 100000);

            RequestStatusCodeSummary requestStatusCodeSummary = new RequestStatusCodeSummary();
            requestStatusCodeSummary.ClientErrorCount = clientErrorCount;

            _requestAggregationService.GetRequestStatusCodeSummary(Arg.Any<IEnumerable<RequestStatusCodeCount>>()).Returns(requestStatusCodeSummary);

            // setup
            var url = Actions.Project.Overview(projectId);

            // execute
            var response = ExecuteGet(url);

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _requestRepo.Received(1).GetStatusCodeSummaryAsync(projectId).GetAwaiter().GetResult();

            ProjectOverviewViewModel result = JsonConvert.DeserializeObject<ProjectOverviewViewModel>(response.Body.AsString());
            Assert.That(result.ClientErrorRequestCount, Is.EqualTo(clientErrorCount));

        }

        [Test]
        public void Overview_ValidProjectId_RequestServerErrorCountCorrectlyLoaded()
        {
            int projectId = new Random().Next(1, 1000);
            long serverErrorCount = new Random().Next(100, 100000);

            RequestStatusCodeSummary requestStatusCodeSummary = new RequestStatusCodeSummary();
            requestStatusCodeSummary.ServerErrorCount = serverErrorCount;

            _requestAggregationService.GetRequestStatusCodeSummary(Arg.Any<IEnumerable<RequestStatusCodeCount>>()).Returns(requestStatusCodeSummary);

            // setup
            var url = Actions.Project.Overview(projectId);

            // execute
            var response = ExecuteGet(url);

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _requestRepo.Received(1).GetStatusCodeSummaryAsync(projectId).GetAwaiter().GetResult();

            ProjectOverviewViewModel result = JsonConvert.DeserializeObject<ProjectOverviewViewModel>(response.Body.AsString());
            Assert.That(result.ServerErrorRequestCount, Is.EqualTo(serverErrorCount));

        }

        [Test]
        public void Overview_ValidProjectId_RequestRedirectionCountCorrectlyLoaded()
        {
            int projectId = new Random().Next(1, 1000);
            long redirectionCount = new Random().Next(100, 100000);

            RequestStatusCodeSummary requestStatusCodeSummary = new RequestStatusCodeSummary();
            requestStatusCodeSummary.RedirectionCount = redirectionCount;

            _requestAggregationService.GetRequestStatusCodeSummary(Arg.Any<IEnumerable<RequestStatusCodeCount>>()).Returns(requestStatusCodeSummary);

            // setup
            var url = Actions.Project.Overview(projectId);

            // execute
            var response = ExecuteGet(url);

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _requestRepo.Received(1).GetStatusCodeSummaryAsync(projectId).GetAwaiter().GetResult();

            ProjectOverviewViewModel result = JsonConvert.DeserializeObject<ProjectOverviewViewModel>(response.Body.AsString());
            Assert.That(result.RedirectionRequestCount, Is.EqualTo(redirectionCount));

        }

        [Test]
        public void Overview_ValidProjectId_LogFileCountCorrectlyLoaded()
        {
            int projectId = new Random().Next(1, 1000);
            int logFileCount = new Random().Next(100, 100000);

            _requestAggregationService.GetRequestStatusCodeSummary(Arg.Any<IEnumerable<RequestStatusCodeCount>>()).Returns(new RequestStatusCodeSummary());
            _logFileRepo.GetCountByProjectAsync(projectId).Returns(Task.FromResult(logFileCount));

            // setup
            var url = Actions.Project.Overview(projectId);

            // execute
            var response = ExecuteGet(url);

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _logFileRepo.Received(1).GetCountByProjectAsync(projectId).GetAwaiter().GetResult();

            ProjectOverviewViewModel result = JsonConvert.DeserializeObject<ProjectOverviewViewModel>(response.Body.AsString());
            Assert.That(result.LogFileCount, Is.EqualTo(logFileCount));

        }

        #endregion

        #region RequestsByAggregate

        [TestCase("abc")]
        [TestCase("111abc")]
        public void RequestsByAggregate_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.RequestsByAggregate(projectId);
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _projectRepo.DidNotReceive().GetById(Arg.Any<int>());

        }

        [Test()]
        public void RequestsByAggregate_ProjectDoesNotExist_Returns404()
        {
            int projectId = new Random().Next(1, 1000);
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.RequestsByAggregate(projectId);
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
        public void RequestsByAggregate_ValidProjectId_LoadsProjectAndPopulatesModel()
        {
            int projectId = new Random().Next(1, 1000);
            string uri = "/" + Path.GetRandomFileName();

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                        context.ViewBag.CurrentUserName = currentUser.UserName;
                        context.ViewBag.Scripts = new List<string>();
                        context.ViewBag.Claims = new List<string>();
                        context.ViewBag.Projects = new List<ProjectModel>();
                    })
                );

            ProjectModel project = DataHelper.CreateProjectModel();
            project.Id = projectId;
            _projectRepo.GetById(projectId).Returns(project);

            // execute
            var url = Actions.Project.RequestsByAggregate(projectId);
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.Query("uri", uri);
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _projectRepo.Received(1).GetById(projectId);

            response.Body["#projectId"].ShouldExistOnce();
            response.Body["#projectId"].ShouldContainAttribute("value", projectId.ToString());

            response.Body["#uri"].ShouldExistOnce();
            response.Body["#uri"].ShouldContainAttribute("value", uri);

        }

        #endregion

        #region RequestByAggregateDetail

        [TestCase("abc")]
        [TestCase("111abc")]
        public void RequestByAggregateDetail_InvalidProjectId_Returns400(string projectId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.Project.RequestsByAggregateDetail(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _projectRepo.DidNotReceive().GetById(Arg.Any<int>());

        }

        [Test]
        public void RequestByAggregateDetail_ValidProjectId_GetsDetailFromDatabase()
        {
            int projectId = new Random().Next(1, 1000);
            string uri = "/" + Guid.NewGuid().ToString();
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );
            RequestModel req1 = DataHelper.CreateRequestModel();
            RequestModel req2 = DataHelper.CreateRequestModel();
            RequestModel req3 = DataHelper.CreateRequestModel();

            _requestRepo.GetByUriStemAggregate(projectId, uri).Returns(new RequestModel[] { req1, req2, req3 });

            // execute
            var url = Actions.Project.RequestsByAggregateDetail(projectId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("uri", uri);
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _requestRepo.Received(1).GetByUriStemAggregate(projectId, uri);

            IEnumerable<RequestModel> result = JsonConvert.DeserializeObject<IEnumerable<RequestModel>>(response.Body.AsString());
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
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

        #region Private Methods

        private Browser CreateBrowser(IUserIdentity currentUser)
        {
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectModule(_dbContext, _projectValidator, _createProjectCommand, _deleteProjectCommand, _projectRepo, _logFileRepo, _requestRepo, _projectRequestAggregateRepo, _requestAggregationService))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );
            return browser;
        }

        private BrowserResponse ExecuteGet(string url)
        {
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = CreateBrowser(currentUser);

            // execute
            var response = browser.Get(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });
            return response;

        }

        #endregion

    }
}
