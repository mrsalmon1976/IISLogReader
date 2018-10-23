using IISLogReader.BLL.Data.Models;
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
    public class ProjectFormViewModel
    {
        public ProjectFormViewModel()
        {
        }

        public string Name { get; set; }

    }
}
