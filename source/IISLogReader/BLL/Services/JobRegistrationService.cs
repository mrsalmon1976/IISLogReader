using Hangfire;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Services
{
    public interface IJobRegistrationService
    {
        /// <summary>
        /// Registers a background job that reset a log file to mark it as unprocessed and updates all other data 
        /// relevant to this state (e.g. UriStemAggregate for all requests is set to NULL).
        /// </summary>
        /// <param name="logFileId"></param>
        void RegisterResetProcessedLogFileJob(int logFileId);

        /// <summary>
        /// Registers a background job that runs through all the requests for a specific log file, and recalculates the aggregates 
        /// for those requests using the project aggregates set by the user.
        /// </summary>
        /// <param name="logFileId"></param>
        void RegisterAggregateRequestJob(int logFileId);

        /// <summary>
        /// Job registered once a file has been uploaded, to process the contents of the log file.
        /// </summary>
        /// <param name="logFileId"></param>
        /// <param name="filePath"></param>
        void RegisterProcessLogFileJob(int logFileId, string filePath);
    }

    public class JobRegistrationService : IJobRegistrationService
    {

        /// <summary>
        /// Registers a background job that reset a log file to mark it as unprocessed and updates all other data 
        /// relevant to this state (e.g. UriStemAggregate for all requests is set to NULL).
        /// </summary>
        /// <param name="logFileId"></param>
        public void RegisterResetProcessedLogFileJob(int logFileId)
        {
            BackgroundJob.Enqueue<JobExecutionService>(x => x.ExecuteResetProcessedLogFileJob(logFileId));
        }

        /// <summary>
        /// Registers a background job that runs through all the requests for a specific log file, and recalculates the aggregates 
        /// for those requests using the project aggregates set by the user.
        /// </summary>
        /// <param name="logFileId"></param>
        public void RegisterAggregateRequestJob(int logFileId)
        {
            BackgroundJob.Enqueue<JobExecutionService>(x => x.ExecuteAggregateRequestJob(logFileId));
        }

        /// <summary>
        /// Job registered once a file has been uploaded, to process the contents of the log file.
        /// </summary>
        /// <param name="logFileId"></param>
        /// <param name="filePath"></param>
        public void RegisterProcessLogFileJob(int logFileId, string filePath)
        {
            BackgroundJob.Enqueue<JobExecutionService>(x => x.ExecuteProcessLogFileJob(logFileId, filePath));
        }
    }
}
