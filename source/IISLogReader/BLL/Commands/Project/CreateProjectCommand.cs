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
    public interface ICreateProjectCommand
    {
        ProjectModel Execute(ProjectModel project);
    }
    public class CreateProjectCommand : ICreateProjectCommand
    {
        private IDbContext _dbContext;
        private IProjectValidator _projectValidator;

        public CreateProjectCommand(IDbContext dbContext, IProjectValidator projectValidator)
        {
            _dbContext = dbContext;
            _projectValidator = projectValidator;
        }

        public ProjectModel Execute(ProjectModel project)
        {
            // validate
            ValidationResult result = _projectValidator.Validate(project);
            if (!result.Success)
            {
                throw new ValidationException(result.Messages);
            }

            // insert new record
            string sql = @"INSERT INTO Projects (Name, CreateDate) VALUES (@Name, @CreateDate)";
            _dbContext.ExecuteNonQuery(sql, project);

            sql = @"select last_insert_rowid()";
            project.Id = _dbContext.ExecuteScalar<int>(sql);
            return project;
        }
    }
}
