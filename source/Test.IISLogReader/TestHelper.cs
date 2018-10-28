using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

       
    }
}
