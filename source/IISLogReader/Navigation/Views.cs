using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.Navigation
{
    public class Views
    {
        public const string Login = "Views/LoginView.cshtml";

        public class Dashboard
        {
            public const string Default = "Views/Dashboard/DashboardView.cshtml";
        }

        public class Project
        {
            public const string View = "Views/Project/ProjectView.cshtml";
        }

        public class User
        {
            public const string ListPartial = "Views/User/_UserList.cshtml";
            public const string Default = "Views/User/UserView.cshtml";
        }
    }
}
