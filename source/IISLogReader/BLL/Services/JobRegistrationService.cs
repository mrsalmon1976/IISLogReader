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
            BackgroundJob.Enqueue<SetLogFileUnprocessedCommand>(x => x.Execute(logFileId));
        }

        /// <summary>
        /// Registers a background job that runs through all the requests for a specific log file, and recalculates the aggregates 
        /// for those requests using the project aggregates set by the user.
        /// </summary>
        /// <param name="logFileId"></param>
        public void RegisterAggregateRequestJob(int logFileId)
        {
            BackgroundJob.Enqueue<ResetRequestAggregatesCommand>(x => x.Execute(logFileId));
            //Func<Task> t = delegate {
            //    using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            //    {
            //        var cmd = new ResetRequestAggregatesCommand(dbContext);
            //        cmd.Execute(logFileId);
            //        return Task.CompletedTask;
            //    }
            //};
            //BackgroundJob.Enqueue(() => t.Invoke());
        }

    }
}
