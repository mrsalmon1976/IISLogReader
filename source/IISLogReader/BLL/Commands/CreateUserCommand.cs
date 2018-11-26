using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Security;

namespace IISLogReader.BLL.Commands
{
    public interface ICreateUserCommand
    {
        UserModel Execute(string userName, string password, string role);
    }
    public class CreateUserCommand : ICreateUserCommand
    {
        private IDbContext _dbContext;
        private IUserValidator _userValidator;
        private IPasswordProvider _passwordProvider;

        public CreateUserCommand(IDbContext dbContext, IUserValidator userValidator, IPasswordProvider passwordProvider)
        {
            _dbContext = dbContext;
            _userValidator = userValidator;
            _passwordProvider = passwordProvider;
        }

        public UserModel Execute(string userName, string password, string role)
        {
            UserModel user = new UserModel();
            user.UserName = userName;
            user.Password = password;
            user.Role = role;

            // validate before we try to hash the password
            ValidationResult result = _userValidator.Validate(user);
            if (!result.Success)
            {
                throw new ValidationException(result.Messages);
            }

            // hash the password
            user.Password = _passwordProvider.HashPassword(password, _passwordProvider.GenerateSalt());

            // insert new record
            string sql = @"INSERT INTO Users (Id, UserName, Password, Role) VALUES (@Id, @UserName, @Password, @Role)";
            _dbContext.ExecuteNonQuery(sql, user);

            return user;
        }
    }
}
