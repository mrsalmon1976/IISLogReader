using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Models
{
    public class ProjectModel
    {
        public ProjectModel()
        {
            this.CreateDate = DateTime.Now;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
