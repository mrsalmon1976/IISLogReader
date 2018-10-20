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
    public interface IDbContext : IDisposable
    {

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        void ExecuteNonQuery(string sql, params IDbDataParameter[] dbParameters);

        /// <summary>
        /// Initialises the application database.  Creates a new file and all tables if they do not exist.
        /// </summary>
        void Initialise();
    }

    public abstract class DbContext : IDbContext
    {

        public virtual void Dispose()
        {

        }

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public abstract void ExecuteNonQuery(string sql, params IDbDataParameter[] dbParameters);

        /// <summary>
        /// Initialises the application database.  Creates a new file and all tables if they do not exist.
        /// </summary>
        public abstract void Initialise();

        protected string ReadResource(string qualifiedName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string result = null;

            using (Stream stream = assembly.GetManifestResourceStream(qualifiedName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
        }
    }
 
}
