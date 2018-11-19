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

namespace Test.IISLogReader.BLL.Repositories
{
    [TestFixture]
    public class ProjectRequestAggregateRepositoryTest
    {
        private IDbContext _dbContext;


        [SetUp]
        public void ProjectRequestAggregateRepositoryTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
        }

        [TearDown]
        public void ProjectRequestAggregateRepositoryTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        /// <summary>
        /// Tests that the GetById method returns a single record
        /// </summary>
        [Test]
        public void GetById_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                IProjectRequestAggregateRepository projectRequestAggregateRepo = new ProjectRequestAggregateRepository(dbContext);

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ISetLogFileUnprocessedCommand setLogFileUnprocessedCommand = Substitute.For<ISetLogFileUnprocessedCommand>();
                ICreateProjectRequestAggregateCommand createProjectRequestAggregateCommand = new CreateProjectRequestAggregateCommand(dbContext, new ProjectRequestAggregateValidator(), new LogFileRepository(dbContext), setLogFileUnprocessedCommand);
                IDeleteProjectRequestAggregateCommand deleteProjectRequestAggregateCommand = new DeleteProjectRequestAggregateCommand(dbContext, projectRequestAggregateRepo, new LogFileRepository(dbContext), setLogFileUnprocessedCommand);

                // create the project
                ProjectModel projectA = DataHelper.CreateProjectModel();
                createProjectCommand.Execute(projectA);

                // create the request aggregate record for ProjectA
                ProjectRequestAggregateModel projectRequestAggregate = DataHelper.CreateProjectRequestAggregateModel();
                projectRequestAggregate.ProjectId = projectA.Id;
                createProjectRequestAggregateCommand.Execute(projectRequestAggregate);
                int id = projectRequestAggregate.Id;
                Assert.Greater(id, 0);

                // fetch the record
                ProjectRequestAggregateModel result = projectRequestAggregateRepo.GetById(id);
                Assert.IsNotNull(result);
                Assert.AreEqual(projectRequestAggregate.Id, id);
                Assert.AreEqual(projectRequestAggregate.RegularExpression, result.RegularExpression);
                Assert.AreEqual(projectRequestAggregate.AggregateTarget, result.AggregateTarget);
            }

        }


        /// <summary>
        /// Tests that the GetByProject method all request aggregates for a specific project
        /// </summary>
        [Test]
        public void GetByProject_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");

            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                IProjectRequestAggregateRepository projectRequestAggregateRepo = new ProjectRequestAggregateRepository(dbContext);

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                ISetLogFileUnprocessedCommand setLogFileUnprocessedCommand = Substitute.For<ISetLogFileUnprocessedCommand>();
                ICreateProjectRequestAggregateCommand projectRequestAggregateCommand = new CreateProjectRequestAggregateCommand(dbContext, new ProjectRequestAggregateValidator(), new LogFileRepository(dbContext), setLogFileUnprocessedCommand);

                // create the projects
                ProjectModel projectA = DataHelper.CreateProjectModel();
                projectA = createProjectCommand.Execute(projectA);
                ProjectModel projectZ = DataHelper.CreateProjectModel();
                projectZ = createProjectCommand.Execute(projectZ);

                // create the request aggregate records for ProjectA
                int numRecords = new Random().Next(5, 10);
                for (var i = 0; i < numRecords; i++)
                {
                    ProjectRequestAggregateModel projectRequestAggregate = DataHelper.CreateProjectRequestAggregateModel();
                    projectRequestAggregate.ProjectId = projectA.Id;
                    projectRequestAggregateCommand.Execute(projectRequestAggregate);
                }

                // create the log file records for ProjectB that should be excluded by the query
                for (var i = 0; i < 5; i++)
                {
                    ProjectRequestAggregateModel projectRequestAggregate = DataHelper.CreateProjectRequestAggregateModel();
                    projectRequestAggregate.ProjectId = projectZ.Id;
                    projectRequestAggregateCommand.Execute(projectRequestAggregate);
                }


                IEnumerable<ProjectRequestAggregateModel> result = projectRequestAggregateRepo.GetByProject(projectA.Id);
                Assert.IsNotNull(result);
                Assert.AreEqual(numRecords, result.Count());
            }

        }

    }
}
