using IISLogReader.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Data.Db
{
    public interface IDbContextFactory
    {
        IDbContext GetDbContext();
    }

    public class DbContextFactory : IDbContextFactory
    {
        private string _databasePath;

        public DbContextFactory(IAppSettings appSettings)
        {
            _databasePath = Path.Combine(appSettings.DataDirectory, "IISLogReader.db");

        }

        public IDbContext GetDbContext()
        {
            return new SQLiteDbContext(_databasePath);
        }
    }
}
