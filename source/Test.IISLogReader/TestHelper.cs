using IISLogReader.BLL.Security;
using IISLogReader.Navigation;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Test.IISLogReader
{
    public class TestHelper
    {

        public static void DeleteTestFiles(string directory, string filter)
        {
            string[] files = Directory.GetFiles(directory, filter);
            foreach (string f in files)
            {
                File.Delete(f);
                Console.WriteLine("Deleted file {0}", f);
            }

        }

        public static void ValidateAuth(UserIdentity currentUser, Browser browser, string url, HttpMethod httpMethod, string acceptedClaim)
        {
            BrowserResponse response = null;

            foreach (string claim in Claims.AllClaims)
            {

                currentUser.Claims = new string[] { claim };

                // execute
                if (httpMethod == HttpMethod.Get)
                {
                    response = browser.Get(url, (with) =>
                    {
                        with.HttpRequest();
                        with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                    });
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    response = browser.Post(url, (with) =>
                    {
                        with.HttpRequest();
                        with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                    });
                }
                else
                {
                    throw new NotImplementedException();
                }

                // assert
                if (claim == acceptedClaim)
                {
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                }
                else
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
                }
            }
        }
    }
}
