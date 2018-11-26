using IISLogReader.BLL.Data;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Commands
{
    public interface IUpdateUserPasswordCommand
    {
        void Execute(string userName, string password);
    }

    public class UpdateUserPasswordCommand : IUpdateUserPasswordCommand
    {
        private readonly IDbContext _dbContext;
        private readonly IUserRepository _userRepo;
        private readonly IPasswordProvider _passwordProvider;

        public UpdateUserPasswordCommand(IDbContext dbContext, IUserRepository userRepo, IPasswordProvider passwordProvider)
        {
            _dbContext = dbContext;
            _userRepo = userRepo;
            _passwordProvider = passwordProvider;
        }

        public void Execute(string userName, string password)
        {
            if (String.IsNullOrEmpty(password))
            {
                throw new ValidationException("No password supplied");
            }

            var user = _userRepo.GetByUserName(userName);
            string newPassword = _passwordProvider.HashPassword(password, _passwordProvider.GenerateSalt());

            const string sql = "UPDATE Users SET Password = @Password WHERE Id = @Id";
            _dbContext.ExecuteNonQuery(sql, new { Password = newPassword, Id = user.Id });
        }
    }
}
