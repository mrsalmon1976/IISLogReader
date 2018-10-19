using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace IISLogReader
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            _logger.Info("IISLogReader Console starting up");

            try
            {
                HostFactory.Run(
                    configuration =>
                    {
                        configuration.Service<LogReaderService>(
                            service =>
                            {
                                service.ConstructUsing(x => new LogReaderService());
                                service.WhenStarted(x => x.Start());
                                service.WhenStopped(x => x.Stop());
                            });

                        configuration.RunAsLocalSystem();

                        configuration.SetServiceName("IISLogReader");
                        configuration.SetDisplayName("IISLogReader");
                        configuration.SetDescription("The IISLogReader service.");
                    });
                _logger.Info("IISLogReader Console shutting down");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "IISLogReader crashed!");
            }


        }
    }
}
