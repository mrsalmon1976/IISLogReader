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

        public static ProjectModel CreateProjectModel()
        {
            ProjectModel model = new ProjectModel();
            model.Name = Path.GetRandomFileName();
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
