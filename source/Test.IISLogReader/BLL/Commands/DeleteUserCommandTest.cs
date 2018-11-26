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
using Tx.Windows;
using Test.IISLogReader.TestAssets;
using IISLogReader.BLL.Services;
using IISLogReader.BLL.Repositories;
using IISLogReader.BLL.Security;

namespace Test.IISLogReader.BLL.Commands
{
    [TestFixture]
    public class DeleteUserCommandTest
    {
        private IDeleteUserCommand _deleteUserCommand;

        private IDbContext _dbContext;

        [SetUp]
        public void DeleteUserCommandTest_SetUp()
        {
            _dbContext = Substitute.For<IDbContext>();

            _deleteUserCommand = new DeleteUserCommand(_dbContext);
        }

        [TearDown]
        public void DeleteUserCommandTest_TearDown()
        {
            // delete all .db files (in case previous tests have failed)
            TestHelper.DeleteTestFiles(AppContext.BaseDirectory, "*.dbtest");

        }


        [Test]
        public void Execute_ValidationSucceeds_StatementsExecuted()
        {
            // execute
            _deleteUserCommand.Execute(Guid.NewGuid());

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

                IUserRepository userRepo = new UserRepository(dbContext);
                ICreateUserCommand createUserCommand = new CreateUserCommand(dbContext, new UserValidator(userRepo), new PasswordProvider());
                IDeleteUserCommand deleteUserCommand = new DeleteUserCommand(dbContext);


                // create the user 
                UserModel user = createUserCommand.Execute("test", Guid.NewGuid().ToString(), Roles.User);

                int rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");
                Assert.AreEqual(1, rowCount);

                //  run the delete command and check the end tables - should be 0 records
                deleteUserCommand.Execute(user.Id);

                rowCount = dbContext.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");
                Assert.AreEqual(0, rowCount);


            }

        }



    }
}
