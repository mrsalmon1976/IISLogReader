using IISLogReader.BLL.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ByteSizeLib;

namespace IISLogReader.ViewModels.LogFile
{
    public class LogFileViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }

        public long FileLength { get; set; }

        public string FileSize
        {
            get
            {
                return ByteSize.FromBytes(FileLength).ToString();
            }
        }

        public int RecordCount { get; set; }

        public LogFileStatus Status { get; set; }

        public string StatusName
        {
            get
            {
                return this.Status.ToString();
            }
        }

        public string ErrorMsg { get; set; }

        /// <summary>
        /// Gets/sets whether the log file has been processed.
        /// </summary>
        public bool IsProcessed
        {
            get
            {
                return (this.Status == LogFileStatus.Complete);
            }
        }

    }
}
