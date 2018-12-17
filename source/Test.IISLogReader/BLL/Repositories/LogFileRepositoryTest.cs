using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Services;

namespace Test.IISLogReader.BLL.Repositories
{
    [TestFixture]
    public class LogFileRepositoryTest
    {
        private IDbContext _dbContext;



        [SetUp]
        public void LogFileRepositoryTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
        }

        [TearDown]
        public void LogFileRepositoryTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }


        /// <summary>
        /// Tests that the GetByHash method loads a file by the correct Id
        /// </summary>
        [Test]
        public void GetByHash_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            string fileHash = Guid.NewGuid().ToString();

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                ILogFileRepository logFileRepo = new LogFileRepository(dbContext);

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project);

                // create the log file record
                LogFileModel logFile = DataHelper.CreateLogFileModel();
                logFile.ProjectId = project.Id;
                logFile.FileHash = fileHash;
                DataHelper.InsertLogFileModel(dbContext, logFile);

                LogFileModel result = logFileRepo.GetByHash(project.Id, fileHash);
                Assert.IsNotNull(result);
                Assert.AreEqual(logFile.FileName, result.FileName);

                result = logFileRepo.GetByHash(0, fileHash);
                Assert.IsNull(result);
            }

        }

        /// <summary>
        /// Tests that the GetById method loads a file by the correct Id
        /// </summary>
        [Test]
        public void GetById_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                ILogFileRepository logFileRepo = new LogFileRepository(dbContext);

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project);

                // create the log file record
                LogFileModel logFile = DataHelper.CreateLogFileModel(project.Id);
                DataHelper.InsertLogFileModel(dbContext, logFile);

                int logFileId = dbContext.ExecuteScalar<int>("select last_insert_rowid()");

                LogFileModel result = logFileRepo.GetById(logFileId);
                Assert.IsNotNull(result);
                Assert.AreEqual(logFile.FileHash, result.FileHash);
            }

        }


        /// <summary>
        /// Tests that the GetByProject method all files for a specific project
        /// </summary>
        [Test]
        public void GetByProject_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                ILogFileRepository logFileRepo = new LogFileRepository(dbContext);

                // create the projects
                ProjectModel projectA = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, projectA);
                ProjectModel projectZ = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, projectZ);

                // create the log file records for ProjectA, as well as some for a different project
                int numRecords = new Random().Next(5, 10);
                for (var i = 0; i < numRecords; i++)
                {
                    LogFileModel logFile = DataHelper.CreateLogFileModel();
                    logFile.ProjectId = projectA.Id;
                    logFile.FileName = "ProjectA_" + i.ToString();
                    DataHelper.InsertLogFileModel(dbContext, logFile);
                }

                // create the log file records for ProjectB that should be excluded by the query
                for (var i = 0; i < 5; i++)
                {
                    LogFileModel logFile = DataHelper.CreateLogFileModel();
                    logFile.ProjectId = projectZ.Id;
                    logFile.FileName = "ProjectZ_" + i.ToString();
                    DataHelper.InsertLogFileModel(dbContext, logFile);
                }


                IEnumerable<LogFileModel> result = logFileRepo.GetByProject(projectA.Id);
                Assert.IsNotNull(result);
                Assert.AreEqual(numRecords, result.Count());
                Assert.AreEqual(numRecords, result.Select(x => x.FileName.StartsWith("ProjectA_")).Count());
            }

        }

    }
}
