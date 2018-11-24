using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Security
{
    public static class Claims
    {
        static Claims()
        {
            AllClaims = new List<string>(new string[] 
            { 
                ProjectEdit,
                UserAdd,
                UserDelete,
                UserList
            }).AsReadOnly();
        }

        public const string UserAdd = "UserAdd";

        public const string UserDelete = "UserDelete";

        public const string UserList = "UserList";

        public const string ProjectEdit = "ProjectEdit";

        public static IReadOnlyList<string> AllClaims { get; private set; }
    }
}
