using IISLogReader.BLL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Data.Repositories
{
    public interface IProjectRepository
    {
        IEnumerable<ProjectModel> GetAll();

        ProjectModel GetById(int projectId);

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

    }
}
