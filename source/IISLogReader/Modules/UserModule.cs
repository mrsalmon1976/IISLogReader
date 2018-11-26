using Nancy;
using Nancy.Authentication.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.ModelBinding;
using Nancy.Security;
using IISLogReader.BLL.Security;
using IISLogReader.Navigation;
using IISLogReader.ViewModels.User;
using IISLogReader.ViewModels;
using IISLogReader.BLL.Models;
using AutoMapper;
using IISLogReader.BLL.Validators;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Exceptions;

namespace IISLogReader.Modules
{
    public class UserModule : DefaultSecureModule
    {
        private readonly IUserRepository _userRepo;
        private readonly ICreateUserCommand _createUserCommand;
        private readonly IUpdateUserPasswordCommand _updateUserPasswordCommand;
        private readonly IDeleteUserCommand _deleteUserCommand;

        public UserModule(IUserRepository userRepo, ICreateUserCommand createUserCommand, IUpdateUserPasswordCommand updateUserPasswordCommand, IDeleteUserCommand deleteUserCommand) : base()
        {
            _userRepo = userRepo;
            _createUserCommand = createUserCommand;
            _updateUserPasswordCommand = updateUserPasswordCommand;
            _deleteUserCommand = deleteUserCommand;

            Get[Actions.User.Default] = (x) =>
            {
                AddScript(Scripts.UserView);
                return Default();
            };
            Post[Actions.User.Delete] = (x) =>
            {
                this.RequiresClaims(new[] { Claims.UserDelete });
                return DeleteUser();
            };
            Get[Actions.User.List] = (x) =>
            {
                this.RequiresClaims(new[] { Claims.UserList });
                return List();
            };
            Post[Actions.User.ChangePassword] = (x) =>
            {
                return ChangePassword();
            };
            Post[Actions.User.Save] = (x) =>
            {
                this.RequiresClaims(new[] { Claims.UserAdd });
                return Save();
            };
        }

        public dynamic ChangePassword()
        {
            string password = Request.Form["Password"];
            string confirmPassword = Request.Form["ConfirmPassword"];
            if (password.Length < 6)
            {
                return Response.AsJson<BasicResult>(new BasicResult(false, "Passwords must be at least 6 characters in length"));
            }
            if (password != confirmPassword)
            {
                return Response.AsJson<BasicResult>(new BasicResult(false, "Password and confirmation password do not match"));
            }

            // all ok - update the password
            _updateUserPasswordCommand.Execute(this.Context.CurrentUser.UserName, password);

            return Response.AsJson<BasicResult>(new BasicResult(true));
        }

        public dynamic Default()
        {
            UserViewModel model = new UserViewModel();
            model.Roles.AddRange(Roles.AllRoles);
            model.SelectedRole = Roles.User;
            return this.View[Views.User.Default, model];

        }
        public dynamic DeleteUser()
        {
            Guid userId = Request.Form["id"];
            _deleteUserCommand.Execute(userId);
            return HttpStatusCode.OK;
        }


        public dynamic List()
        {
            UserListViewModel model = new UserListViewModel();
            model.Users.AddRange(_userRepo.GetAll());
            return this.View[Views.User.ListPartial, model];
        }

        public dynamic Save()
        {

            var model = this.Bind<UserViewModel>();
            UserModel user = Mapper.Map<UserViewModel, UserModel>(model);

            // try and execute the command 
            BasicResult result = new BasicResult(true);
            try
            {
                if (model.Password != model.ConfirmPassword)
                {
                    throw new ValidationException("Password does not match confirmation password");
                }
                _createUserCommand.Execute(user.UserName, user.Password, user.Role);
            }
            catch (ValidationException vex)
            {
                result = new BasicResult(false, vex.ValidationErrors.ToArray());
            }
            catch (Exception ex)
            {
                result = new BasicResult(false, ex.Message);
            }

            return Response.AsJson(result);
        }


    }
}
