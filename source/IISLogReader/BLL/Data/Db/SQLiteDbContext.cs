using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;

namespace IISLogReader.BLL.Data
{
    public class SQLiteDbContext : DbContext
    {
        private readonly string _dbPath;
        private readonly string _connString;
        private SQLiteConnection _conn;

        public SQLiteDbContext(string filePath)
        {
            this.Id = Guid.NewGuid(); 
            _dbPath = filePath;
            _connString = String.Format("Data Source={0};Version=3;", filePath);
            _conn = new SQLiteConnection(_connString);
            _conn.Open();
        }


        /// <summary>
        /// Gets/sets the unique identifier of the DbContext - useful for debugging but serves no other practical purpose
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Begins a new transaction (if supported by the DbContext)
        /// </summary>
        public override void BeginTransaction()
        {
            this.Transaction = _conn.BeginTransaction();
        }

        /// <summary>
        /// Commits the current transaction (if supported by the DbContext)
        /// </summary>
        public override void Commit()
        {
            if (this.Transaction == null)
            {
                throw new InvalidOperationException("Transaction has not been started");
            }
            this.Transaction.Commit();
        }

        public override void Dispose()
        {
            if (_conn != null)
            {
                _conn.Dispose();
            }
        }

        /// <summary>
        /// Executes a query and maps the result to a strongly typed list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public override IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return _conn.Query<T>(sql, param, this.Transaction);
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
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public override T ExecuteScalar<T>(string sql, object param = null)
        {
            return _conn.ExecuteScalar<T>(sql, param, this.Transaction);
        }

        /// <summary>
        /// Executes a query against the database using Dapper to substitute model values.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        public override void ExecuteNonQuery(string sql, object param)
        {
            _conn.Execute(sql, param, this.Transaction);
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

            string sqlFileContents = this.ReadResource("IISLogReader.BLL.Data.Scripts.SQLite.sql");
            IEnumerable<string> sqlStatements = Regex.Split(sqlFileContents, "--\\*\\*").Select(x => x.Trim()).Where(x => x.Length > 0);
            using (SQLiteCommand cmd = new SQLiteCommand(_conn))
            {
                foreach (string sql in sqlStatements)
                {
                    cmd.CommandText = sql;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        // SQLite doesn't allow for ALTER IF EXISTS, so we just ignore duplicate column names
                        if (ex.Message.Contains("duplicate column name"))
                        {
                            continue;
                        }
                        throw;
                    }
                }
            
            }
        }

        /// <summary>
        /// Rolls back the current transaction (if supported by the DbContext)
        /// </summary>
        public override void Rollback()
        {
            if (this.Transaction == null)
            {
                throw new InvalidOperationException("Transaction has not been started");
            }
            this.Transaction.Rollback();
        }

    }
}
