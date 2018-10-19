using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using IISLogReader.BLL.Data.Models;
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
                return this.View[Views.Dashboard.Default, this.Default()];
            };
        }

        public DashboardViewModel Default()
        {
            DashboardViewModel model = new DashboardViewModel();
            return model;

        }

    }
}
