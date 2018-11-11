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
    public interface IDeleteProjectCommand
    {
        void Execute(int projectId);
    }
    public class DeleteProjectCommand : IDeleteProjectCommand
    {
        private IDbContext _dbContext;

        public DeleteProjectCommand(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(int projectId)
        {
            string sql = @"DELETE FROM ProjectRequestAggregates WHERE ProjectId = @ProjectId";
            _dbContext.ExecuteNonQuery(sql, new { ProjectId = projectId });

            sql = @"DELETE FROM Requests WHERE LogFileId IN (SELECT Id FROM LogFiles WHERE ProjectId = @ProjectId)";
            _dbContext.ExecuteNonQuery(sql, new { ProjectId = projectId });

            sql = @"DELETE FROM LogFiles WHERE ProjectId = @ProjectId";
            _dbContext.ExecuteNonQuery(sql, new { ProjectId = projectId });

            sql = @"DELETE FROM Projects WHERE Id = @ProjectId";
            _dbContext.ExecuteNonQuery(sql, new { ProjectId = projectId });
        }
    }
}
