using IISLogReader.BLL.Data;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.IISLogReader
{
    public class DataHelper
    {

        public static LogFileModel CreateLogFileModel(int projectId = 0)
        {
            Random r = new Random();
            LogFileModel model = new LogFileModel();
            model.Id = r.Next(1, 1000);
            model.ProjectId = (projectId <=0 ? r.Next(1, 1000) : projectId);
            model.FileName = Path.GetRandomFileName();
            model.FileHash = Path.GetRandomFileName();
            model.FileLength = r.Next(1, 1000);
            model.RecordCount = r.Next(1, 1000);
            return model;
        }

        public static ProjectModel CreateProjectModel()
        {
            Random r = new Random();
            ProjectModel model = new ProjectModel();
            model.Id = r.Next(1, 1000);
            model.Name = Path.GetRandomFileName();
            return model;
        }

        public static ProjectRequestAggregateModel CreateProjectRequestAggregateModel()
        {
            Random r = new Random();
            ProjectRequestAggregateModel model = new ProjectRequestAggregateModel();
            model.Id = r.Next(1, 1000);
            model.ProjectId = r.Next(1, 1000);
            model.AggregateTarget = Path.GetRandomFileName();
            model.RegularExpression = Path.GetRandomFileName();
            return model;
        }

        public static RequestModel CreateRequestModel(int logFileId = 0)
        {
            Random r = new Random();
            RequestModel model = new RequestModel();
            model.Id = r.Next(1, 1000);
            model.LogFileId = (logFileId == 0 ? r.Next(1, 1000) : logFileId);
            model.RequestDateTime = DateTime.Now;
            return model;
        }

        public static RequestPageLoadTimeModel CreateRequestPageLoadTimeModel()
        {
            Random r = new Random();
            RequestPageLoadTimeModel model = new RequestPageLoadTimeModel();
            model.AvgTimeTakenMilliseconds = r.Next(1, 5000);
            model.RequestCount = r.Next(1, 1000000);
            model.UriStemAggregate = Path.GetRandomFileName() + "/" + Path.GetRandomFileName();
            return model;
        }

        public static UserModel CreateUserModel()
        {
            UserModel model = new UserModel();
            model.UserName = "TestUser";
            model.Password = "password";
            model.Role = Roles.User;
            return model;
        }

        public static void InsertLogFileModel(IDbContext dbContext, LogFileModel logFile)
        {
            const string sql = @"INSERT INTO LogFiles (ProjectId, FileName, FileHash, CreateDate, FileLength, RecordCount, Status) VALUES (@ProjectId, @FileName, @FileHash, @CreateDate, @FileLength, @RecordCount, @Status)";
            dbContext.ExecuteNonQuery(sql, logFile);
            logFile.Id = dbContext.ExecuteScalar<int>("select last_insert_rowid()");
        }

        public static void InsertProjectModel(IDbContext dbContext, ProjectModel project)
        {
            const string sql = @"INSERT INTO Projects (Name, CreateDate) VALUES (@Name, @CreateDate)";
            dbContext.ExecuteNonQuery(sql, project);
            project.Id = dbContext.ExecuteScalar<int>("select last_insert_rowid()");
        }

    }
}
