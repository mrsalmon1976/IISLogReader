using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.User
{
    public class UserListViewModel : BaseViewModel
    {
        public UserListViewModel()
            : base()
        {
            this.Users = new List<UserModel>();
        }

        public List<UserModel> Users { get; private set; }
        

    }
}
