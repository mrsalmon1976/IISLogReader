using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Data.Models
{
    public class RequestModel
    {
        public int Id { get; set; }

        public int LogFileId { get; set; }

        public DateTime RequestDateTime { get; set; }

    }
}
