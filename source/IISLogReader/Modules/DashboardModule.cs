using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using IISLogReader.Navigation;
using IISLogReader.ViewModels.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.Modules
{
    public class DashboardModule : DefaultSecureModule
    {
        public DashboardModule() : base()
        {
            Get[Actions.Dashboard.Default] = (x) =>
            {
                AddScript(Scripts.DashboardView);
                return this.Default();
            };
        }

        public dynamic Default()
        {
            var currentUser = this.Context.CurrentUser;
            DashboardViewModel model = new DashboardViewModel();
            model.IsProjectEditor = currentUser.HasClaim(Claims.ProjectEdit);
            return this.View[Views.Dashboard.Default, model]; ;

        }

    }
}
