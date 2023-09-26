using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.ModelBinding;
using Nancy.Security;
using IISLogReader.Navigation;
using IISLogReader.ViewModels;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using IISLogReader.ViewModels.Project;
using AutoMapper;
using IISLogReader.BLL.Validators;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Repositories;
using IISLogReader.ViewModels.LogFile;
using System.Threading.Tasks;
using IISLogReader.BLL.Services;

namespace IISLogReader.Modules
{
    public class ProjectModule : DefaultSecureModule
    {

        private IDbContext _dbContext;
        private IProjectValidator _projectValidator;
        private ICreateProjectCommand _createProjectCommand;
        private IDeleteProjectCommand _deleteProjectCommand;
        private IProjectRepository _projectRepo;
        private ILogFileRepository _logFileRepo;
        private IRequestRepository _requestRepo;
        private IProjectRequestAggregateRepository _projectRequestAggregateRepo;
        private IRequestAggregationService _requestAggregationService;

        public ProjectModule(IDbContext dbContext, IProjectValidator projectValidator
            , ICreateProjectCommand createProjectCommand, IDeleteProjectCommand deleteProjectCommand
            , IProjectRepository projectRepo, ILogFileRepository logFileRepo, IRequestRepository requestRepo
            , IProjectRequestAggregateRepository projectRequestAggregateRepo
            , IRequestAggregationService requestAggregationService)
        {
            _dbContext = dbContext;
            _projectValidator = projectValidator;
            _createProjectCommand = createProjectCommand;
            _deleteProjectCommand = deleteProjectCommand;
            _projectRepo = projectRepo;
            _logFileRepo = logFileRepo;
            _requestRepo = requestRepo;
            _projectRequestAggregateRepo = projectRequestAggregateRepo;
            _requestAggregationService = requestAggregationService;

            Post[Actions.Project.Aggregates()] = x =>
            {
                return Aggregates(x.projectId);
            };
            Post[Actions.Project.Files()] = x =>
            {
                return Files(x.projectId);
            };

            Post[Actions.Project.AvgLoadTimes()] = x =>
            {
                return AvgLoadTimes(x.projectId);
            };

            Post[Actions.Project.Delete()] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectEdit });
                return DeleteProject(x.projectId);
            };

            Get[Actions.Project.Overview()] = x =>
            {
                return Overview(x.projectId);
            };

            Get[Actions.Project.RequestsByAggregate()] = x =>
            {
                return RequestsByAggregate(x.projectId);
            };
            Post[Actions.Project.RequestsByAggregateDetail()] = x =>
            {
                return RequestsByAggregateDetail(x.projectId);
            };

            Get[Actions.Project.View()] = x =>
            {
                return ProjectView(x.projectId);
            };

            Post[Actions.Project.Save] = x =>
            {
                this.RequiresClaims(new[] { Claims.ProjectEdit });
                return ProjectSave();
            };

        }

        public dynamic Aggregates(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            IEnumerable<ProjectRequestAggregateModel> aggregates = _projectRequestAggregateRepo.GetByProject(projectId);
            return this.Response.AsJson<IEnumerable<ProjectRequestAggregateModel>>(aggregates);
        }

        public dynamic AvgLoadTimes(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            IEnumerable<RequestPageLoadTimeModel> loadTimes = _requestRepo.GetPageLoadTimes(projectId);
            return this.Response.AsJson(loadTimes);
        }

        public dynamic DeleteProject(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            _deleteProjectCommand.Execute(projectId);
            return this.Response.AsJson("");
        }

        public dynamic Files(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            IEnumerable<LogFileModel> logFiles = _logFileRepo.GetByProject(projectId);
            IEnumerable<LogFileViewModel> logFileViewModels = logFiles.Select(x => Mapper.Map<LogFileModel, LogFileViewModel>(x));

            return this.Response.AsJson<IEnumerable<LogFileViewModel>>(logFileViewModels);
        }

        public dynamic ProjectSave()
        {
            ProjectFormViewModel model = this.Bind<ProjectFormViewModel>();
            ProjectModel project = Mapper.Map<ProjectFormViewModel, ProjectModel>(model);

            ValidationResult validationResult = _projectValidator.Validate(project);
            if (validationResult.Success)
            {
                _dbContext.BeginTransaction();
                project = _createProjectCommand.Execute(project);
                _dbContext.Commit();
            }

            SaveResultModel result = new SaveResultModel(project.Id.ToString(), validationResult.Success, validationResult.Messages.ToArray());
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
            viewModel.UnprocessedCount = _projectRepo.GetUnprocessedLogFileCount(projectId);
            viewModel.IsProjectEditor = this.Context.CurrentUser.HasClaim(Claims.ProjectEdit);

            AddScript(Scripts.ProjectView);
            return this.View[Views.Project.View, viewModel];
        }

        public dynamic Overview(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            Task<long> totalRequestCountTask = _requestRepo.GetTotalRequestCountAsync(projectId);
            Task<IEnumerable<RequestStatusCodeCount>> requestCodeSummaryTask = _requestRepo.GetStatusCodeSummaryAsync(projectId);
            Task<int> logFileCount = _logFileRepo.GetCountByProjectAsync(projectId);

            Task.WhenAll(totalRequestCountTask, requestCodeSummaryTask, logFileCount);

            var groupedStatusCodes = _requestAggregationService.GetRequestStatusCodeSummary(requestCodeSummaryTask.Result);

            // set up the view model
            ProjectOverviewViewModel viewModel = new ProjectOverviewViewModel();
            viewModel.ProjectId = projectId;
            viewModel.TotalRequestCount = totalRequestCountTask.Result;
            viewModel.SuccessRequestCount = groupedStatusCodes.SuccessCount;
            viewModel.RedirectionRequestCount = groupedStatusCodes.RedirectionCount;
            viewModel.ClientErrorRequestCount = groupedStatusCodes.ClientErrorCount;
            viewModel.ServerErrorRequestCount = groupedStatusCodes.ServerErrorCount;
            viewModel.LogFileCount = logFileCount.Result;

            return this.Response.AsJson(viewModel);
        }


        public dynamic RequestsByAggregate(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            string uriStemAggregate = Request.Query["uri"];

            // look up the project - return a 404 if we can't load it
            ProjectModel project = _projectRepo.GetById(projectId);
            if (project == null)
            {
                return HttpStatusCode.NotFound;
            }

            // set up the view model
            RequestUriAggregateViewModel viewModel = new RequestUriAggregateViewModel();
            viewModel.ProjectId = projectId;
            viewModel.ProjectName = project.Name;
            viewModel.UriStemAggregate = uriStemAggregate;

            AddScript(Scripts.RequestsByAggregateView);
            return this.View[Views.Project.RequestByAggregate, viewModel];
        }

        public dynamic RequestsByAggregateDetail(dynamic pId)
        {
            // make sure the id is a valid integer
            int projectId = 0;
            if (!Int32.TryParse((pId ?? "").ToString(), out projectId))
            {
                return HttpStatusCode.BadRequest;
            }

            string uriStemAggregate = Request.Form["uri"];

            IEnumerable<RequestModel> requests = _requestRepo.GetByUriStemAggregate(projectId, uriStemAggregate);
            return this.Response.AsJson(requests);
        }

    }
}
