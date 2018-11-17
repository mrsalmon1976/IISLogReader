﻿using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using Nancy.Security;
using IISLogReader.Navigation;
using IISLogReader.ViewModels.Login;
using Nancy.Responses.Negotiation;
using IISLogReader.ViewModels;
using IISLogReader.BLL.Data.Stores;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using IISLogReader.ViewModels.Project;
using AutoMapper;
using IISLogReader.BLL.Validators;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Repositories;
using System.IO;
using Tx.Windows;
using IISLogReader.BLL.Exceptions;

namespace IISLogReader.Modules
{
    public class ProjectRequestAggregateModule : DefaultSecureModule
    {

        private IDbContext _dbContext;
        private ICreateProjectRequestAggregateCommand _createProjectRequestAggregateCommand;

        public ProjectRequestAggregateModule(IDbContext dbContext, ICreateProjectRequestAggregateCommand createProjectRequestAggregateCommand)
        {
            _dbContext = dbContext;
            _createProjectRequestAggregateCommand = createProjectRequestAggregateCommand;

            Post[Actions.ProjectRequestAggregate.Save()] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectSave });
                return Save();
            };

        }

        public dynamic Save()
        {
            ProjectAggregateViewModel viewModel = this.Bind<ProjectAggregateViewModel>();

            ProjectRequestAggregateModel model = new ProjectRequestAggregateModel();
            model.ProjectId = viewModel.ProjectId;
            model.AggregateTarget = viewModel.AggregateTarget;
            model.RegularExpression = viewModel.RegularExpression;
            SaveResultModel result = null;
            _dbContext.BeginTransaction();
            try
            {
                _createProjectRequestAggregateCommand.Execute(model);
                _dbContext.Commit();
                result = new SaveResultModel(model.Id.ToString(), true, Enumerable.Empty<string>().ToArray());
            }
            catch (ValidationException vex)
            {
                result = new SaveResultModel(String.Empty, false, vex.ValidationErrors);
                _dbContext.Rollback();
            }

            return this.Response.AsJson(result);
        }



    }
}
