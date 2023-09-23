﻿using AutoMapper;
using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using IISLogReader.Modules;
using IISLogReader.Navigation;
using IISLogReader.ViewModels;
using System;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Exceptions;
using System.Net.Http;

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
        public void Delete_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectRequestAggregateModule(_dbContext, _createProjectRequestAggregateCommand, _deleteProjectRequestAggregateCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            TestHelper.ValidateAuth(currentUser, browser, Actions.ProjectRequestAggregate.Delete(), HttpMethod.Post, Claims.ProjectEdit);
        }


        [Test]
        public void Delete_OnValidPost_ExecutesCommand()
        {
            int id = new Random().Next(1, 100);

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };

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
        public void Save_AuthTest()
        {
            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            var browser = new Browser((bootstrapper) =>
                bootstrapper.Module(new ProjectRequestAggregateModule(_dbContext, _createProjectRequestAggregateCommand, _deleteProjectRequestAggregateCommand))
                    .RequestStartup((container, pipelines, context) => {
                        context.CurrentUser = currentUser;
                    })
                );

            TestHelper.ValidateAuth(currentUser, browser, Actions.ProjectRequestAggregate.Save(), HttpMethod.Post, Claims.ProjectEdit);
        }

        [Test]
        public void Save_OnSaveValidationError_ErrorReturnedInResponse()
        {
            int projectId = new Random().Next(1, 100);
            string exceptionMessage = Guid.NewGuid().ToString();

            // setup
            var currentUser = new UserIdentity() { Id = Guid.NewGuid(), UserName = "Joe Soap" };
            currentUser.Claims = new string[] { Claims.ProjectEdit };
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
            currentUser.Claims = new string[] { Claims.ProjectEdit };

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
