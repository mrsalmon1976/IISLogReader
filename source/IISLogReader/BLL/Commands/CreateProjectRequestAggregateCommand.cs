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
    public interface ICreateProjectRequestAggregateCommand
    {
        ProjectRequestAggregateModel Execute(ProjectRequestAggregateModel projectRequestAggregate);
    }

    public class CreateProjectRequestAggregateCommand : ICreateProjectRequestAggregateCommand
    {
        private readonly IDbContext _dbContext;
        private readonly IProjectRequestAggregateValidator _projectRequestAggregateValidator;
        private readonly ILogFileRepository _logFileRepo;
        private readonly ISetLogFileUnprocessedCommand _setLogFileUnprocessedCommand;

        public CreateProjectRequestAggregateCommand(IDbContext dbContext, IProjectRequestAggregateValidator projectRequestAggregateValidator, ILogFileRepository logFileRepo, ISetLogFileUnprocessedCommand setLogFileUnprocessedCommand)
        {
            _dbContext = dbContext;
            _projectRequestAggregateValidator = projectRequestAggregateValidator;
            _logFileRepo = logFileRepo;
            _setLogFileUnprocessedCommand = setLogFileUnprocessedCommand;
        }

        public ProjectRequestAggregateModel Execute(ProjectRequestAggregateModel projectRequestAggregate)
        {
            // validate
            ValidationResult result = _projectRequestAggregateValidator.Validate(projectRequestAggregate);
            if (!result.Success)
            {
                throw new ValidationException(result.Messages);
            }

            // insert new record
            string sql = @"INSERT INTO ProjectRequestAggregates (ProjectId, RegularExpression, AggregateTarget, CreateDate) VALUES (@ProjectId, @RegularExpression, @AggregateTarget, @CreateDate)";
            _dbContext.ExecuteNonQuery(sql, projectRequestAggregate);

            // update the id
            sql = @"select last_insert_rowid()";
            projectRequestAggregate.Id = _dbContext.ExecuteScalar<int>(sql);

            // register jobs so all log files are reprocessed
            IEnumerable<LogFileModel> logFiles = _logFileRepo.GetByProject(projectRequestAggregate.ProjectId);
            foreach (LogFileModel logFile in logFiles)
            {
                _setLogFileUnprocessedCommand.Execute(logFile.Id);
            }

            return projectRequestAggregate;
        }
    }
}
