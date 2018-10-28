using NSubstitute;
using NUnit.Framework;
using IISLogReader.BLL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IISLogReader.BLL.Commands.Project;
using IISLogReader.BLL.Data;
using System.IO;
using IISLogReader.BLL.Data.Repositories;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class CreateLogFileWithRequestsCommandTest
    {
        private ICreateLogFileWithRequestsCommand _createLogFileWithRequestsCommand;

        private IDbContext _dbContext;
        private ILogFileRepository _logFileRepo;

        [SetUp]
        public void AddProjectFileCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();
            _logFileRepo = Substitute.For<ILogFileRepository>();

            _createLogFileWithRequestsCommand = new CreateLogFileWithRequestsCommand(_dbContext, _logFileRepo);
        }

        [TearDown]
        public void AddProjectFileCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.log");
        }

        [Test]
        public void Execute_InvalidFileFormat_ThrowsFileFormatException()
        {
            int projectId = new Random().Next(1, 100);
            string fileName = Path.GetRandomFileName() + ".log";
            string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            File.WriteAllText(filePath, "This is not a valid IIS file");

            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                // execute
                TestDelegate del = () => _createLogFileWithRequestsCommand.Execute(projectId, fileName, stream);

                // assert
                Assert.Throws<FileFormatException>(del);

                // we shouldn't have even tried to load the project by hash
                _logFileRepo.DidNotReceive().GetByHash(Arg.Any<int>(), Arg.Any<string>());
            }
        }

        /// <summary>
        /// Tests that the insert actually works
        /// </summary>
        [Test]
        public void Execute_IntegrationTest_SQLite()
        {
            //string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            //using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            //{
            //    dbContext.Initialise();

            //    ProjectModel project = DataHelper.CreateProjectModel();

            //    IProjectValidator projectValidator = new ProjectValidator();
            //    ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, projectValidator);
            //    ProjectModel savedProject = createProjectCommand.Execute(project);

            //    Assert.Greater(savedProject.Id, 0);

            //    int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Projects");
            //    Assert.Greater(rowCount, 0);

            //    string projectName = dbContext.ExecuteScalar<string>("SELECT Name FROM projects WHERE Id = @Id", savedProject);
            //    Assert.AreEqual(savedProject.Name, projectName);

            //}

        }



    }
}
