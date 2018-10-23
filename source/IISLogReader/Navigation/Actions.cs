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
            public const string View = "/project/{projectId}";
            public const string Save = "/project/save";
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
