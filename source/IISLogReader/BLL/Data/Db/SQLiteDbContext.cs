using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Data
{
    public class SQLiteDbContext : DbContext
    {
        private readonly string _dbPath;
        private readonly string _connString;
        private SQLiteConnection _conn;

        public SQLiteDbContext(string filePath)
        {
            _dbPath = filePath;
            _connString = String.Format("Data Source={0};Version=3;", filePath);
            _conn = new SQLiteConnection(_connString);
            _conn.Open();
        }

        public override void Dispose()
        {
            if (_conn != null)
            {
                _conn.Dispose();
            }
        }

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public override void ExecuteNonQuery(string sql, params IDbDataParameter[] dbParameters)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sql, _conn))
            {
                cmd.Parameters.AddRange(dbParameters);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Initialises the application database.  Creates a new file and all tables if they do not exist.
        /// </summary>
        public override void Initialise()
        {
            if (!System.IO.File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            string sql = this.ReadResource("IISLogReader.BLL.Data.Scripts.SQLite.sql");
            using (SQLiteCommand cmd = new SQLiteCommand(sql, _conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
