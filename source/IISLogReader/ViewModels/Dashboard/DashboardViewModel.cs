using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.Dashboard
{
    public class DashboardViewModel : BaseViewModel
    {
        public DashboardViewModel()
        {
        }

        public bool IsProjectEditor { get; set; }

    }
}
