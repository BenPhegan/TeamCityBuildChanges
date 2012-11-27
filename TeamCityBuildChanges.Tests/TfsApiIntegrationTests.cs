using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TFS;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class TfsApiIntegrationTests
    {
        [Test]
        public void GetWorkItem_RetrievesId()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 31545;

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(workItemId, wi.Id);
        }

        [Test]
        public void GetWorkItem_RetrievesCreated()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 31545;

            DateTime expectedCreatedDate = new DateTime(2012, 10, 25, 19, 45, 43, 103);

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedCreatedDate, wi.Created);
        }

        [Test]
        public void GetWorkItem_RetrievesState()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 31545;

            string expectedState = "Closed";

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedState, wi.State);
        }

        [Test]
        public void GetWorkItem_RetrievesType()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 31545;

            string expectedType = "Bug";

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedType, wi.Type);
        }

        [Test]
        public void GetWorkItem_RetrievesTitle()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 31545;

            string expectedTitle = "R29.1 SDW Mobile PV compliance copy footer is displaying incorrectly for SDW scenarios";

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedTitle, wi.Title);
        }

        [Test]
        public void GetWorkItem_RetrievesDescription()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 34897;

            string expectedDescription = "As a <type of user> I want <some goal> so that <some reason>";

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedDescription, wi.Description);
        }
        
        [Test]
        public void GetWorkItem_RetrievesParentId()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 26454;

            int expectedParentWorkItemId = 26453;

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.AreEqual(expectedParentWorkItemId, wi.ParentId);
        }

        [Test]
        public void GetWorkItem_RetrievesChildrenIds()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 34736;

            int[] expectedChildren = new int[] { 34709, 34738, 34758, 34768, 34771, 34805 };

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.NotNull(wi.ChildrenIds);
            Assert.AreEqual(expectedChildren, wi.ChildrenIds.ToArray());
        }

        [Test]
        public void GetWorkItem_RetrievesHistoryComments()
        {
            string connectionUri = "http://nw2tf-pras01:8080/tfs/essd";
            int workItemId = 31545;

            string[] expectedHistoryComments = new string[] 
            { "", "", "", "", "", "", "", "", "", "", "", "", 
              "Comment added through BA Portal : \nReview completed",
              "", 
              "User Story State update triggered by DB Review completion", 
              "", "", "", 
              "force sync", 
              ""
            };

            TfsWorkItem wi = new TfsApi(connectionUri).GetWorkItem(workItemId);

            Assert.NotNull(wi.HistoryComments);
            Assert.AreEqual(expectedHistoryComments, wi.HistoryComments.ToArray());
        }
    }
}
