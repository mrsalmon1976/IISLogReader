using Hangfire;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Db;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;

namespace IISLogReader.BLL.Services
{
    public interface IJobExecutionService
    {
        void ExecuteResetProcessedLogFileJob(int logFileId);
             
        void ExecuteAggregateRequestJob(int logFileId);
             
        void ExecuteProcessLogFileJob(int logFileId, string filePath);
    }

    public class JobExecutionService : IJobExecutionService
    {

        private readonly IDbContextFactory _dbContextFactory;

        public JobExecutionService(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void ExecuteResetProcessedLogFileJob(int logFileId)
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                ISetLogFileUnprocessedCommand cmd = new SetLogFileUnprocessedCommand(dbContext
                    , new JobRegistrationService()
                    );

                dbContext.BeginTransaction();
                cmd.Execute(logFileId);
                dbContext.Commit();
            }
        }

        /// <summary>
        /// Registers a background job that runs through all the requests for a specific log file, and recalculates the aggregates 
        /// for those requests using the project aggregates set by the user.
        /// </summary>
        /// <param name="logFileId"></param>
        public void ExecuteAggregateRequestJob(int logFileId)
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                IResetRequestAggregatesCommand cmd = new ResetRequestAggregatesCommand(dbContext
                    , new LogFileRepository(dbContext)
                    , new RequestRepository(dbContext)
                    , new ProjectRequestAggregateRepository(dbContext)
                    , new RequestAggregationService()
                    );

                dbContext.BeginTransaction();
                cmd.Execute(logFileId);
                dbContext.Commit();
            }
        }

        /// <summary>
        /// Job registered once a file has been uploaded, to process the contents of the log file.
        /// </summary>
        /// <param name="logFileId"></param>
        /// <param name="filePath"></param>
        public void ExecuteProcessLogFileJob(int logFileId, string filePath)
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                IProcessLogFileCommand cmd = new ProcessLogFileCommand(dbContext
                    , new LogFileRepository(dbContext)
                    , new CreateRequestBatchCommand(dbContext, new RequestValidator())
                    , new JobRegistrationService()
                    , new FileWrap()
                    );

                dbContext.BeginTransaction();
                cmd.Execute(logFileId, filePath);
                dbContext.Commit();
            }
        }
    }
}
