using IISLogReader.BLL.Data;
using IISLogReader.BLL.Lookup;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Commands
{
    public interface IResetRequestAggregatesCommand
    {
        void Execute(int logFileId);
    }

    public class ResetRequestAggregatesCommand : IResetRequestAggregatesCommand
    {
        private readonly IDbContext _dbContext;
        private readonly ILogFileRepository _logFileRepo;
        private readonly IRequestRepository _requestRepo;
        private readonly IProjectRequestAggregateRepository _projectRequestAggregateRepo;
        private readonly IRequestAggregationService _requestAggregationService;

        private ILogger _logger = LogManager.GetCurrentClassLogger();

        public ResetRequestAggregatesCommand(IDbContext dbContext, ILogFileRepository logFileRepo, IRequestRepository requestRepo, IProjectRequestAggregateRepository projectRequestAggregateRepo, IRequestAggregationService requestAggregationService)
        {
            _dbContext = dbContext;
            _logFileRepo = logFileRepo;
            _requestRepo = requestRepo;
            _projectRequestAggregateRepo = projectRequestAggregateRepo;
            _requestAggregationService = requestAggregationService;
        }

        public void Execute(int logFileId)
        {
            // load all the requests for the log file
            IEnumerable<RequestModel> requests =_requestRepo.GetByLogFile(logFileId);

            // only apply aggregates if we have requests!
            if (requests.Any())
            {

                // load all the aggregates for the project
                LogFileModel logFile = _logFileRepo.GetById(logFileId);
                IEnumerable<ProjectRequestAggregateModel> requestAggregates = _projectRequestAggregateRepo.GetByProject(logFile.ProjectId);

                // run through the requests and apply the configured aggregates - if the value changes then update in the database
                foreach (var req in requests)
                {
                    const string usql = "UPDATE Requests SET UriStemAggregate = @UriStemAggregate WHERE Id = @RequestId";

                    string uriStemAggregate = _requestAggregationService.GetAggregatedUriStem(req.UriStem, requestAggregates);
                    if (uriStemAggregate != req.UriStemAggregate)
                    {
                        _dbContext.ExecuteNonQuery(usql, new { UriStemAggregate = uriStemAggregate, RequestId = req.Id });
                    }
                }
            }

            // mark the log file as processed
            string sql = "UPDATE LogFiles SET Status = @Status WHERE Id = @LogFileId";
            _dbContext.ExecuteNonQuery(sql, new { LogFileId = logFileId, Status = LogFileStatus.Complete });
            _logger.Info("Marked LogFile {0} as Complete", logFileId);
        }
    }
}
