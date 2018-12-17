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
using SystemWrapper.IO;
using IISLogReader.Configuration;

namespace Test.IISLogReader.Modules
{
    [TestFixture]
    public class LogFileModuleTest
    {
        private IDbContext _dbContext;
        private IAppSettings _appSettings;
        private ICreateLogFileCommand _createLogFileCommand;
        private IDeleteLogFileCommand _deleteLogFileCommand;
        private IDirectoryWrap _dirWrap;

        [SetUp]
        public void LogFileModuleTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _appSettings = Substitute.For<IAppSettings>();
            _createLogFileCommand = Substitute.For<ICreateLogFileCommand>();
            _deleteLogFileCommand = Substitute.For<IDeleteLogFileCommand>();
            _dirWrap = Substitute.For<IDirectoryWrap>();
        }

        [TearDown]
        public void LogFileModuleTest_TearDown()
        {
            // delete all .log files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.log");

            Mapper.Reset();
        }

        #region Delete

        [Test]
        public void Delete_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _appSettings, _createLogFileCommand, _deleteLogFileCommand, _dirWrap))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );

            TestHelper.ValidateAuth(currentUser, browser, Actions.LogFile.Delete(1), HttpMethod.Post, Claims.ProjectEdit);
        }

        [TestCase("abc")]
        [TestCase("111abc")]
        public void Delete_InvalidId_Returns400(string logFileId)
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _appSettings, _createLogFileCommand, _deleteLogFileCommand, _dirWrap))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.LogFile.Delete(logFileId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _deleteLogFileCommand.DidNotReceive().Execute(Arg.Any<int>());

        }

        [Test]
        public void Delete_ValidId_DeletesProject()
        {
            int logFileId = new Random().Next(1, 1000);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _appSettings, _createLogFileCommand, _deleteLogFileCommand, _dirWrap))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.LogFile.Delete(logFileId);
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _deleteLogFileCommand.Received(1).Execute(logFileId);

        }

        #endregion

        #region Save Tests

        [Test]
        public void Save_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _appSettings, _createLogFileCommand, _deleteLogFileCommand, _dirWrap))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );

            TestHelper.ValidateAuth(currentUser, browser, Actions.LogFile.Save(1), HttpMethod.Post, Claims.ProjectEdit);
        }


        [Test]
        public void Save_OnSaveError_ErrorReturnedInResponse()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            string exceptionMessage = Guid.NewGuid().ToString();

            // setup
            _appSettings.LogFileProcessingDirectory.Returns(AppContext.BaseDirectory);

            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
            _createLogFileCommand.When(x => x.Execute(projectId, filePath))
                .Do((c) => { throw new Exception(exceptionMessage); });

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _appSettings, _createLogFileCommand, _deleteLogFileCommand, _dirWrap))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );
            byte[] buffer = new byte[100];
            new Random().NextBytes(buffer);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                var multipart = new BrowserContextMultipartFormData(x =>
                {
                    x.AddFile("foo", fileName, "text/plain", stream);
                });

                // execute
                var url = Actions.LogFile.Save(projectId);
                var response = browser.Post(url, (with) =>
                {
                    with.HttpRequest();
                    with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                    with.MultiPartFormData(multipart);
                    with.FormValue("projectId", projectId.ToString());
                });

                // assert
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                string result = response.Body.AsString();

                Assert.AreEqual(JsonConvert.SerializeObject(exceptionMessage), result);
                _createLogFileCommand.Received(1).Execute(projectId, filePath);
                _dbContext.Received(1).Rollback();
            }
        }

        [Test]
        public void Save_OnFilePost_ExecutesCommand()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);

            // setup
            _appSettings.LogFileProcessingDirectory.Returns(AppContext.BaseDirectory);

            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _appSettings, _createLogFileCommand, _deleteLogFileCommand, _dirWrap))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );
            byte[] buffer = new byte[100];
            new Random().NextBytes(buffer);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                var multipart = new BrowserContextMultipartFormData(x =>
                {
                    x.AddFile("foo", fileName, "text/plain", stream);
                });

                // execute
                var url = Actions.LogFile.Save(projectId);
                var response = browser.Post(url, (with) =>
                {
                    with.HttpRequest();
                    with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                    with.MultiPartFormData(multipart);
                    with.FormValue("projectId", projectId.ToString());
                });

                // assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                _createLogFileCommand.Received(1).Execute(projectId, filePath);
                _dbContext.Received(1).BeginTransaction();
                _dbContext.Received(1).Commit();
            }
        }

        [Test]
        public void Save_OnFilePost_CreatesDirectoryAndFile()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);

            // setup
            _appSettings.LogFileProcessingDirectory.Returns(AppContext.BaseDirectory);

            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _appSettings, _createLogFileCommand, _deleteLogFileCommand, _dirWrap))
                    .RequestStartup((container, pipelines, context) =>
                    {
                        context.CurrentUser = currentUser;
                    })
                );
            byte[] buffer = new byte[100];
            new Random().NextBytes(buffer);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                var multipart = new BrowserContextMultipartFormData(x =>
                {
                    x.AddFile("foo", fileName, "text/plain", stream);
                });

                // execute
                var url = Actions.LogFile.Save(projectId);
                var response = browser.Post(url, (with) =>
                {
                    with.HttpRequest();
                    with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                    with.MultiPartFormData(multipart);
                    with.FormValue("projectId", projectId.ToString());
                });

                // assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                _dirWrap.Received(1).CreateDirectory(AppContext.BaseDirectory);
                Assert.IsTrue(File.Exists(filePath));
            }
        }

        #endregion





    }
}
