using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Repositories
{
    public interface ILogFileRepository
    {
        LogFileModel GetByHash(int projectId, string md5Hash);

        LogFileModel GetById(int logFileId);

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

        public LogFileModel GetById(int logFileId)
        {
            const string sql = "SELECT * FROM LogFiles WHERE Id = @Id";
            return _dbContext.Query<LogFileModel>(sql, new { Id = logFileId }).SingleOrDefault();
        }


        public IEnumerable<LogFileModel> GetByProject(int projectId)
        {
            const string sql = "SELECT * FROM LogFiles WHERE ProjectId = @ProjectId";
            return _dbContext.Query<LogFileModel>(sql, new { ProjectId = projectId });
        }

    }
}
