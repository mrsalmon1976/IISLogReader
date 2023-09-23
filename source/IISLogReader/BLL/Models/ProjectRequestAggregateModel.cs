using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Models
{
    public class ProjectRequestAggregateModel
    {
        public ProjectRequestAggregateModel()
        {
            this.CreateDate = DateTime.Now;
        }

        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string RegularExpression { get; set; }

        public string AggregateTarget { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsIgnored {  get; set; }

    }
}
