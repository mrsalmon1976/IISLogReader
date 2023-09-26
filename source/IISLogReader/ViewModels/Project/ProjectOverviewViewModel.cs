using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.Project
{
    public class ProjectOverviewViewModel
    {
        public int ProjectId { get; set; }

        public int LogFileCount { get; set; }

        public long TotalRequestCount { get; set; }

        // 2xx status codes
        public long SuccessRequestCount { get; set; }

        public long RedirectionRequestCount { get; set; }

        public long ClientErrorRequestCount { get; set; }

        public long ServerErrorRequestCount { get; set; }
    }
}
