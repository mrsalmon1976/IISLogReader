using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Models;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Commands.Project
{
    public interface ICreateLogFileCommand
    {
        LogFileModel Execute(LogFileModel logFile);
    }
    public class CreateLogFileCommand : ICreateLogFileCommand
    {
        private IDbContext _dbContext;
        private ILogFileValidator _logFileValidator;

        public CreateLogFileCommand(IDbContext dbContext, ILogFileValidator logFileValidator)
        {
            _dbContext = dbContext;
            _logFileValidator = logFileValidator;
        }

        public LogFileModel Execute(LogFileModel logFile)
        {
            // validate
            ValidationResult result = _logFileValidator.Validate(logFile);
            if (!result.Success)
            {
                throw new ValidationException(result.Messages);
            }

            // insert new record
            string sql = @"INSERT INTO LogFiles (ProjectId, FileName, FileHash, CreateDate, FileLength, RecordCount) VALUES (@ProjectId, @FileName, @FileHash, @CreateDate, @FileLength, @RecordCount)";
            _dbContext.ExecuteNonQuery(sql, logFile);

            sql = @"select last_insert_rowid()";
            logFile.Id = _dbContext.ExecuteScalar<int>(sql);
            return logFile;
        }
    }
}
