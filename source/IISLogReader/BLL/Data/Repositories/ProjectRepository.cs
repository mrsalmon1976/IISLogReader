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
        IEnumerable<ProjectModel> GetAll(IDbContext dbContext);
    }
    public class ProjectRepository : IProjectRepository
    {
        public IEnumerable<ProjectModel> GetAll(IDbContext dbContext)
        {
            const string sql = "SELECT * FROM Projects ORDER BY Name";
            return dbContext.Query<ProjectModel>(sql);
        }
    }
}
