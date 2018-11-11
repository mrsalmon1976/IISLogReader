using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tx.Windows;
using IISLogReader.BLL.Services;

namespace IISLogReader.BLL.Commands
{
    public interface ICreateLogFileWithRequestsCommand
    {
        void Execute(int projectId, string fileName, Stream fileStream);
    }

    public class CreateLogFileWithRequestsCommand : ICreateLogFileWithRequestsCommand
    {
        private IDbContext _dbContext;
        private ILogFileRepository _logFileRepo;
        private ICreateLogFileCommand _createLogFileCommand;
        private ICreateRequestBatchCommand _createRequestBatchCommand;
        private IJobRegistrationService _jobRegistrationService;

        public CreateLogFileWithRequestsCommand(IDbContext dbContext, ILogFileRepository logFileRepo, ICreateLogFileCommand createLogFileCommand, ICreateRequestBatchCommand createRequestBatchCommand, IJobRegistrationService jobRegistrationService)
        {
            _dbContext = dbContext;
            _logFileRepo = logFileRepo;
            _createLogFileCommand = createLogFileCommand;
            _createRequestBatchCommand = createRequestBatchCommand;
            _jobRegistrationService = jobRegistrationService;
        }

        public void Execute(int projectId, string fileName, Stream fileStream)
        {
            // load the contents of the file
            StreamReader reader = new StreamReader(fileStream);
            List<W3CEvent> logEvents;
            string fileHash = String.Empty;

            try
            {
                logEvents = W3CEnumerable.FromStream(reader).ToList();
            }
            catch (Exception)
            {
                throw new FileFormatException("File is not a valid IIS log file");
            }

            // check the hash of the file - if it is already saved against this project, throw an exception
            using (var md5 = MD5.Create())
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                var hash = md5.ComputeHash(fileStream);
                fileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            LogFileModel logFile = _logFileRepo.GetByHash(projectId, fileHash);
            if (logFile != null)
            {
                throw new ValidationException("Log file already loaded for this project");
            }

            // save details of the file itself
            logFile = new LogFileModel();
            logFile.ProjectId = projectId;
            logFile.FileHash = fileHash;
            logFile.FileLength = fileStream.Length;
            logFile.FileName = fileName;
            logFile.RecordCount = logEvents.Count;
            logFile = _createLogFileCommand.Execute(logFile);

            // save the requests
            _createRequestBatchCommand.Execute(logFile.Id, logEvents);

            // register the job that processes the log file
            _jobRegistrationService.RegisterAggregateRequestJob(logFile.Id);
        }
    }
}
