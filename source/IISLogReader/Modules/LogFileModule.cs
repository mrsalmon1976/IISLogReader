using Nancy;
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

namespace IISLogReader.Modules
{
    public class LogFileModule : DefaultSecureModule
    {

        private IDbContext _dbContext;
        private ICreateLogFileWithRequestsCommand _createLogFileWithRequestsCommand;
        private IDeleteLogFileCommand _deleteLogFileCommand;

        public LogFileModule(IDbContext dbContext, ICreateLogFileWithRequestsCommand createLogFileWithRequestsCommand, IDeleteLogFileCommand deleteLogFileCommand)
        {
            _dbContext = dbContext;
            _createLogFileWithRequestsCommand = createLogFileWithRequestsCommand;
            _deleteLogFileCommand = deleteLogFileCommand;

            Post[Actions.LogFile.Delete()] = x =>
            {
                return DeleteLogFile(x.logFileId);
            };

            Post[Actions.LogFile.Save()] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectSave });
                return Save();
            };

        }

        public dynamic DeleteLogFile(dynamic lfid)
        {
            // make sure the id is a valid integer
            int logFileId = 0;
            if (!Int32.TryParse((lfid ?? "").ToString(), out logFileId))
            {
                return HttpStatusCode.BadRequest;
            }

            _deleteLogFileCommand.Execute(logFileId);
            return this.Response.AsJson("");
        }

        public dynamic Save()
        {
            SaveLogFileViewModel model = this.Bind<SaveLogFileViewModel>();
            foreach (HttpFile f in Request.Files)
            {
                _dbContext.BeginTransaction();
                try
                {
                    _createLogFileWithRequestsCommand.Execute(model.ProjectId, f.Name, f.Value);
                    _dbContext.Commit();
                }
                catch (Exception ex)
                {
                    _dbContext.Rollback();
                    return this.Response.AsJson<string>(ex.Message, HttpStatusCode.BadRequest);
                }

            }
            return HttpStatusCode.OK;
        }



    }
}
