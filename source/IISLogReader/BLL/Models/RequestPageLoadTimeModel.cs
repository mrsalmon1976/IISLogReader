﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Models
{
    public class RequestPageLoadTimeModel
    {
        public string UriStemAggregate { get; set; }

        public int RequestCount { get; set; }

        public int AvgTimeTakenMilliseconds { get; set; }
    }
}
