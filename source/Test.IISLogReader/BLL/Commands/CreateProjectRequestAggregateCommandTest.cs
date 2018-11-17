using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Models;
using IISLogReader.BLL.Data.Stores;
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
    public class CreateProjectRequestAggregateCommandTest
    {
        private ICreateProjectRequestAggregateCommand _createProjectRequestAggregateCommand;

        private IDbContext _dbContext;
        private IProjectRequestAggregateValidator _projectRequestAggregateValidator;
        private ILogFileRepository _logFileRepo;
        private ISetLogFileUnprocessedCommand _setLogFileUnprocessedCommand;

        [SetUp]
        public void CreateProjectRequestAggregateCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _projectRequestAggregateValidator = Substitute.For<IProjectRequestAggregateValidator>();
            _logFileRepo = Substitute.For<ILogFileRepository>();
            _setLogFileUnprocessedCommand = Substitute.For<ISetLogFileUnprocessedCommand>();

            _createProjectRequestAggregateCommand = new CreateProjectRequestAggregateCommand(_dbContext, _projectRequestAggregateValidator, _logFileRepo, _setLogFileUnprocessedCommand);
        }

        [TearDown]
        public void CreateProjectRequestAggregateCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_ValidationFails_ThrowsException()
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();

            _projectRequestAggregateValidator.Validate(model).Returns(new ValidationResult("error"));

            // execute
            TestDelegate del = () => _createProjectRequestAggregateCommand.Execute(model);
            
            // assert
            Assert.Throws<ValidationException>(del);

            // we shouldn't have even tried to do the insert
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_RecordInserted()
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();

            _projectRequestAggregateValidator.Validate(model).Returns(new ValidationResult());

            // execute
            _createProjectRequestAggregateCommand.Execute(model);

            // assert
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_JobsRegisteredForLogFiles()
        {
            ProjectRequestAggregateModel model = DataHelper.CreateProjectRequestAggregateModel();

            _projectRequestAggregateValidator.Validate(model).Returns(new ValidationResult());

            LogFileModel log1 = DataHelper.CreateLogFileModel(model.ProjectId);
            log1.Id = 1;
            LogFileModel log2 = DataHelper.CreateLogFileModel(model.ProjectId);
            log2.Id = 2;
            LogFileModel log3 = DataHelper.CreateLogFileModel(model.ProjectId);
            log3.Id = 3;

            _logFileRepo.GetByProject(model.ProjectId).Returns(new LogFileModel[] { log1, log2, log3 });

            // execute
            _createProjectRequestAggregateCommand.Execute(model);

            // assert
            _logFileRepo.Received(1).GetByProject(model.ProjectId);
            _setLogFileUnprocessedCommand.Received(1).Execute(log1.Id);
            _setLogFileUnprocessedCommand.Received(1).Execute(log2.Id);
            _setLogFileUnprocessedCommand.Received(1).Execute(log3.Id);
        }

        /// <summary>
        /// Tests that the insert actually works
        /// </summary>
        [Test]
        public void Execute_IntegrationTest_SQLite()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                IProjectValidator projectValidator = new ProjectValidator();
                IProjectRequestAggregateValidator validator = new ProjectRequestAggregateValidator();
                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, projectValidator);
                ISetLogFileUnprocessedCommand setLogFileUnprocessedCommand = Substitute.For<ISetLogFileUnprocessedCommand>();
                ICreateProjectRequestAggregateCommand createProjectRequestAggregateCommand = new CreateProjectRequestAggregateCommand(dbContext, validator, new LogFileRepository(dbContext), setLogFileUnprocessedCommand);

                // create the project first so we have one
                ProjectModel project = DataHelper.CreateProjectModel();
                createProjectCommand.Execute(project);

                // create the request aggregate
                ProjectRequestAggregateModel projectRequestAggregate = DataHelper.CreateProjectRequestAggregateModel();
                projectRequestAggregate.ProjectId = project.Id;
                createProjectRequestAggregateCommand.Execute(projectRequestAggregate);

                Assert.Greater(projectRequestAggregate.Id, 0);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM ProjectRequestAggregates");
                Assert.Greater(rowCount, 0);

                ProjectRequestAggregateModel savedModel = dbContext.Query<ProjectRequestAggregateModel>("SELECT * FROM ProjectRequestAggregates WHERE Id = @Id", new { Id = projectRequestAggregate.Id }).Single();
                Assert.AreEqual(projectRequestAggregate.RegularExpression, savedModel.RegularExpression);
                Assert.AreEqual(projectRequestAggregate.AggregateTarget, savedModel.AggregateTarget);

            }

        }



    }
}
