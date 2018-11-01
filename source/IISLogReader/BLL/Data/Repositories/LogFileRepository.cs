using IISLogReader.BLL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Data.Repositories
{
    public interface ILogFileRepository
    {
        LogFileModel GetByHash(int projectId, string md5Hash);

        IEnumerable<LogFileModel> GetByProject(int projectId);

    }
    public class LogFileRepository : ILogFileRepository
    {
        private IDbContext _dbContext;

        public LogFileRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public LogFileModel GetByHash(int projectId, string md5Hash)
        {
            const string sql = "SELECT * FROM LogFiles WHERE ProjectId = @ProjectId AND FileHash = @Hash";
            return _dbContext.Query<LogFileModel>(sql, new { ProjectId = projectId, Hash = md5Hash}).SingleOrDefault();
        }

        public IEnumerable<LogFileModel> GetByProject(int projectId)
        {
            const string sql = "SELECT * FROM LogFiles WHERE ProjectId = @ProjectId";
            return _dbContext.Query<LogFileModel>(sql, new { ProjectId = projectId });
        }

    }
}
