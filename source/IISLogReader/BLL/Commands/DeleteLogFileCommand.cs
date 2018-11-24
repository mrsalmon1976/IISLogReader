using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Commands
{
    public interface IDeleteLogFileCommand
    {
        void Execute(int logFileId);
    }
    public class DeleteLogFileCommand : IDeleteLogFileCommand
    {
        private IDbContext _dbContext;

        public DeleteLogFileCommand(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(int logFileId)
        {
            string sql = @"DELETE FROM Requests WHERE LogFileId = @LogFileId";
            _dbContext.ExecuteNonQuery(sql, new { LogFileId = logFileId });

            sql = @"DELETE FROM LogFiles WHERE Id = @LogFileId";
            _dbContext.ExecuteNonQuery(sql, new { LogFileId = logFileId });

        }
    }
}
