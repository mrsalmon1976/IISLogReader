using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<UserModel> GetAll();

        UserModel GetById(Guid id);

        UserModel GetByUserName(string userName);

    }

    public class UserRepository : IUserRepository
    {

        private IDbContext _dbContext;

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<UserModel> GetAll()
        {
            return _dbContext.Query<UserModel>("SELECT * FROM Users ORDER BY UserName");
        }

        public UserModel GetById(Guid id)
        {
            const string sql = "SELECT * FROM Users WHERE Id = @Id";
            return _dbContext.Query<UserModel>(sql, new { Id = id }).SingleOrDefault();
        }

        public UserModel GetByUserName(string userName)
        {
            const string sql = "SELECT * FROM Users WHERE UserName = @UserName";
            return _dbContext.Query<UserModel>(sql, new { UserName = userName }).SingleOrDefault();
        }

    }
}
