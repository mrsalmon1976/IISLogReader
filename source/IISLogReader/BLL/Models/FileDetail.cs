using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Models
{
    public class FileDetail
    {
        public long Length { get; set; }

        public string Name { get; set; }

        public string Hash { get; set;  }
    }
}
