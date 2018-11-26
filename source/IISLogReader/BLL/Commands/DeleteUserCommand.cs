using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Commands
{
    public interface IDeleteUserCommand
    {
        void Execute(Guid userId);
    }
    public class DeleteUserCommand : IDeleteUserCommand
    {
        private IDbContext _dbContext;

        public DeleteUserCommand(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute(Guid userId)
        {
            const string sql = @"DELETE FROM Users WHERE Id = @Id";
            _dbContext.ExecuteNonQuery(sql, new { Id = userId });
        }
    }
}
