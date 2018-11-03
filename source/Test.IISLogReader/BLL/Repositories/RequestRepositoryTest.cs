using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Data.Models;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IISLogReader.BLL.Data.Repositories;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Commands.Project;
using Test.IISLogReader.TestAssets;
using Tx.Windows;

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

                IEnumerable<RequestPageLoadTimeModel> result = requestRepo.GetPageLoadTimes(project.Id);
                Assert.AreEqual(2, result.Count());

                RequestPageLoadTimeModel pageAResult = result.Where(x => x.UriStem == "PageA").SingleOrDefault();
                Assert.IsNotNull(pageAResult);
                Assert.AreEqual(5, pageAResult.RequestCount);
                Assert.AreEqual(20, pageAResult.AvgTimeTakenMilliseconds);

                RequestPageLoadTimeModel pageBResult = result.Where(x => x.UriStem == "PageB").SingleOrDefault();
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
