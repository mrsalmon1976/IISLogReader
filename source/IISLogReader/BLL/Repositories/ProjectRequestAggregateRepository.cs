using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Repositories
{
    public interface IProjectRequestAggregateRepository
    {
        ProjectRequestAggregateModel GetById(int id);

        IEnumerable<ProjectRequestAggregateModel> GetByProject(int projectId);

    }
    public class ProjectRequestAggregateRepository : IProjectRequestAggregateRepository
    {
        private IDbContext _dbContext;

        public ProjectRequestAggregateRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ProjectRequestAggregateModel GetById(int id)
        {
            const string sql = "SELECT * FROM ProjectRequestAggregates WHERE Id = @Id";
            return _dbContext.Query<ProjectRequestAggregateModel>(sql, new { Id = id }).SingleOrDefault();
        }

        public IEnumerable<ProjectRequestAggregateModel> GetByProject(int projectId)
        {
            const string sql = "SELECT * FROM ProjectRequestAggregates WHERE ProjectId = @ProjectId ORDER BY Id";
            return _dbContext.Query<ProjectRequestAggregateModel>(sql, new { ProjectId = projectId });
        }

    }
}
