using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels
{
    public class SaveResultModel
    {
        public SaveResultModel()
        {

        }

        public SaveResultModel(string id, bool success, string[] messages)
        {
            this.Id = id;
            this.Success = success;
            this.Messages = messages;
        }

        public string Id { get; set; }

        public bool Success { get; set; }

        public string[] Messages { get; set; }
    }
}
