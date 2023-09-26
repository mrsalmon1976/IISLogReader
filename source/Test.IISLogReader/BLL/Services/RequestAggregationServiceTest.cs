using IISLogReader.BLL.Models;
using IISLogReader.BLL.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.IISLogReader.BLL.Services
{
    [TestFixture]
    public class RequestAggregationServiceTest
    {
        private IRequestAggregationService _requestAggregationService;

        [SetUp]
        public void RequestAggregationServiceTest_SetUp()
        {
            _requestAggregationService = new RequestAggregationService();
        }

        #region GetAggregatedUriStem Tests

        [Test]
        public void GetAggregatedUriStem_SuppliedAggregatesNull_ReturnsOriginal()
        {
            string uriStem = Path.GetRandomFileName();

            string result = _requestAggregationService.GetAggregatedUriStem(uriStem, null);

            Assert.AreEqual(uriStem, result);
        }

        [Test]
        public void GetAggregatedUriStem_SuppliedAggregatesEmpty_ReturnsOriginal()
        {
            string uriStem = Path.GetRandomFileName();

            string result = _requestAggregationService.GetAggregatedUriStem(uriStem, Enumerable.Empty<ProjectRequestAggregateModel>());

            Assert.AreEqual(uriStem, result);
        }

        [TestCase("/products/batch/a90035aa-c9ec-4a15-ae72-8c4dcd7d755a", "testest")]
        [TestCase("/products/123/test", "testest")]
        public void GetAggregatedUriStem_NoMatchFound_ReturnsOriginal(string originalUriStem, string regEx)
        {
            ProjectRequestAggregateModel aggregateModel = DataHelper.CreateProjectRequestAggregateModel();
            aggregateModel.RegularExpression = regEx;

            ProjectRequestAggregateModel[] requestAggregates = { aggregateModel };

            string result = _requestAggregationService.GetAggregatedUriStem(originalUriStem, requestAggregates);

            Assert.AreEqual(originalUriStem, result);
        }

        [TestCase("/products/batch/A90035AA-C9EC-4A15-AE72-8C4DCD7D755A", "^/products/batch/[{(]?[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?$")]
        [TestCase("/products/123/test", "^/products/[0-9]+/test$")]
        public void GetAggregatedUriStem_MatchFound_ReturnsAggregateTarget(string originalUriStem, string regEx)
        {
            ProjectRequestAggregateModel aggregateModel = DataHelper.CreateProjectRequestAggregateModel();
            aggregateModel.RegularExpression = regEx;

            ProjectRequestAggregateModel[] requestAggregates = { aggregateModel };

            string result = _requestAggregationService.GetAggregatedUriStem(originalUriStem, requestAggregates);

            Assert.AreEqual(aggregateModel.AggregateTarget, result);
        }

        [Test]
        public void GetAggregatedUriStem_IsIgnored_ReturnsNull()
        {
            ProjectRequestAggregateModel aggregateModel = DataHelper.CreateProjectRequestAggregateModel();
            aggregateModel.RegularExpression = "test";
            aggregateModel.IsIgnored = true;

            ProjectRequestAggregateModel[] requestAggregates = { aggregateModel };

            string result = _requestAggregationService.GetAggregatedUriStem("test", requestAggregates);

            Assert.That(result, Is.Null);
        }

        #endregion

        #region GetRequestStatusCodeSummary

        [Test]
        public void GetRequestStatusCodeSummary_MultipleRequests_GroupsStatusCodes()
        {
            Random r = new Random();
            List<RequestStatusCodeCount> requests100 = CreateRequestStatusCodeSummaries(r.Next(10, 100), 100, 199);
            List<RequestStatusCodeCount> requests200 = CreateRequestStatusCodeSummaries(r.Next(10, 100), 200, 249);
            List<RequestStatusCodeCount> requests250 = CreateRequestStatusCodeSummaries(r.Next(10, 100), 250, 299);
            List<RequestStatusCodeCount> requests300 = CreateRequestStatusCodeSummaries(r.Next(10, 100), 300, 399);
            List<RequestStatusCodeCount> requests400 = CreateRequestStatusCodeSummaries(r.Next(10, 100), 400, 499);
            List<RequestStatusCodeCount> requests500 = CreateRequestStatusCodeSummaries(r.Next(10, 100), 500, 599);

            List<RequestStatusCodeCount> allRequests = new List<RequestStatusCodeCount>();
            allRequests.AddRange(requests100);
            allRequests.AddRange(requests200);
            allRequests.AddRange(requests250);      // just to make sure we have a spread!
            allRequests.AddRange(requests300);
            allRequests.AddRange(requests400);
            allRequests.AddRange(requests500);

            // execute
            RequestStatusCodeSummary result = _requestAggregationService.GetRequestStatusCodeSummary(allRequests);

            // assert
            Assert.That(result.InformationalCount, Is.EqualTo(requests100.Sum(x => x.TotalCount)));
            Assert.That(result.SuccessCount, Is.EqualTo(requests200.Sum(x => x.TotalCount) + requests250.Sum(x => x.TotalCount)));
            Assert.That(result.RedirectionCount, Is.EqualTo(requests300.Sum(x => x.TotalCount)));
            Assert.That(result.ClientErrorCount, Is.EqualTo(requests400.Sum(x => x.TotalCount)));
            Assert.That(result.ServerErrorCount, Is.EqualTo(requests500.Sum(x => x.TotalCount)));

        }

        #endregion

        #region Private Methods

        private List<RequestStatusCodeCount> CreateRequestStatusCodeSummaries(int requestCount, int minStatusCode, int maxStatusCode)
        {
            Random r = new Random();
            List<RequestStatusCodeCount> requests = new List<RequestStatusCodeCount>();
            for (int i = 0; i < requestCount; i++)
            {
                requests.Add(new RequestStatusCodeCount()
                {
                    StatusCode = r.Next(minStatusCode, maxStatusCode),
                    TotalCount = r.Next(10, 1000)
                });
            }
            return requests;
        }

        #endregion
    }
}
