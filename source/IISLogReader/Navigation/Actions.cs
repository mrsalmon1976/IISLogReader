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

            public static string Delete()
            {
                return "/logfile/delete/{logfileid}";
            }

            public static string Delete(string logFileId)
            {
                return Actions.LogFile.Delete().Replace("{logfileid}", logFileId);
            }

            public static string Delete(int logFileId)
            {
                return Actions.LogFile.Delete().Replace("{logfileid}", logFileId.ToString());
            }

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
            public static string Aggregates()
            {
                return "/project/{projectId}/aggregates";
            }

            public static string Aggregates(string projectId)
            {
                return Actions.Project.Aggregates().Replace("{projectId}", projectId);
            }

            public static string Aggregates(int projectId)
            {
                return Actions.Project.Aggregates(projectId.ToString());
            }

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
                return Actions.Project.AvgLoadTimes(projectId.ToString());
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
                return Actions.Project.Delete(projectId.ToString());
            }

            public static string Overview()
            {
                return "/project/{projectId}/overview";
            }

            public static string Overview(string projectId)
            {
                return Actions.Project.Overview().Replace("{projectId}", projectId);
            }

            public static string Overview(int projectId)
            {
                return Actions.Project.Overview(projectId.ToString());
            }


            public static string RequestsByAggregate()
            {
                return "/project/{projectId}/requests";
            }

            public static string RequestsByAggregate(string projectId)
            {
                return Actions.Project.RequestsByAggregate().Replace("{projectId}", projectId);
            }

            public static string RequestsByAggregate(int projectId)
            {
                return Actions.Project.RequestsByAggregate(projectId.ToString());
            }

            public static string RequestsByAggregateDetail()
            {
                return "/project/{projectId}/requests/detail";
            }

            public static string RequestsByAggregateDetail(string projectId)
            {
                return Actions.Project.RequestsByAggregateDetail().Replace("{projectId}", projectId);
            }

            public static string RequestsByAggregateDetail(int projectId)
            {
                return Actions.Project.RequestsByAggregateDetail(projectId.ToString());
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
                return Actions.Project.View(projectId.ToString());
            }

            public static string ErrorsByAggregate()
            {
                return "/project/{projectId}/errors";
            }

            public static string ErrorsByAggregate(string projectId)
            {
                return Actions.Project.ErrorsByAggregate().Replace("{projectId}", projectId);
            }

            public static string ErrorsByAggregate(int projectId)
            {
                return Actions.Project.ErrorsByAggregate(projectId.ToString());
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
                return Actions.Project.Files(projectId.ToString());
            }

        }

        public class ProjectRequestAggregate
        {
            public static string Delete()
            {
                return "/project/requestaggregate/delete";
            }

            public static string Save()
            {
                return "/project/requestaggregate/save";
            }
        }

        public class User
        {
            public const string Default = "/user";

            public const string Delete = "/user/delete";

            public const string ChangePassword = "/user/changepassword";

            public const string List = "/user/list";

            public const string Save = "/user/save";
        }
    }
}
