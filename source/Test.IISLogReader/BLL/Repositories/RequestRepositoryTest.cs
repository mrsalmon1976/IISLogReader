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
                dbContext.BeginTransaction();

                IRequestRepository requestRepo = new RequestRepository(dbContext);

                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project);

                // create multiple log file records 
                for (var i = 0; i < 3; i++)
                {
                    LogFileModel logFile = DataHelper.CreateLogFileModel(project.Id);
                    DataHelper.InsertLogFileModel(dbContext, logFile);

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
                dbContext.BeginTransaction();

                IRequestRepository requestRepo = new RequestRepository(dbContext);

                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project);

                ProjectModel project2 = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project2);

                // create log file and request records for each
                LogFileModel logFile = DataHelper.CreateLogFileModel(project.Id);
                DataHelper.InsertLogFileModel(dbContext, logFile);
                createRequestBatchCommand.Execute(logFile.Id, logEvents);

                LogFileModel logFile2 = DataHelper.CreateLogFileModel(project2.Id);
                DataHelper.InsertLogFileModel(dbContext, logFile2);
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
                dbContext.BeginTransaction();

                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                IRequestRepository requestRepo = new RequestRepository(dbContext);

                // create the project
                ProjectModel project = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project);

                // create the log file record
                LogFileModel logFile = DataHelper.CreateLogFileModel(project.Id);
                DataHelper.InsertLogFileModel(dbContext, logFile);

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


        /// <summary>
        /// Tests that the GetPageLoadTimes loads correct averages
        /// </summary>
        [Test]
        public void GetTotalRequestCountAsync_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");

            ProjectModel firstProject = DataHelper.CreateProjectModel(1);
            long firstProjectRequestCount = new Random().Next(101, 1000);
            List<W3CEvent> firstProjectEvents = CreateW3CEvents(firstProjectRequestCount);

            ProjectModel secondProject = DataHelper.CreateProjectModel(5);
            long secondProjectRequestCount = new Random().Next(10, 100);
            List<W3CEvent> secondProjectEvents = CreateW3CEvents(secondProjectRequestCount);

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();
                dbContext.BeginTransaction();

                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                IRequestRepository requestRepo = new RequestRepository(dbContext);

                // create the projects
                DataHelper.InsertProjectModel(dbContext, firstProject);
                DataHelper.InsertProjectModel(dbContext, secondProject);

                // create the log file records
                LogFileModel firstLogFile = DataHelper.CreateLogFileModel(firstProject.Id);
                DataHelper.InsertLogFileModel(dbContext, firstLogFile);
                LogFileModel secondLogFile = DataHelper.CreateLogFileModel(secondProject.Id);
                DataHelper.InsertLogFileModel(dbContext, secondLogFile);

                // create the requests
                createRequestBatchCommand.Execute(firstLogFile.Id, firstProjectEvents);
                createRequestBatchCommand.Execute(secondLogFile.Id, secondProjectEvents);

                long firstResult = requestRepo.GetTotalRequestCountAsync(firstProject.Id).Result;
                long secondResult = requestRepo.GetTotalRequestCountAsync(secondProject.Id).Result;

                Assert.That(firstResult, Is.EqualTo(firstProjectRequestCount));
                Assert.That(secondResult, Is.EqualTo(secondProjectRequestCount));

            }

        }

        /// <summary>
        /// Tests that the GetPageLoadTimes loads correct averages
        /// </summary>
        [Test]
        public void GetStatusCodeSummaryAsync_Integration_ReturnsData()
        {
            Random r = new Random();
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");

            ProjectModel firstProject = DataHelper.CreateProjectModel(1);
            long requests200 = r.Next(10, 200);
            long requests300 = r.Next(10, 200);
            long requests400 = r.Next(10, 200);
            long requests500 = r.Next(10, 200);
            List<W3CEvent> firstProjectEvents = CreateW3CEvents(requests200, 200);
            firstProjectEvents.AddRange(CreateW3CEvents(requests300, 300));
            firstProjectEvents.AddRange(CreateW3CEvents(requests400, 400));
            firstProjectEvents.AddRange(CreateW3CEvents(requests500, 500));

            ProjectModel secondProject = DataHelper.CreateProjectModel(5);
            long secondProjectRequestCount = r.Next(10, 100);
            List<W3CEvent> secondProjectEvents = CreateW3CEvents(secondProjectRequestCount);

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();
                dbContext.BeginTransaction();

                ICreateRequestBatchCommand createRequestBatchCommand = new CreateRequestBatchCommand(dbContext, new RequestValidator());
                IRequestRepository requestRepo = new RequestRepository(dbContext);

                // create the projects
                DataHelper.InsertProjectModel(dbContext, firstProject);
                DataHelper.InsertProjectModel(dbContext, secondProject);

                // create the log file records
                LogFileModel firstLogFile = DataHelper.CreateLogFileModel(firstProject.Id);
                DataHelper.InsertLogFileModel(dbContext, firstLogFile);
                LogFileModel secondLogFile = DataHelper.CreateLogFileModel(secondProject.Id);
                DataHelper.InsertLogFileModel(dbContext, secondLogFile);

                // create the requests
                createRequestBatchCommand.Execute(firstLogFile.Id, firstProjectEvents);
                createRequestBatchCommand.Execute(secondLogFile.Id, secondProjectEvents);

                IEnumerable<RequestStatusCodeCount> result = requestRepo.GetStatusCodeSummaryAsync(firstProject.Id).Result;

                Assert.That(result.Count(), Is.EqualTo(4));

                var expectedSummary = result.SingleOrDefault(x => x.StatusCode == 200);
                Assert.That(expectedSummary.TotalCount, Is.EqualTo(requests200));

                expectedSummary = result.SingleOrDefault(x => x.StatusCode == 300);
                Assert.That(expectedSummary.TotalCount, Is.EqualTo(requests300));

                expectedSummary = result.SingleOrDefault(x => x.StatusCode == 400);
                Assert.That(expectedSummary.TotalCount, Is.EqualTo(requests400));

                expectedSummary = result.SingleOrDefault(x => x.StatusCode == 500);
                Assert.That(expectedSummary.TotalCount, Is.EqualTo(requests500));

            }

        }


        private W3CEvent CreateW3CEvent(string uriStem, int timeTaken, int statusCode = 200)
        {
            W3CEvent evt = new W3CEvent();
            evt.cs_uri_stem = uriStem;
            evt.time_taken = timeTaken.ToString();
            evt.sc_status = statusCode.ToString();
            return evt;
        }

        private List<W3CEvent> CreateW3CEvents(long count, int statusCode = 200)
        {
            List<W3CEvent> events = new List<W3CEvent>();
            Random r = new Random();
            for (int i=0; i < count; i++)
            {
                events.Add(CreateW3CEvent(Guid.NewGuid().ToString(), r.Next(1, 10000), statusCode));
            }
            return events;
        }

    }
}
