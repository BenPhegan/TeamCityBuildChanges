using System;
using System.Linq;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TFS;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class TfsApiIntegrationTests
    {
        private const string ConnectionUri = "http://nw2tf-pras01:8080/tfs/essd";

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesChildrenIds()
        {
            var workItemId = 34736;

            var expectedChildren = new[] {34709, 34738, 34758, 34768, 34771, 34805};

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.NotNull(wi.ChildrenIds);
            Assert.AreEqual(expectedChildren, wi.ChildrenIds.ToArray());
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesCreated()
        {
            var workItemId = 31545;

            var expectedCreatedDate = new DateTime(2012, 10, 25, 19, 45, 43, 103);

            TfsWorkItem wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedCreatedDate, wi.Created);
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesDescription()
        {
            var workItemId = 34897;

            var expectedDescription = "As a <type of user> I want <some goal> so that <some reason>";

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedDescription, wi.Description);
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesHistoryComments()
        {
            var workItemId = 31545;

            var expectedHistoryComments = new[]
                {
                    "", "", "", "", "", "", "", "", "", "", "", "",
                    "Comment added through BA Portal : \nReview completed",
                    "",
                    "User Story State update triggered by DB Review completion",
                    "", "", "",
                    "force sync",
                    ""
                };

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.NotNull(wi.HistoryComments);
            Assert.AreEqual(expectedHistoryComments, wi.HistoryComments.ToArray());
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesId()
        {
            var workItemId = 31545;

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(workItemId, wi.Id);
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesParentId()
        {
            var workItemId = 26454;

            var expectedParentWorkItemId = 26453;

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedParentWorkItemId, wi.ParentId);
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesState()
        {
            var workItemId = 31545;

            var expectedState = "Closed";

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedState, wi.State);
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesTitle()
        {
            var workItemId = 31545;

            var expectedTitle = "R29.1 SDW Mobile PV compliance copy footer is displaying incorrectly for SDW scenarios";

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedTitle, wi.Title);
        }

        [Test]
        [Category("Integration")]
        public void GetWorkItem_RetrievesType()
        {
            var workItemId = 31545;

            var expectedType = "Bug";

            var wi = new TfsApi(ConnectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedType, wi.Type);
        }
    }
}