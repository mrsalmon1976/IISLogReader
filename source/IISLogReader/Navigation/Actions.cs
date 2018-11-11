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

        public class LogFile
        {
            public static string Save()
            {
                return "/project/{projectId}/logfile";
            }

            public static string Save(string projectId)
            {
                return Actions.LogFile.Save().Replace("{projectId}", projectId);
            }

            public static string Save(int projectId)
            {
                return Actions.LogFile.Save().Replace("{projectId}", projectId.ToString());
            }

        }

        public class Login
        {
            public const string Default = "/login";
            public const string Logout = "/logout";
        }

        public class Project
        {
            public static string AvgLoadTimes()
            {
                return "/project/{projectId}/avgloadtimes";
            }

            public static string AvgLoadTimes(string projectId)
            {
                return Actions.Project.AvgLoadTimes().Replace("{projectId}", projectId);
            }

            public static string AvgLoadTimes(int projectId)
            {
                return Actions.Project.AvgLoadTimes().Replace("{projectId}", projectId.ToString());
            }

            public static string Delete()
            {
                return "/project/delete/{projectId}";
            }

            public static string Delete(string projectId)
            {
                return Actions.Project.Delete().Replace("{projectId}", projectId);
            }

            public static string Delete(int projectId)
            {
                return Actions.Project.Delete().Replace("{projectId}", projectId.ToString());
            }

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

            public static string Files()
            {
                return "/project/{projectId}/files";
            }

            public static string Files(string projectId)
            {
                return Actions.Project.Files().Replace("{projectId}", projectId);
            }

            public static string Files(int projectId)
            {
                return Actions.Project.Files().Replace("{projectId}", projectId.ToString());
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
