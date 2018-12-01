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
using Test.IISLogReader.TestAssets;
using Tx.Windows;
using IISLogReader.BLL.Services;

namespace Test.IISLogReader.BLL.Repositories
{
    [TestFixture]
    public class RequestRepositoryTest
    {
        private IDbContext _dbContext;


        [SetUp]
        public void RequestRepositoryTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
        }

        [TearDown]
        public void RequestRepositoryTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        /// <summary>
        /// Tests that the GetByLogFile method all files for a specific project
        /// </summary>
        [Test]
        public void GetByLogFile_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            List<W3CEvent> logEvents = null;
            int logFileId = 0;

            using (StreamReader logStream = new StreamReader(TestAsset.ReadTextStream(TestAsset.LogFile)))
            {
                logEvents = W3CEnumerable.FromStream(logStream).ToList().GetRange(0, 10);
            }


            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                IRequestRepository requestRepo = new RequestRepository(dbContext);

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, new LogFileValidator());
                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                project = createProjectCommand.Execute(project);

                // create multiple log file records 
                for (var i = 0; i < 3; i++)
                {
                    LogFileModel logFile = DataHelper.CreateLogFileModel();
                    logFile.ProjectId = project.Id;
                    createLogFileCommand.Execute(logFile);

                    createRequestBatchCommand.Execute(logFile.Id, logEvents);

                    if (logFileId == 0)
                    {
                        logFileId = logFile.Id;
                    }
                }


                IEnumerable<RequestModel> result = requestRepo.GetByLogFile(logFileId);
                Assert.IsNotNull(result);
                Assert.AreEqual(logEvents.Count, result.Count());
            }

        }

        /// <summary>
        /// Tests that the GetByUriStemAggregate method gets all requests for a specific project and uri stem aggregate
        /// </summary>
        [Test]
        public void GetByUriStemAggregate_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            List<W3CEvent> logEvents = null;
            int logFileId = 0;
            string uriStemAggregate = Guid.NewGuid().ToString();
            int expectedCount = new Random().Next(3, 7);

            using (StreamReader logStream = new StreamReader(TestAsset.ReadTextStream(TestAsset.LogFile)))
            {
                logEvents = W3CEnumerable.FromStream(logStream).ToList();
            }

            for (int i = 0; i < expectedCount; i++)
            {
                logEvents[i].cs_uri_stem = uriStemAggregate;
            }

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                IRequestRepository requestRepo = new RequestRepository(dbContext);

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, new LogFileValidator());
                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                createProjectCommand.Execute(project);

                ProjectModel project2 = DataHelper.CreateProjectModel();
                createProjectCommand.Execute(project2);

                // create log file and request records for each
                LogFileModel logFile = DataHelper.CreateLogFileModel();
                logFile.ProjectId = project.Id;
                createLogFileCommand.Execute(logFile);
                createRequestBatchCommand.Execute(logFile.Id, logEvents);

                LogFileModel logFile2 = DataHelper.CreateLogFileModel();
                logFile2.ProjectId = project2.Id;
                createLogFileCommand.Execute(logFile2);
                createRequestBatchCommand.Execute(logFile2.Id, logEvents);


                IEnumerable<RequestModel> result = requestRepo.GetByUriStemAggregate(project.Id, uriStemAggregate);
                Assert.IsNotNull(result);
                Assert.AreEqual(expectedCount, result.Count());
                foreach (RequestModel rm in result)
                {
                    Assert.AreEqual(logFile.Id, rm.LogFileId);
                }
            }

        }


        /// <summary>
        /// Tests that the GetPageLoadTimes loads correct averages
        /// </summary>
        [Test]
        public void GetPageLoadTimes_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");

            List<W3CEvent> logEvents = new List<W3CEvent>();
            logEvents.Add(CreateW3CEvent("PageA", 17));
            logEvents.Add(CreateW3CEvent("PageA", 13));
            logEvents.Add(CreateW3CEvent("PageA", 21));
            logEvents.Add(CreateW3CEvent("PageA", 9));
            logEvents.Add(CreateW3CEvent("PageA", 40));

            logEvents.Add(CreateW3CEvent("PageB", 1000));
            logEvents.Add(CreateW3CEvent("PageB", 3000));

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ICreateLogFileCommand createLogFileCommand = new CreateLogFileCommand(dbContext, new LogFileValidator());
                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                IRequestRepository requestRepo = new RequestRepository(dbContext);

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                project = createProjectCommand.Execute(project);

                // create the log file record
                LogFileModel logFile = DataHelper.CreateLogFileModel();
                logFile.ProjectId = project.Id;
                logFile = createLogFileCommand.Execute(logFile);

                // create the requests
                createRequestBatchCommand.Execute(logFile.Id, logEvents);

                // update all requests so the aggregate is different to the stem
                string sql = "UPDATE Requests SET UriStemAggregate = UriStem || '__'";
                dbContext.ExecuteNonQuery(sql);

                IEnumerable<RequestPageLoadTimeModel> result = requestRepo.GetPageLoadTimes(project.Id);
                Assert.AreEqual(2, result.Count());

                RequestPageLoadTimeModel pageAResult = result.Where(x => x.UriStemAggregate == "PageA__").SingleOrDefault();
                Assert.IsNotNull(pageAResult);
                Assert.AreEqual(5, pageAResult.RequestCount);
                Assert.AreEqual(20, pageAResult.AvgTimeTakenMilliseconds);

                RequestPageLoadTimeModel pageBResult = result.Where(x => x.UriStemAggregate == "PageB__").SingleOrDefault();
                Assert.IsNotNull(pageBResult);
                Assert.AreEqual(2, pageBResult.RequestCount);
                Assert.AreEqual(2000, pageBResult.AvgTimeTakenMilliseconds);

            }

        }



        private W3CEvent CreateW3CEvent(string uriStem, int timeTaken)
        {
            W3CEvent evt = new W3CEvent();
            evt.cs_uri_stem = uriStem;
            evt.time_taken = timeTaken.ToString();
            return evt;
        }
   
    }
}
