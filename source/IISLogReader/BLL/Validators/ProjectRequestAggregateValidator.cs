using IISLogReader.BLL.Models;
using IISLogReader.BLL.Data.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Validators
{
    public interface IProjectRequestAggregateValidator
    {
        ValidationResult Validate(ProjectRequestAggregateModel model);
    }

    public class ProjectRequestAggregateValidator : IProjectRequestAggregateValidator
    {
        public ProjectRequestAggregateValidator()
        {
        }

        public ValidationResult Validate(ProjectRequestAggregateModel model)
        {
            ValidationResult result = new ValidationResult();
            if (model.ProjectId <= 0)
            {
                result.Messages.Add("Project id must be a valid number");
            }
            if (String.IsNullOrWhiteSpace(model.AggregateTarget))
            {
                result.Messages.Add("Aggregate target cannot be empty");
            }
            if (String.IsNullOrWhiteSpace(model.RegularExpression))
            {
                result.Messages.Add("Regular expression cannot be empty");
            }

            return result;
        }
    }
}
