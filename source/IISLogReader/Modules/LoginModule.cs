using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using IISLogReader.Navigation;
using IISLogReader.ViewModels.Login;
using Nancy.Responses.Negotiation;
using IISLogReader.ViewModels;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using IISLogReader.BLL.Repositories;

namespace IISLogReader.Modules
{
    public class LoginModule : DefaultModule
    {
        private IUserRepository _userRepo;
        private IPasswordProvider _passwordProvider;

        public LoginModule(IUserRepository userRepo, IPasswordProvider passwordProvider)
        {
            _userRepo = userRepo;
            _passwordProvider = passwordProvider;

            Get["/"] = x =>
            {
                return this.Response.AsRedirect(Actions.Login.Default);
            };

            Get[Actions.Login.Default] = x =>
            {
                AddScript(Scripts.LoginView);
                return this.LoginGet();
            };

            Post[Actions.Login.Default] = x =>
            {
                return LoginPost();
            };

            Get[Actions.Login.Logout] = x =>
            {
                return this.Logout(Actions.Login.Default);
            };

        }

        public dynamic LoginGet()
        {
            var model = this.Bind<LoginViewModel>();
            if (this.Context.CurrentUser != null)
            {
                return this.Response.AsRedirect(Actions.Dashboard.Default);
            }
            if (String.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = Actions.Dashboard.Default;
            }
            return this.View[Views.Login, model];
        }

        public dynamic LoginPost()
        {
            LoginViewModel model = this.Bind<LoginViewModel>();
            BasicResult result = new BasicResult(false);

            // if the email or password hasn't been supplied, exit
            if ((!String.IsNullOrWhiteSpace(model.UserName)) && (!String.IsNullOrWhiteSpace(model.Password)))
            {
                // get the user
                UserModel user = _userRepo.GetByUserName(model.UserName);
                if (user != null && _passwordProvider.CheckPassword(model.Password, user.Password))
                {
                    result.Success = true;
                    return this.Login(user.Id, DateTime.Now.AddDays(1));
                }
            }

            return this.Response.AsJson(result);
        }
    }
}
