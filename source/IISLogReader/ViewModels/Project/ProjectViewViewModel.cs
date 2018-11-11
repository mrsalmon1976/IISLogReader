using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.Project
{
    /// <summary>
    /// Viewmodel used when a project is viewed via the ProjectModule (/project/{projectId}.
    /// </summary>
    public class ProjectViewViewModel
    {
        public ProjectViewViewModel()
        {
        }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        /// <summary>
        /// Gets the number of log files that still need to be processed.
        /// </summary>
        public int UnprocessedCount { get; set; }
    }
}
