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
using System.Data;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class CreateProjectCommandTest
    {
        private ICreateProjectCommand _createProjectCommand;

        private IDbContext _dbContext;
        private IProjectValidator _projectValidator;

        [SetUp]
        public void CreateProjectCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _projectValidator = Substitute.For<IProjectValidator>();

            _createProjectCommand = new CreateProjectCommand(_dbContext, _projectValidator);
        }

        [TearDown]
        public void CreateProjectCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        [Test]
        public void Execute_ValidationFails_ThrowsException()
        {
            ProjectModel model = DataHelper.CreateProjectModel();

            _projectValidator.Validate(model).Returns(new ValidationResult("error"));

            // execute
            TestDelegate del = () => _createProjectCommand.Execute(model);
            
            // assert
            Assert.Throws<ValidationException>(del);

            // we shouldn't have even tried to do the insert
            _dbContext.DidNotReceive().ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Execute_ValidationSucceeds_RecordInserted()
        {
            ProjectModel model = DataHelper.CreateProjectModel();

            _projectValidator.Validate(model).Returns(new ValidationResult());

            // execute
            _createProjectCommand.Execute(model);

            // assert
            _dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<object>());
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
                dbContext.BeginTransaction();

                ProjectModel project = DataHelper.CreateProjectModel();

                IProjectValidator projectValidator = new ProjectValidator();
                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, projectValidator);
                ProjectModel savedProject = createProjectCommand.Execute(project);

                Assert.Greater(savedProject.Id, 0);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Projects");
                Assert.Greater(rowCount, 0);

                string projectName = dbContext.ExecuteScalar<string>("SELECT Name FROM projects WHERE Id = @Id", savedProject);
                Assert.AreEqual(savedProject.Name, projectName);

            }

        }



    }
}
