using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Services
{
    public interface IRequestAggregationService
    {
        string GetAggregatedUriStem(string originalUriStem, IEnumerable<ProjectRequestAggregateModel> requestAggregates);
    }

    public class RequestAggregationService : IRequestAggregationService
    {
        public string GetAggregatedUriStem(string originalUriStem, IEnumerable<ProjectRequestAggregateModel> requestAggregates)
        {
            if (requestAggregates == null || !requestAggregates.Any())
            {
                return originalUriStem;
            }

            foreach (ProjectRequestAggregateModel agg in requestAggregates)
            {
                if (Regex.IsMatch(originalUriStem, agg.RegularExpression))
                {
                    return agg.AggregateTarget;
                }
            }

            // no match found, return original
            return originalUriStem;
        }
    }
}
