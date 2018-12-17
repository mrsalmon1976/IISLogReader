using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Repositories;
using SystemWrapper.IO;
using System.Security.Cryptography;
using System.IO;
using IISLogReader.BLL.Lookup;
using IISLogReader.BLL.Services;
using IISLogReader.BLL.Utils;

namespace IISLogReader.BLL.Commands
{
    public interface ICreateLogFileCommand
    {
        LogFileModel Execute(int projectId, string filePath);
    }
    public class CreateLogFileCommand : ICreateLogFileCommand
    {
        private readonly IDbContext _dbContext;
        private readonly ILogFileValidator _logFileValidator;
        private readonly ILogFileRepository _logFileRepo;
        private readonly IFileUtils _fileUtils;
        private readonly IJobRegistrationService _jobRegistrationService;

        public CreateLogFileCommand(IDbContext dbContext
            , ILogFileValidator logFileValidator
            , ILogFileRepository logFileRepo
            , IJobRegistrationService jobRegistrationService
            , IFileUtils fileUtils)
        {
            _dbContext = dbContext;
            _logFileValidator = logFileValidator;
            _logFileRepo = logFileRepo;
            _jobRegistrationService = jobRegistrationService;
            _fileUtils = fileUtils;
        }

        public LogFileModel Execute(int projectId, string filePath)
        {
            FileDetail fileDetail = _fileUtils.GetFileHash(filePath);

            LogFileModel logFile = _logFileRepo.GetByHash(projectId, fileDetail.Hash);
            if (logFile != null)
            {
                throw new ValidationException("Log file already loaded for this project");
            }

            // save details of the file itself
            logFile = new LogFileModel();
            logFile.ProjectId = projectId;
            logFile.FileHash = fileDetail.Hash;
            logFile.FileLength = fileDetail.Length;
            logFile.FileName = fileDetail.Name;
            logFile.RecordCount = -1;
            logFile.Status = LogFileStatus.Processing;

            // validate
            ValidationResult result = _logFileValidator.Validate(logFile);
            if (!result.Success)
            {
                throw new ValidationException(result.Messages);
            }

            // insert new record
            string sql = @"INSERT INTO LogFiles (ProjectId, FileName, FileHash, CreateDate, FileLength, RecordCount, Status) VALUES (@ProjectId, @FileName, @FileHash, @CreateDate, @FileLength, @RecordCount, @Status)";
            _dbContext.ExecuteNonQuery(sql, logFile);

            sql = @"select last_insert_rowid()";
            logFile.Id = _dbContext.ExecuteScalar<int>(sql);

            // register the job to process the log file
            _jobRegistrationService.RegisterProcessLogFileJob(logFile.Id, filePath);

            return logFile;
        }
    }
}
