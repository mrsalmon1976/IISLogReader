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
    }
}
