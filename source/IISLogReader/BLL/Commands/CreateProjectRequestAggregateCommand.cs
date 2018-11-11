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
    public interface ICreateProjectRequestAggregateCommand
    {
        ProjectRequestAggregateModel Execute(ProjectRequestAggregateModel projectRequestAggregate);
    }

    public class CreateProjectRequestAggregateCommand : ICreateProjectRequestAggregateCommand
    {
        private IDbContext _dbContext;
        private IProjectRequestAggregateValidator _projectRequestAggregateValidator;

        public CreateProjectRequestAggregateCommand(IDbContext dbContext, IProjectRequestAggregateValidator projectRequestAggregateValidator)
        {
            _dbContext = dbContext;
            _projectRequestAggregateValidator = projectRequestAggregateValidator;
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

            sql = @"select last_insert_rowid()";
            projectRequestAggregate.Id = _dbContext.ExecuteScalar<int>(sql);
            return projectRequestAggregate;
        }
    }
}
