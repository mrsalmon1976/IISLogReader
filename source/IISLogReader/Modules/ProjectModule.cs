using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using Nancy.Security;
using IISLogReader.Navigation;
using IISLogReader.ViewModels.Login;
using Nancy.Responses.Negotiation;
using IISLogReader.ViewModels;
using IISLogReader.BLL.Data.Stores;
using IISLogReader.BLL.Data.Models;
using IISLogReader.BLL.Security;
using IISLogReader.ViewModels.Project;
using AutoMapper;
using IISLogReader.BLL.Validators;
using IISLogReader.BLL.Commands.Project;
using IISLogReader.BLL.Data;

namespace IISLogReader.Modules
{
    public class ProjectModule : DefaultSecureModule
    {

        private IDbContext _dbContext;
        private IProjectValidator _projectValidator;
        private ICreateProjectCommand _createProjectCommand;

        public ProjectModule(IDbContext dbContext, IProjectValidator projectValidator, ICreateProjectCommand createProjectCommand)
        {
            _dbContext = dbContext;
            _projectValidator = projectValidator;
            _createProjectCommand = createProjectCommand;

            Get["/"] = x =>
            {
                return this.Response.AsRedirect(Actions.Login.Default);
            };

            Post[Actions.Project.Save] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectSave });
                return ProjectSave();
            };

        }

        public dynamic ProjectSave()
        {
            ProjectViewModel model = this.Bind<ProjectViewModel>();
            ProjectModel project = Mapper.Map<ProjectViewModel, ProjectModel>(model);

            ValidationResult result = _projectValidator.Validate(project);
            if (result.Success)
            {
                _dbContext.BeginTransaction();
                _createProjectCommand.Execute(_dbContext, project);
                _dbContext.Commit();
            }

            return this.Response.AsJson(result);
        }
    }
}
