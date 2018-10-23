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
using IISLogReader.BLL.Data.Repositories;

namespace IISLogReader.Modules
{
    public class ProjectModule : DefaultSecureModule
    {

        private IDbContext _dbContext;
        private IProjectValidator _projectValidator;
        private ICreateProjectCommand _createProjectCommand;
        private IProjectRepository _projectRepo;

        public ProjectModule(IDbContext dbContext, IProjectValidator projectValidator, ICreateProjectCommand createProjectCommand, IProjectRepository projectRepo)
        {
            _dbContext = dbContext;
            _projectValidator = projectValidator;
            _createProjectCommand = createProjectCommand;
            _projectRepo = projectRepo;

            Get[Actions.Project.View] = x =>
            {
                return ProjectView(x.projectId);
            };

            Post[Actions.Project.Save] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectSave });
                return ProjectSave();
            };

        }

        public dynamic ProjectSave()
        {
            ProjectFormViewModel model = this.Bind<ProjectFormViewModel>();
            ProjectModel project = Mapper.Map<ProjectFormViewModel, ProjectModel>(model);

            ValidationResult result = _projectValidator.Validate(project);
            if (result.Success)
            {
                _dbContext.BeginTransaction();
                _createProjectCommand.Execute(project);
                _dbContext.Commit();
            }

            return this.Response.AsJson(result);
        }

        public dynamic ProjectView(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            // look up the project - return a 404 if we can't load it
            ProjectModel project = _projectRepo.GetById(projectId);
            if (project == null)
            {
                return HttpStatusCode.NotFound;
            }

            // set up the view model
            ProjectViewViewModel viewModel = new ProjectViewViewModel();
            viewModel.ProjectId = projectId;
            viewModel.ProjectName = project.Name;
            return Response.AsJson(viewModel);
        }
    }
}
