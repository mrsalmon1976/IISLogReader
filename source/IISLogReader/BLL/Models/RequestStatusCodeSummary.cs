using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Models
{
    public class RequestStatusCodeSummary
    {
        public long InformationalCount { get; set; }

        public long SuccessCount { get; set; }

        public long RedirectionCount { get; set; }

        public long ClientErrorCount { get; set; }

        public long ServerErrorCount { get; set; }

        public long TotalCount { get; set; }
    }
}
