using IISLogReader.BLL.Data;
using IISLogReader.BLL.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Commands
{
    public interface ISetLogFileUnprocessedCommand
    {
        void Execute(int logFileId);
    }
    public class SetLogFileUnprocessedCommand : ISetLogFileUnprocessedCommand
    {
        private IDbContext _dbContext;
        private IJobRegistrationService _jobRegistrationService;
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        public SetLogFileUnprocessedCommand(IDbContext dbContext, IJobRegistrationService jobRegistrationService)
        {
            _dbContext = dbContext;
            _jobRegistrationService = jobRegistrationService;
        }

        public void Execute(int logFileId)
        {
            // set IsProcessed flag to 0
            string sql = "UPDATE LogFiles SET IsProcessed = 0 WHERE Id = @LogFileId";
            _dbContext.ExecuteNonQuery(sql, new { LogFileId = logFileId });
            _logger.Info("Marked LogFile {0} as Unprocessed", logFileId);

            // register the job to recalculate the UriStemAggregate for each request
            _jobRegistrationService.RegisterAggregateRequestJob(logFileId);
            _logger.Info("Registered AggregateReqeustJob for LogFileId {0}", logFileId);

        }
    }
}
