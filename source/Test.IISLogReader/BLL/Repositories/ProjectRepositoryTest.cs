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

namespace Test.IISLogReader.BLL.Repositories
{
    [TestFixture]
    public class ProjectRepositoryTest
    {
        private IDbContext _dbContext;
        private IProjectRepository _projectRepo;


        [SetUp]
        public void ProjectRepositoryTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();

            _projectRepo = new ProjectRepository(_dbContext);
        }

        [TearDown]
        public void ProjectRepositoryTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        /// <summary>
        /// Tests that the GetAll method loads data in an ordered format
        /// </summary>
        [Test]
        public void GetAll_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                IProjectRepository projectRepo = new ProjectRepository(dbContext);

                ProjectModel projectA = DataHelper.CreateProjectModel();
                projectA.Name = "AAA";
                ProjectModel projectB = DataHelper.CreateProjectModel();
                projectB.Name = "BBB";
                ProjectModel projectC = DataHelper.CreateProjectModel();
                projectC.Name = "CCC";

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                createProjectCommand.Execute(projectA);
                createProjectCommand.Execute(projectC);
                createProjectCommand.Execute(projectB);

                List<ProjectModel> projects = projectRepo.GetAll().ToList();

                Assert.AreEqual(3, projects.Count);
                Assert.AreEqual("AAA", projects[0].Name);
                Assert.AreEqual("BBB", projects[1].Name);
                Assert.AreEqual("CCC", projects[2].Name);

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

                IProjectRepository projectRepo = new ProjectRepository(dbContext);

                ProjectModel project = DataHelper.CreateProjectModel();

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(dbContext, new ProjectValidator());
                project = createProjectCommand.Execute(project);

                ProjectModel result = projectRepo.GetById(project.Id);
                Assert.IsNotNull(result);
                Assert.AreEqual(project.Name, result.Name);

                result = projectRepo.GetById(project.Id + 1);
                Assert.IsNull(result);
            }

        }

    }
}
