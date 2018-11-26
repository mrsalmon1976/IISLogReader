using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Repositories;

namespace IISLogReader.BLL.Security
{
    public class UserMapper : IUserMapper
    {
        private IUserRepository _userRepo;

        public UserMapper(IUserRepository userRepo)
        {
            this._userRepo = userRepo;
        }

        public virtual IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            UserIdentity ui = null;
            UserModel user = _userRepo.GetById(identifier);
            if (user != null)
            {
                ui = new UserIdentity();
                ui.Id = user.Id;
                ui.UserName = user.UserName;
                if (user.Role == Roles.Admin)
                {
                    ui.Claims = Claims.AllClaims;
                }
                else
                {
                    ui.Claims = new string[] { };
                }
            }
            return ui;
        }
    }
}
