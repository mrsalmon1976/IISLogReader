using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Nancy.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using IISLogReader.Configuration;
using System.IO;
using SystemWrapper.IO;
using IISLogReader.BLL.Security;
using Encryption;
using AutoMapper;
using IISLogReader.BLL.Models;
using IISLogReader.ViewModels.User;
using System.Diagnostics;
using IISLogReader.BLL.Data;
using IISLogReader.ViewModels.Project;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Services;
using IISLogReader.BLL.Data.Db;
using IISLogReader.BLL.Validators;

namespace IISLogReader
{

    public class WebConsoleBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {

            // override maximum JSON length for Nancy
            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;

            // register settings first as we will use that below
            IAppSettings settings = new AppSettings();
            container.Register<IAppSettings>(settings);

            // IO Wrappers
            container.Register<IDirectoryWrap, DirectoryWrap>();
            //container.Register<IPathWrap, PathWrap>();
            container.Register<IFileWrap, FileWrap>();
            //container.Register<IPathHelper, PathHelper>();

            // security
            container.Register<IEncryptionProvider, AESGCM>();
            container.Register<IPasswordProvider, PasswordProvider>();

            // set up mappings
            Mapper.Initialize((cfg) => {
                cfg.CreateMap<ProjectFormViewModel, ProjectModel>();
                cfg.CreateMap<UserViewModel, UserModel>();//.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DocumentId));
            });

            // Initialise the database only on application start
            IDbContextFactory dbContextFactory = new DbContextFactory(settings);
            container.Register<IDbContextFactory>(dbContextFactory);
            using (IDbContext dbc = dbContextFactory.GetDbContext())
            {
                dbc.Initialise();

                // make sure an administrator exists
                IUserRepository userRepo = new UserRepository(dbc);
                IUserValidator userValidator = new UserValidator(userRepo);
                IUserService userService = new UserService(userRepo, new CreateUserCommand(dbc, userValidator, new PasswordProvider()));
                userService.InitialiseAdminUser();
            }

        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            
            IAppSettings settings = container.Resolve<IAppSettings>();

            // register database context per request
            IDbContextFactory dbContextFactory = container.Resolve<IDbContextFactory>();
            container.Register<IDbContext>(dbContextFactory.GetDbContext());

            // validators
            container.Register<IUserValidator, UserValidator>();

            // repositories
            container.Register<ILogFileRepository, LogFileRepository>();
            container.Register<IProjectRepository, ProjectRepository>();
            container.Register<IRequestRepository, RequestRepository>();
            container.Register<IProjectRequestAggregateRepository, ProjectRequestAggregateRepository>();
            container.Register<IUserRepository, UserRepository>();

            // commands
            container.Register<ICreateLogFileCommand, CreateLogFileCommand>();
            container.Register<ICreateProjectCommand, CreateProjectCommand>();
            container.Register<ICreateProjectRequestAggregateCommand, CreateProjectRequestAggregateCommand>();
            container.Register<ICreateRequestBatchCommand, CreateRequestBatchCommand>();
            container.Register<ICreateUserCommand, CreateUserCommand>();
            container.Register<IDeleteLogFileCommand, DeleteLogFileCommand>();
            container.Register<IDeleteProjectCommand, DeleteProjectCommand>();
            container.Register<IDeleteProjectRequestAggregateCommand, DeleteProjectRequestAggregateCommand>();
            container.Register<IDeleteUserCommand, DeleteUserCommand>();
            container.Register<IProcessLogFileCommand, ProcessLogFileCommand>();
            container.Register<ISetLogFileUnprocessedCommand, SetLogFileUnprocessedCommand>();
            container.Register<IUpdateUserPasswordCommand, UpdateUserPasswordCommand>();

            // services
            container.Register<IJobRegistrationService, JobRegistrationService>();
            container.Register<IUserService, UserService>();

            container.Register<IUserMapper, UserMapper>();

        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            var formsAuthConfiguration = new FormsAuthenticationConfiguration()
            {
                RedirectUrl = "~/login",
                UserMapper = container.Resolve<IUserMapper>(),
                DisableRedirect = context.Request.IsAjaxRequest()    
            };
            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);

            // set shared ViewBag details here
            context.ViewBag.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            context.ViewBag.Scripts = new List<string>();
            context.ViewBag.Claims = new List<string>();

            // before the request builds up, if there is a logged in user then set the user info
            pipelines.BeforeRequest += (ctx) =>
            {
                if (ctx.CurrentUser != null)
                {
                    // set the current user name so it is available for the layout
                    ctx.ViewBag.CurrentUserName = ctx.CurrentUser.UserName;
                    if (ctx.CurrentUser.Claims != null)
                    {
                        ctx.ViewBag.Claims = new List<string>(ctx.CurrentUser.Claims);
                    }

                    // load a list of projects
                    IDbContext dbContext = container.Resolve<IDbContext>();
                    IProjectRepository projectRepo = container.Resolve<IProjectRepository>();
                    ctx.ViewBag.Projects = projectRepo.GetAll().ToList();
                }

                return null;
            };

            // clean up anything that needs to be
            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
                {
                    IDbContext dbContext = container.Resolve<IDbContext>();
                    dbContext.Dispose();
                });
        }

    }
}
