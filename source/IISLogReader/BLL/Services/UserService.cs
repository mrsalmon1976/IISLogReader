using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Services
{

    public interface IUserService
    {
        UserModel InitialiseAdminUser();
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ICreateUserCommand _createUserCommand;

        public const string AdminUserName = "admin";
        public const string AdminDefaultPassword = "admin";

        public UserService(IUserRepository userRepo, ICreateUserCommand createUserCommand)
        {
            _userRepo = userRepo;
            _createUserCommand = createUserCommand;
        }

        public UserModel InitialiseAdminUser()
        {
            UserModel user = _userRepo.GetByUserName(AdminUserName);
            if (user == null)
            {
                user = _createUserCommand.Execute(AdminUserName, AdminDefaultPassword, Roles.Admin);
            }
            return user;
        }
    }
}
