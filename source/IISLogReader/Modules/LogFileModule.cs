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
using IISLogReader.Configuration;
using SystemWrapper.IO;

namespace IISLogReader.Modules
{
    public class LogFileModule : DefaultSecureModule
    {

        private IDbContext _dbContext;
        private ICreateLogFileCommand _createLogFileCommand;
        private IDeleteLogFileCommand _deleteLogFileCommand;
        private IAppSettings _appSettings;
        private IDirectoryWrap _dirWrap;

        public LogFileModule(IDbContext dbContext
            , IAppSettings appSettings
            , ICreateLogFileCommand createLogFileCommand
            , IDeleteLogFileCommand deleteLogFileCommand
            , IDirectoryWrap dirWrap
            )
        {
            _dbContext = dbContext;
            _appSettings = appSettings;
            _createLogFileCommand = createLogFileCommand;
            _deleteLogFileCommand = deleteLogFileCommand;
            _dirWrap = dirWrap;

            Post[Actions.LogFile.Delete()] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectEdit });
                return DeleteLogFile(x.logFileId);
            };

            Post[Actions.LogFile.Save()] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectEdit });
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

            // make sure the processing directory exists
            _dirWrap.CreateDirectory(_appSettings.LogFileProcessingDirectory);

            foreach (HttpFile f in Request.Files)
            {
                // save the file to disk
                string filePath = Path.Combine(_appSettings.LogFileProcessingDirectory, f.Name);
                using (var fileStream = File.Create(filePath))
                {
                    f.Value.Seek(0, SeekOrigin.Begin);
                    f.Value.CopyTo(fileStream);
                }

                // create the log file record - this will kick off the job to actually process the file
                try
                {
                    _dbContext.BeginTransaction();
                    _createLogFileCommand.Execute(model.ProjectId, filePath);
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
