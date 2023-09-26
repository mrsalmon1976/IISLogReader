using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Models
{
    public class RequestStatusCodeCount
    {
        public int StatusCode { get; set;}

        public long TotalCount { get; set; }
    }
}
