using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Lookup
{
    public enum LogFileStatus
    {
        None = 0,
        Error = 1,
        Processing = 2,
        Complete = 3
    }
}
