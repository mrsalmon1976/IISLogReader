using IISLogReader.BLL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Data.Repositories
{
    public interface IRequestRepository
    {
        IEnumerable<RequestPageLoadTimeModel> GetPageLoadTimes(int projectId);
    }

    public class RequestRepository :IRequestRepository
    {

        private IDbContext _dbContext;

        public RequestRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<RequestPageLoadTimeModel> GetPageLoadTimes(int projectId)
        {
            const string sql = @"SELECT r.UriStem, COUNT(r.UriStem) AS RequestCount, AVG(r.TimeTaken) AS AvgTimeTakenMilliseconds FROM Requests r
                INNER JOIN LogFiles lf on r.LogFileId = lf.Id
                WHERE lf.ProjectId = @ProjectId
                GROUP BY r.UriStem
                ORDER BY AvgTimeTakenMilliseconds DESC";
            return _dbContext.Query<RequestPageLoadTimeModel>(sql, new { ProjectId = projectId });

        }
    }
}
