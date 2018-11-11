using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.User
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            this.Id = Guid.NewGuid();
            this.ValidationErrors = new List<string>();
            this.Roles = new List<string>();
        }

        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public List<string> ValidationErrors { get; private set; }

        public List<string> Roles { get; private set; }

        public string SelectedRole { get; set; }

    }
}
