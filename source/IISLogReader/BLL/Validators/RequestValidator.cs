using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Validators
{
    public interface IRequestValidator
    {
        ValidationResult Validate(RequestModel model);
    }

    public class RequestValidator : IRequestValidator
    {
        public RequestValidator()
        {
        }

        public ValidationResult Validate(RequestModel model)
        {
            ValidationResult result = new ValidationResult();
            if (model.LogFileId <= 0)
            {
                result.Messages.Add("Log file id must be a valid number");
            }
            return result;
        }
    }
}
