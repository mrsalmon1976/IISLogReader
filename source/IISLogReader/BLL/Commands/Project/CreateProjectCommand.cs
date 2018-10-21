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
        ProjectModel Execute(IDbContext dbContext, ProjectModel project);
    }
    public class CreateProjectCommand : ICreateProjectCommand
    {
        private IProjectValidator _projectValidator;

        public CreateProjectCommand(IProjectValidator projectValidator)
        {
            _projectValidator = projectValidator;
        }

        public ProjectModel Execute(IDbContext dbContext, ProjectModel project)
        {
            // validate
            ValidationResult result = _projectValidator.Validate(project);
            if (!result.Success)
            {
                throw new ValidationException(result.Messages);
            }

            // insert new record
            string sql = @"INSERT INTO Projects (Name, CreateDate) VALUES (@Name, @CreateDate)";
            dbContext.ExecuteNonQuery(sql, project);

            sql = @"select last_insert_rowid()";
            project.Id = dbContext.ExecuteScalar<int>(sql);
            return project;
        }
    }
}
