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
using IISLogReader.BLL.Utils;
using SystemWrapper.IO;

namespace IISLogReader.BLL.Commands
{
    public interface IProcessLogFileCommand
    {
        void Execute(int logFileId, string filePath);
    }

    public class ProcessLogFileCommand : IProcessLogFileCommand
    {
        private readonly IDbContext _dbContext;
        private readonly ILogFileRepository _logFileRepo;
        private readonly ICreateRequestBatchCommand _createRequestBatchCommand;
        private readonly IJobRegistrationService _jobRegistrationService;
        private readonly IFileWrap _fileWrap;

        public ProcessLogFileCommand(IDbContext dbContext
            , ILogFileRepository logFileRepo
            , ICreateRequestBatchCommand createRequestBatchCommand
            , IJobRegistrationService jobRegistrationService
            , IFileWrap fileWrap
            )
        {
            _dbContext = dbContext;
            _logFileRepo = logFileRepo;
            _createRequestBatchCommand = createRequestBatchCommand;
            _jobRegistrationService = jobRegistrationService;
            _fileWrap = fileWrap;
        }

        public void Execute(int logFileId, string filePath)
        {
            LogFileModel logFile = _logFileRepo.GetById(logFileId);
            try
            {
                // load the contents of the file
                List<W3CEvent> logEvents;
                try
                {
                    logEvents = W3CEnumerable.FromFile(filePath).ToList();
                }
                catch (Exception)
                {
                    throw new FileFormatException("File is not a valid IIS log file");
                }


                // save the requests
                _createRequestBatchCommand.Execute(logFileId, logEvents);

                // update the record count of the log file
                const string sql = "UPDATE LogFiles SET RecordCount = @RecordCount WHERE Id = @Id";
                _dbContext.ExecuteNonQuery(sql, new { Id = logFileId, RecordCount = logEvents.Count });

                // delete the file
                _fileWrap.Delete(filePath);

                // register the job that marks the file as needing aggregate processing
                _jobRegistrationService.RegisterResetProcessedLogFileJob(logFile.Id);
            }
            catch (Exception ex)
            {
                // try and update the status of the log file record, the file will be marked as in an error state
                logFile.Status = BLL.Lookup.LogFileStatus.Error;
                logFile.ErrorMsg = ex.Message;
                string sql = "UPDATE LogFiles SET Status = @Status, ErrorMsg = @ErrorMsg WHERE Id = @Id";
                _dbContext.ExecuteNonQuery(sql, logFile);
            }
        }
    }
}
