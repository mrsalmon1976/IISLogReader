using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.Project
{
    public class RequestUriAggregateViewModel
    {
        private readonly List<RequestModel> _requests = new List<RequestModel>();

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string UriStemAggregate { get; set; }
    }
}
