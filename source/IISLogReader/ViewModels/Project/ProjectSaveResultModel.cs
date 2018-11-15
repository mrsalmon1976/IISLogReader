using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels.Project
{
    public class ProjectSaveResultModel
    {
        public ProjectSaveResultModel()
        {

        }

        public ProjectSaveResultModel(int projectId, bool success, string[] messages)
        {
            this.ProjectId = projectId;
            this.Success = success;
            this.Messages = messages;
        }

        public int ProjectId { get; set; }

        public bool Success { get; set; }

        public string[] Messages { get; set; }
    }
}
