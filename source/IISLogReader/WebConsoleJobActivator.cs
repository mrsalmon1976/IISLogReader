using Hangfire;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Db;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Services;
using IISLogReader.Configuration;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader
{
    public class WebConsoleJobActivator : JobActivator
    {
        private TinyIoCContainer _container;
        private IDbContextFactory _dbContextFactory;

        public WebConsoleJobActivator()
        {
            _container = new TinyIoCContainer();

            _dbContextFactory = new DbContextFactory(new AppSettings());

            _container.Register<IDbContext>(_dbContextFactory.GetDbContext());

            // repositories
            _container.Register<ILogFileRepository, LogFileRepository>();
            _container.Register<IProjectRepository, ProjectRepository>();
            _container.Register<IProjectRequestAggregateRepository, ProjectRequestAggregateRepository>();
            _container.Register<IRequestRepository, RequestRepository>();

            // services
            _container.Register<IJobRegistrationService, JobRegistrationService>();
            _container.Register<IRequestAggregationService, RequestAggregationService>();

            _container.Register<ResetRequestAggregatesCommand>();
            _container.Register<SetLogFileUnprocessedCommand>();
        }

        public override object ActivateJob(Type type)
        {
            return _container.Resolve(type);
        }
    }
}
