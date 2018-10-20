using IISLogReader.BLL.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.IISLogReader.BLL.Data.Db
{
    [TestFixture]
    public class SQLiteDbContextTest
    {
        private SQLiteDbContext _dbContext;
        private string _filePath;

        [SetUp]
        public void SQLiteDbContextTest_SetUp()
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, "_test_" + Path.GetRandomFileName() + ".db");
            _dbContext = new SQLiteDbContext(_filePath);
        }

        [TearDown]
        public void SQLiteDbContextTest_TearDown()
        {
            _dbContext.Dispose();

            // delete all .db files (in case previous tests have failed)
            string[] files = Directory.GetFiles(AppContext.BaseDirectory, "_test_*.db");
            foreach (string f in files)
            {
                File.Delete(f);
                Console.WriteLine("Deleted file {0}", f);
            }
        }

        #region Initalise Tests

        [Test]
        public void Initialise_CreatesDatabaseFile()
        {
            _dbContext.Initialise();
            Assert.IsTrue(File.Exists(_filePath));
        }

        [Test]
        public void Initialise_CreatesTable_Projects()
        {
            _dbContext.Initialise();
            const string sql = "SELECT * FROM projects";
            _dbContext.ExecuteNonQuery(sql);

            Assert.IsTrue(File.Exists(_filePath));
        }

        [Test]
        public void Initialise_CreatesTable_LogFiles()
        {
            _dbContext.Initialise();
            const string sql = "SELECT * FROM log_files";
            _dbContext.ExecuteNonQuery(sql);

            Assert.IsTrue(File.Exists(_filePath));
        }

        [Test]
        public void Initialise_CreatesTable_Requests()
        {
            _dbContext.Initialise();
            const string sql = "SELECT * FROM requests";
            _dbContext.ExecuteNonQuery(sql);

            Assert.IsTrue(File.Exists(_filePath));
        }

        #endregion
    }
}
