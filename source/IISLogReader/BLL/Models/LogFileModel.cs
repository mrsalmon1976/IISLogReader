﻿using IISLogReader.BLL.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Models
{
    public class LogFileModel
    {
        public LogFileModel()
        {
            this.CreateDate = DateTime.Now;
        }

        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string FileName { get; set; }

        public string FileHash { get; set; }

        public long FileLength { get; set; }

        public int RecordCount { get; set; }

        public DateTime CreateDate { get; set; }

        public LogFileStatus Status { get; set; }

        public string ErrorMsg { get; set; }


    }
}
