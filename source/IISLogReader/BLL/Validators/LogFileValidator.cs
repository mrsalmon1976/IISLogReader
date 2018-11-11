using IISLogReader.BLL.Models;
using IISLogReader.BLL.Data.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Validators
{
    public interface ILogFileValidator
    {
        ValidationResult Validate(LogFileModel model);
    }

    public class LogFileValidator : ILogFileValidator
    {
        public LogFileValidator()
        {
        }

        public ValidationResult Validate(LogFileModel model)
        {
            ValidationResult result = new ValidationResult();
            if (model.ProjectId <= 0)
            {
                result.Messages.Add("Project id must be a valid number");
            }
            if (String.IsNullOrWhiteSpace(model.FileName))
            {
                result.Messages.Add("File name cannot be empty");
            }
            if (String.IsNullOrWhiteSpace(model.FileHash))
            {
                result.Messages.Add("File hash cannot be empty");
            }
            if (model.FileLength <= 0)
            {
                result.Messages.Add("File length must be a valid number");
            }
            if (model.RecordCount <= 0)
            {
                result.Messages.Add("Record count must be a valid number");
            }

            return result;
        }
    }
}
