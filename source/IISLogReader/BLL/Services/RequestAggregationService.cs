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

        RequestStatusCodeSummary GetRequestStatusCodeSummary(IEnumerable<RequestStatusCodeCount> requestStatusCodeCounts);
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
                    if (agg.IsIgnored)
                    {
                        return null;
                    }

                    return agg.AggregateTarget;
                }
            }

            // no match found, return original
            return originalUriStem;
        }

        public RequestStatusCodeSummary GetRequestStatusCodeSummary(IEnumerable<RequestStatusCodeCount> requestStatusCodeCounts)
        {
            RequestStatusCodeSummary summary = new RequestStatusCodeSummary();
            summary.InformationalCount = GroupStatusCodes(requestStatusCodeCounts, 100);
            summary.SuccessCount = GroupStatusCodes(requestStatusCodeCounts, 200);
            summary.RedirectionCount = GroupStatusCodes(requestStatusCodeCounts, 300);
            summary.ClientErrorCount = GroupStatusCodes(requestStatusCodeCounts, 400);
            summary.ServerErrorCount = GroupStatusCodes(requestStatusCodeCounts, 500);
            return summary;
        }

        private long GroupStatusCodes(IEnumerable<RequestStatusCodeCount> requestStatusCodeCounts, int statusCode)
        {
            return requestStatusCodeCounts
                .Where(x => x.StatusCode >= statusCode && x.StatusCode <= (statusCode + 99))
                .Sum(x => x.TotalCount);
        }

    }
}
