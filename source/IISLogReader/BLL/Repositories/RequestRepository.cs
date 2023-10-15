﻿using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Repositories
{
    public interface IRequestRepository
    {

        IEnumerable<RequestModel> GetByLogFile(int logFileId);

        IEnumerable<RequestModel> GetByUriStemAggregate(int projectId, string uriStemAggregate);

        IEnumerable<RequestPageLoadTimeModel> GetPageLoadTimes(int projectId);

        Task<long> GetTotalRequestCountAsync(int projectId);

        Task<IEnumerable<RequestStatusCodeCount>> GetStatusCodeSummaryAsync(int projectId);

        Task<IEnumerable<RequestStatusCodeCount>> GetServerErrorStatusCodeSummaryAsync(int projectId);

    }

    public class RequestRepository :IRequestRepository
    {

        private IDbContext _dbContext;

        public RequestRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<RequestModel> GetByLogFile(int logFileId)
        {
            const string sql = "SELECT * FROM Requests WHERE LogFileId = @LogFileId";
            return _dbContext.Query<RequestModel>(sql, new { LogFileId = logFileId });
        }

        public IEnumerable<RequestModel> GetByUriStemAggregate(int projectId, string uriStemAggregate)
        {
            const string sql = @"SELECT * 
                FROM Requests r 
                INNER JOIN LogFiles lf ON r.LogFileId = lf.Id
                WHERE lf.ProjectId = @ProjectId
                AND r.UriStemAggregate = @UriStemAggregate";
            return _dbContext.Query<RequestModel>(sql, new { ProjectId = projectId, UriStemAggregate = uriStemAggregate });
        }

        public IEnumerable<RequestPageLoadTimeModel> GetPageLoadTimes(int projectId)
        {
            const string sql = @"SELECT 
                    r.UriStemAggregate
                    , COUNT(r.UriStem) AS RequestCount
                    , AVG(r.TimeTaken) AS AvgTimeTakenMilliseconds 
                FROM Requests r
                INNER JOIN LogFiles lf on r.LogFileId = lf.Id
                WHERE lf.ProjectId = @ProjectId
                AND r.UriStemAggregate IS NOT NULL
                GROUP BY r.UriStemAggregate
                ORDER BY AvgTimeTakenMilliseconds DESC";
            return _dbContext.Query<RequestPageLoadTimeModel>(sql, new { ProjectId = projectId });

        }

        public async Task<long> GetTotalRequestCountAsync(int projectId)
        {
            const string sql = @"SELECT COUNT(r.Id) 
                FROM Requests r
                INNER JOIN LogFiles lf on r.LogFileId = lf.Id
                WHERE lf.ProjectId = @ProjectId";
            return await _dbContext.ExecuteScalarAsync<long>(sql, new { ProjectId = projectId });

        }

        public async Task<IEnumerable<RequestStatusCodeCount>> GetStatusCodeSummaryAsync(int projectId)
        {
            const string sql = @"SELECT r.ProtocolStatus AS StatusCode
                    , COUNT(r.Id) AS TotalCount
                FROM Requests r
                INNER JOIN LogFiles lf on r.LogFileId = lf.Id
                WHERE lf.ProjectId = @ProjectId
                GROUP BY r.ProtocolStatus";
            return await _dbContext.QueryAsync<RequestStatusCodeCount>(sql, new { ProjectId = projectId });
        }

        public async Task<IEnumerable<RequestStatusCodeCount>> GetServerErrorStatusCodeSummaryAsync(int projectId)
        {
            const string sql = @"SELECT r.ProtocolStatus AS StatusCode
	                , r.UriStemAggregate
                    , COUNT(r.Id) AS TotalCount
                FROM Requests r
                INNER JOIN LogFiles lf on r.LogFileId = lf.Id
                WHERE lf.ProjectId = @ProjectId
				AND r.ProtocolStatus >= 500
                AND r.UriStemAggregate IS NOT NULL
				GROUP BY r.ProtocolStatus, r.UriStemAggregate
				ORDER BY TotalCount DESC";
            return await _dbContext.QueryAsync<RequestStatusCodeCount>(sql, new { ProjectId = projectId });
        }

    }
}
