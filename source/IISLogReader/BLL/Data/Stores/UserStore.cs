using Newtonsoft.Json;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;

namespace IISLogReader.BLL.Data.Stores
{
    public interface IUserStore
    {
        /// <summary>
        /// Gets/sets the file path of the file store.
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Gets the users of the application.
        /// </summary>
        List<UserModel> Users { get; }

        /// <summary>
        /// Gets a user by user name.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        UserModel GetUser(string userName);

        /// <summary>
        /// Loads the users for the app from the file on disk.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves the user store to disk.
        /// </summary>
        void Save();
        
    }

    public class UserStore : IUserStore
    {
        private IFileWrap _fileWrap;
        private IDirectoryWrap _dirWrap;
        private IPasswordProvider _passwordProvider;

        public UserStore(string filePath, IFileWrap fileWrap, IDirectoryWrap dirWrap, IPasswordProvider passwordProvider)
        {
            this.FilePath = filePath;
            _fileWrap = fileWrap;
            _dirWrap = dirWrap;
            _passwordProvider = passwordProvider;
            this.Users = new List<UserModel>();
        }

        [IgnoreDataMember]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets the users of the application.
        /// </summary>
        public List<UserModel> Users { get; private set; }

        /// <summary>
        /// Gets a user by user name.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public UserModel GetUser(string userName)
        {
            return this.Users.Where(x => x.UserName == userName).FirstOrDefault();
        }


        /// <summary>
        /// Loads the users for the app from the file on disk.
        /// </summary>
        public void Load()
        {
            if (_fileWrap.Exists(this.FilePath))
            {
                string text = _fileWrap.ReadAllText(this.FilePath);
                UserStore store = JsonConvert.DeserializeObject<UserStore>(text);
                this.Users.AddRange(store.Users);
            }
            else
            {
                // first time - create a default admin user
                UserModel user = new UserModel();
                user.Id = Guid.NewGuid();
                user.UserName = "admin";
                user.Password = _passwordProvider.HashPassword("admin", _passwordProvider.GenerateSalt());
                user.Role = Roles.Admin;
                Users.Add(user);
            }
        }

        /// <summary>
        /// Saves the user store to disk.
        /// </summary>
        public void Save()
        {
            string contents = JsonConvert.SerializeObject(this, Formatting.Indented);
            _dirWrap.CreateDirectory(Path.GetDirectoryName(this.FilePath));
            _fileWrap.WriteAllText(this.FilePath, contents);
        }
    }
}
