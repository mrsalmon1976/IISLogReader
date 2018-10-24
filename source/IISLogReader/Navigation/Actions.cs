using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.Navigation
{
    public class Actions
    {
        public class Dashboard
        {
            public const string Default = "/dashboard";
        }


        public class Login
        {
            public const string Default = "/login";
            public const string Logout = "/logout";
        }

        public class Project
        {
            public const string Save = "/project/save"; 

            public static string View()
            {
                return "/project/{projectId}";
            }

            public static string View(string projectId)
            {
                return Actions.Project.View().Replace("{projectId}", projectId);
            }

            public static string View(int projectId)
            {
                return Actions.Project.View().Replace("{projectId}", projectId.ToString());
            }

        }

        public class User
        {
            public const string Default = "/user";

            public const string ChangePassword = "/user/changepassword";

            public const string List = "/user/list";

            public const string Save = "/user/save";
        }
    }
}
