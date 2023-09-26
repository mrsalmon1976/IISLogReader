using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.Configuration
{
    public interface IAppSettings
    {
        string DataDirectory { get; }

        /// <summary>
        /// Gets the folder where log files are stored for processing.
        /// </summary>
        string LogFileProcessingDirectory { get; }

        /// <summary>
        /// Gets the port used for the application.
        /// </summary>
        int Port { get; }

        string SecureKey { get; }


    }

    public class AppSettings : IAppSettings
    {
        public string DataDirectory
        {
            get
            {
                return Path.Combine(AppContext.BaseDirectory, "Data");
            }
        }

        /// <summary>
        /// Gets the folder where log files are stored for processing.
        /// </summary>
        public string LogFileProcessingDirectory
        {
            get
            {
                return Path.Combine(this.DataDirectory, "LogFileProcessing");
            }
        }

        /// <summary>
        /// Gets the port used for the application.
        /// </summary>
        public int Port 
        {
            get
            {
                return Int32.Parse(ConfigurationManager.AppSettings["Port"]);
            }
        }

        /// <summary>
        /// Gets/sets the application security key
        /// </summary>
        public string SecureKey
        {
            get
            {
                return (ConfigurationManager.AppSettings["SecureKey"] ?? "This is the Def@ult IISLogReader security key if it is not specified in app.config");
            }
        }


    }
}
