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
        private IProjectRepository _projectRepo;


        [SetUp]
        public void CreateProjectCommandTest_SetUp()
        {
            _projectRepo = new ProjectRepository();
        }

        [TearDown]
        public void CreateProjectCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }

        /// <summary>
        /// Tests that the insert actually works
        /// </summary>
        [Test]
        public void GetAll_Integration_ReturnsData()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName() + ".dbtest");
            using (SQLiteDbContext dbContext = new SQLiteDbContext(filePath))
            {
                dbContext.Initialise();

                ProjectModel projectA = DataHelper.CreateProjectModel();
                projectA.Name = "AAA";
                ProjectModel projectB = DataHelper.CreateProjectModel();
                projectB.Name = "BBB";
                ProjectModel projectC = DataHelper.CreateProjectModel();
                projectC.Name = "CCC";

                ICreateProjectCommand createProjectCommand = new CreateProjectCommand(new ProjectValidator());
                createProjectCommand.Execute(dbContext, projectA);
                createProjectCommand.Execute(dbContext, projectC);
                createProjectCommand.Execute(dbContext, projectB);

                List<ProjectModel> projects = _projectRepo.GetAll(dbContext).ToList();

                Assert.AreEqual(3, projects.Count);
                Assert.AreEqual("AAA", projects[0].Name);
                Assert.AreEqual("BBB", projects[1].Name);
                Assert.AreEqual("CCC", projects[2].Name);

            }

        }



    }
}
