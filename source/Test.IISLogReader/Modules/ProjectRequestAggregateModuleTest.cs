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
using IISLogReader.BLL.Exceptions;

namespace Test.IISLogReader.Modules
{
    [TestFixture]
    public class ProjectRequestAggregateModuleTest
    {
        private IDbContext _dbContext;
        private ICreateProjectRequestAggregateCommand _createProjectRequestAggregateCommand;
        private IDeleteProjectRequestAggregateCommand _deleteProjectRequestAggregateCommand;

        [SetUp]
        public void ProjectRequestAggregateModuleTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _createProjectRequestAggregateCommand = Substitute.For<ICreateProjectRequestAggregateCommand>();
            _deleteProjectRequestAggregateCommand = Substitute.For<IDeleteProjectRequestAggregateCommand>();
        }

        [TearDown]
        public void ProjectRequestAggregateModuleTest_TearDown()
        {
            Mapper.Reset();
        }

        #region Delete Tests

        [Test]
        public void Delete_OnValidPost_ExecutesCommand()
        {
            int id = new Random().Next(1, 100);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectSave };

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectRequestAggregateModule(_dbContext, _createProjectRequestAggregateCommand, _deleteProjectRequestAggregateCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.ProjectRequestAggregate.Delete();
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("id", id.ToString());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _deleteProjectRequestAggregateCommand.Received(1).Execute(id);
            _dbContext.Received(1).Commit();
        }


        #endregion

        #region Save Tests

        [Test]
        public void Save_OnSaveValidationError_ErrorReturnedInResponse()
        {
            int projectId = new Random().Next(1, 100);
            string exceptionMessage = Guid.NewGuid().ToString();

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectSave };
            _createProjectRequestAggregateCommand.When(x => x.Execute(Arg.Any<ProjectRequestAggregateModel>()))
                .Do((c) => { throw new ValidationException(exceptionMessage); });
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectRequestAggregateModule(_dbContext, _createProjectRequestAggregateCommand, _deleteProjectRequestAggregateCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );
            // execute
            var url = Actions.ProjectRequestAggregate.Save();
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("projectId", projectId.ToString());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            SaveResultModel result = JsonConvert.DeserializeObject<SaveResultModel>(response.Body.AsString());

            Assert.IsFalse(result.Success);
            _createProjectRequestAggregateCommand.Received(1).Execute(Arg.Any<ProjectRequestAggregateModel>());
            _dbContext.Received(1).Rollback();
        }

        [Test]
        public void Save_OnValidPost_ExecutesCommand()
        {
            int projectId = new Random().Next(1, 100);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectSave };

            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectRequestAggregateModule(_dbContext, _createProjectRequestAggregateCommand, _deleteProjectRequestAggregateCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            // execute
            var url = Actions.ProjectRequestAggregate.Save();
            var response = browser.Post(url, (with) =>
            {
                with.HttpRequest();
                with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                with.FormValue("projectId", projectId.ToString());
            });

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _createProjectRequestAggregateCommand.Received(1).Execute(Arg.Any<ProjectRequestAggregateModel>());
            _dbContext.Received(1).Commit();
        }

        #endregion





    }
}
