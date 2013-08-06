using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax;
using TeamCityBuildChanges.ExternalApi.TFS;


namespace TeamCityBuildChanges.Tests.IssueDetailResolvers
{
    using NUnit.Framework;
    using TeamCityBuildChanges.ExternalApi.TeamCity;
    using TeamCityBuildChanges.IssueDetailResolvers;

    [TestFixture]
    public class TfsIssueResolverTests
    {
        public IEnumerable<TestCaseData> ChangeDetailsData
        {
            get
            {
                const int workItemId = 1;
                var tfsApiMock = A.Fake<ITfsApi>();
                tfsApiMock.Configure().CallsTo(x => x.GetWorkItemsByCommit(1234)).Returns(new List<TfsWorkItem> { new TfsWorkItem { Id = workItemId } });
                A.CallTo(() => tfsApiMock.ConnectionUri).Returns("http://localhost:8080/tfs/test");

                yield return new TestCaseData(new List<ChangeDetail> { new ChangeDetail { Version = "alphanumericId" }, new ChangeDetail { Version = "1234" } },
                    new TFSIssueResolver(tfsApiMock), workItemId);
            }
        }


        [Test]
        [TestCaseSource("ChangeDetailsData")]
        public void ChangeDetailIgnoredWhenVersionDoesNotResolveToInteger(IEnumerable<ChangeDetail> changeDetails, TFSIssueResolver resolver, int expectedWorkItemId)
        {
            //Act
            var issues =resolver.GetIssues(changeDetails);

            //Assert
            Assert.That(issues.Single().Id.Equals(expectedWorkItemId.ToString()));
        }
    }
}
