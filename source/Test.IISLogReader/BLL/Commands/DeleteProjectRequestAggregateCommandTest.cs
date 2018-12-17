using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IISLogReader.BLL.Commands;
using IISLogReader.BLL.Data;
using IISLogReader.BLL.Exceptions;
using System.IO;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Services;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class DeleteProjectRequestAggregateCommandTest
    {
        private IDeleteProjectRequestAggregateCommand _deleteProjectRequestAggregateCommand;

        private IDbContext _dbContext;
        private IProjectRequestAggregateRepository _projectRequestAggregateRepo;
        private ILogFileRepository _logFileRepo;
        private ISetLogFileUnprocessedCommand _setLogFileUnprocessedCommand;

        [SetUp]
        public void DeleteProjectRequestAggregateCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _projectRequestAggregateRepo = Substitute.For<IProjectRequestAggregateRepository>();
            _logFileRepo = Substitute.For<ILogFileRepository>();
            _setLogFileUnprocessedCommand = Substitute.For<ISetLogFileUnprocessedCommand>();

            _deleteProjectRequestAggregateCommand = new DeleteProjectRequestAggregateCommand(_dbContext, _projectRequestAggregateRepo, _logFileRepo, _setLogFileUnprocessedCommand);
        }

        [TearDown]
        public void DeleteProjectRequestAggregateCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_RecordDoesNotExist_ThrowsException()
        {
            int id = new Random().Next(1, 1000);
            ProjectRequestAggregateModel model = null;

            _projectRequestAggregateRepo.GetById(id).Returns(model);

            // execute
            TestDelegate del = () => _deleteProjectRequestAggregateCommand.Execute(id);
            
            // assert
            Assert.Throws<InvalidOperationException>(del);

            // we shouldn't have even tried to do the delete
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_RecordInserted()
        {
            int id = new Random().Next(1, 1000);
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();

            _projectRequestAggregateRepo.GetById(id).Returns(model);

            // execute
            _deleteProjectRequestAggregateCommand.Execute(id);

            // assert
            _projectRequestAggregateRepo.Received(1).GetById(id);
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_JobsRegisteredForLogFiles()
        {
            int id = new Random().Next(1, 1000);
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();
            _projectRequestAggregateRepo.GetById(id).Returns(model);

            LogFileModel log1 = DataHelper.CreateLogFileModel(model.ProjectId);
            log1.Id = 1;
            LogFileModel log2 = DataHelper.CreateLogFileModel(model.ProjectId);
            log2.Id = 2;
            LogFileModel log3 = DataHelper.CreateLogFileModel(model.ProjectId);
            log3.Id = 3;

            _logFileRepo.GetByProject(model.ProjectId).Returns(new LogFileModel[] { log1, log2, log3 });

            // execute
            _deleteProjectRequestAggregateCommand.Execute(id);

            // assert
            _logFileRepo.Received(1).GetByProject(model.ProjectId);
            _setLogFileUnprocessedCommand.Received(1).Execute(log1.Id);
            _setLogFileUnprocessedCommand.Received(1).Execute(log2.Id);
            _setLogFileUnprocessedCommand.Received(1).Execute(log3.Id);
        }

        /// <summary>
        /// Tests that the delete actually works
        /// </summary>
        [Test]
        public void Execute_IntegrationTest_SQLite()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();
                dbContext.BeginTransaction();

                IProjectRequestAggregateRepository projectRequestAggregateRepo = new ProjectRequestAggregateRepository(dbContext);

                ISetLogFileUnprocessedCommand setLogFileUnprocessedCommand = Substitute.For<ISetLogFileUnprocessedCommand>();
                ICreateProjectRequestAggregateCommand createProjectRequestAggregateCommand = new CreateProjectRequestAggregateCommand(dbContext, new ProjectRequestAggregateValidator(), new LogFileRepository(dbContext), setLogFileUnprocessedCommand);
                IDeleteProjectRequestAggregateCommand deleteProjectRequestAggregateCommand = new DeleteProjectRequestAggregateCommand(dbContext, projectRequestAggregateRepo, new LogFileRepository(dbContext), setLogFileUnprocessedCommand);

                // create the project first so we have one
                ProjectModel project = DataHelper.CreateProjectModel();
                DataHelper.InsertProjectModel(dbContext, project);

                // create the request aggregate
                ProjectRequestAggregateModel projectRequestAggregate = DataHelper.CreateProjectRequestAggregateModel();
                projectRequestAggregate.ProjectId = project.Id;
                createProjectRequestAggregateCommand.Execute(projectRequestAggregate);
                int id = projectRequestAggregate.Id;
                Assert.Greater(id, 0);

                // fetch the record to make sure it exists
                ProjectRequestAggregateModel record = projectRequestAggregateRepo.GetById(id);
                Assert.IsNotNull(record);

                // delete the request aggregate
                deleteProjectRequestAggregateCommand.Execute(id);

                record = projectRequestAggregateRepo.GetById(id);
                Assert.IsNull(record);


            }

        }



    }
}
