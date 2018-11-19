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
using IISLogReader.BLL.Services;

namespace IISLogReader.BLL.Commands
{
    public interface IDeleteProjectRequestAggregateCommand
    {
        void Execute(int id);
    }

    public class DeleteProjectRequestAggregateCommand : IDeleteProjectRequestAggregateCommand
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectRequestAggregateRepository _projectRequestAggregateRepo;
        private readonly ILogFileRepository _logFileRepo;
        private readonly ISetLogFileUnprocessedCommand _setLogFileUnprocessedCommand;

        public DeleteProjectRequestAggregateCommand(IDbContext dbContext, IProjectRequestAggregateRepository projectRequestAggregateRepo, ILogFileRepository logFileRepo, ISetLogFileUnprocessedCommand setLogFileUnprocessedCommand)
        {
            _dbContext = dbContext;
            _projectRequestAggregateRepo = projectRequestAggregateRepo;
            _logFileRepo = logFileRepo;
            _setLogFileUnprocessedCommand = setLogFileUnprocessedCommand;
        }

        public void Execute(int id)
        {
            // load the record up
            ProjectRequestAggregateModel projectRequestAggregate = _projectRequestAggregateRepo.GetById(id);
            if (projectRequestAggregate == null)
            {
                throw new InvalidOperationException(String.Format("No project aggregate request found with id {0}", id));
            }

            // insert new record
            string sql = @"DELETE FROM ProjectRequestAggregates WHERE Id = @Id";
            _dbContext.ExecuteNonQuery(sql, new { Id = id });

            // register jobs so all log files are reprocessed
            IEnumerable<LogFileModel> logFiles = _logFileRepo.GetByProject(projectRequestAggregate.ProjectId);
            foreach (LogFileModel logFile in logFiles)
            {
                _setLogFileUnprocessedCommand.Execute(logFile.Id);
            }
        }
    }
}
