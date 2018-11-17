using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.Project
{
    /// <summary>
    /// Viewmodel used for the creation of projects.
    /// </summary>
    public class ProjectAggregateViewModel
    {
        public ProjectAggregateViewModel()
        {
        }

        public int ProjectId { get; set; }

        public string AggregateTarget { get; set; }

        public string RegularExpression { get; set; }

    }
}
