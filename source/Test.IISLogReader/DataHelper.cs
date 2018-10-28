using IISLogReader.BLL.Data.Models;
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

        public static LogFileModel CreateLogFileModel()
        {
            Random r = new Random();
            LogFileModel model = new LogFileModel();
            model.Id = r.Next(1, 1000);
            model.ProjectId = r.Next(1, 1000);
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

        public static RequestModel CreateRequestModel()
        {
            Random r = new Random();
            RequestModel model = new RequestModel();
            model.Id = r.Next(1, 1000);
            model.LogFileId = r.Next(1, 1000);
            model.RequestDateTime = DateTime.Now;
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

    }
}
