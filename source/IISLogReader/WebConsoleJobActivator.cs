using Hangfire;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Db;
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
            _container.Register<IJobRegistrationService, JobRegistrationService>();

            _container.Register<ResetRequestAggregatesCommand>();
            _container.Register<SetLogFileUnprocessedCommand>();
        }

        public override object ActivateJob(Type type)
        {
            return _container.Resolve(type);
        }
    }
}
