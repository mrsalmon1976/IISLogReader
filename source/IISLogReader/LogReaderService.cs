using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using System.Net.Sockets;
using NLog;
using IISLogReader.Configuration;
using Hangfire;
using Hangfire.SQLite;
using System.IO;

namespace IISLogReader
{
    public class LogReaderService
    {
        private NancyHost _host;
        private BackgroundJobServer _jobServer;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Start()
        {
            _logger.Info("IISLogReader Windows Service starting");
            IAppSettings appSettings = new AppSettings();

            // make sure the data directory exists
            Directory.CreateDirectory(appSettings.DataDirectory);

            _logger.Info("Starting Nancy host");
            var hostConfiguration = new HostConfiguration
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };

            string url = String.Format("http://localhost:{0}", appSettings.Port);
            _host = new NancyHost(hostConfiguration, new Uri(url));
            _host.Start();

            // fire up the background job processor
            _logger.Info("Starting background job server");
            var sqlLiteOptions = new SQLiteStorageOptions();
            string connString = String.Format("Data Source={0}\\Data\\IISLogReaderJobs.db;Version=3;", AppDomain.CurrentDomain.BaseDirectory);
            GlobalConfiguration.Configuration.UseSQLiteStorage(connString, sqlLiteOptions);
            GlobalConfiguration.Configuration.UseActivator(new WebConsoleJobActivator());
            var jobServerOptions = new BackgroundJobServerOptions { WorkerCount = 1 };
            _jobServer = new BackgroundJobServer(jobServerOptions);

        }

        public void Stop()
        {

            // shut down the background processor
            _logger.Info("Shutting down background job server");
            _jobServer.Dispose();

            // shut down the Nancy host
            _logger.Info("Shutting down Nancy host");
            _host.Stop();
            _host.Dispose();

            _logger.Info("IISLogReader Service stopped");
        }
    }
}
