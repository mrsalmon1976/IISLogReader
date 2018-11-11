using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Repositories
{
    public interface IProjectRepository
    {
        IEnumerable<ProjectModel> GetAll();

        ProjectModel GetById(int projectId);

        /// <summary>
        /// Gets the number of log files attached to the project that have not been processed yet.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        int GetUnprocessedLogFileCount(int projectId);

    }
    public class ProjectRepository : IProjectRepository
    {
        private IDbContext _dbContext;

        public ProjectRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IEnumerable<ProjectModel> GetAll()
        {
            const string sql = "SELECT * FROM Projects ORDER BY Name COLLATE NOCASE ASC";
            return _dbContext.Query<ProjectModel>(sql);
        }

        public ProjectModel GetById(int projectId)
        {
            const string sql = "SELECT * FROM Projects WHERE Id = @Id";
            return _dbContext.Query<ProjectModel>(sql, new { Id = projectId }).SingleOrDefault();
        }

        /// <summary>
        /// Gets the number of log files attached to the project that have not been processed yet.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public int GetUnprocessedLogFileCount(int projectId)
        {
            const string sql = @"SELECT COUNT(lf.Id) 
                FROM LogFiles lf 
                WHERE lf.ProjectId = @ProjectId
                AND IsProcessed = 0";
            return _dbContext.ExecuteScalar<int>(sql, new { ProjectId = projectId });
        }

    }
}
