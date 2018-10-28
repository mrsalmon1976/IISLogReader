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
    public class LogFileModuleTest
    {
        private IDbContext _dbContext;
        private ICreateLogFileWithRequestsCommand _createLogFileWithRequestsCommand;

        [SetUp]
        public void LogFileModuleTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _createLogFileWithRequestsCommand = Substitute.For<ICreateLogFileWithRequestsCommand>();
        }

        [TearDown]
        public void LogFileModuleTest_TearDown()
        {
            Mapper.Reset();
        }

        #region Save Tests


        [Test]
        public void Save_OnSaveError_ErrorReturnedInResponse()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string exceptionMessage = Guid.NewGuid().ToString();

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectSave };
            _createLogFileWithRequestsCommand.When(x => x.Execute(projectId, fileName, Arg.Any<HttpMultipartSubStream>()))
                .Do((c) => { throw new Exception(exceptionMessage); });
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _createLogFileWithRequestsCommand))
                    .RequestStartup((container, pipelines, context) => {
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
                _createLogFileWithRequestsCommand.Received(1).Execute(projectId, fileName, Arg.Any<HttpMultipartSubStream>());
                _dbContext.Received(1).Rollback();
            }
        }

        [Test]
        public void Save_OnFilePost_ExecutesCommand()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectSave };

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new LogFileModule(_dbContext, _createLogFileWithRequestsCommand))
                    .RequestStartup((container, pipelines, context) => {
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
                _createLogFileWithRequestsCommand.Received(1).Execute(projectId, fileName, Arg.Any<HttpMultipartSubStream>());
                _dbContext.Received(1).Commit();
            }
        }

        #endregion





    }
}
