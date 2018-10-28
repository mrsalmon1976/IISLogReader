using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test.IISLogReader.TestAssets
{
    public class TestAsset
    {
        public static string LogFile {  get { return "Test.IISLogReader.TestAssets.LogFile.log"; } }

        public static Stream ReadTextStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
