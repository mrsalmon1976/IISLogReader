using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Utils
{
    public class NumericUtils
    {
        public static int? TryParse(string number)
        {
            int result;
            if (Int32.TryParse(number, out result))
            {
                return result;
            }
            return null;
        }
    }
}
