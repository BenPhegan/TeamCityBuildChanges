using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Tests.IssueDetailResolvers
{
    [TestFixture]
    public class IssueDetailResolverTests
    {
        [Test]
        public void IssueDetailResolverShouldPassDistinctListsOfIssuesToResolvers()
        {
            var mockResolver = A.Fake<IExternalIssueResolver>();
            var issues = new List<Issue>
                {
                    new Issue {Id = "1", Url = "http://qwerty.io/1"},
                    new Issue {Id = "1", Url = "http://qwerty.io/1"},
                    new Issue {Id = "2", Url = "http://qwerty.io/2"},
                    new Issue {Id = "2", Url = "http://qwerty.io/2"},
                };

            new IssueDetailResolver(new List<IExternalIssueResolver>{mockResolver}).GetExternalIssueDetails(issues);

            A.CallTo(() => mockResolver.GetDetails(A<IEnumerable<Issue>>.That.Matches(p => p.Count() == 2))).MustHaveHappened();

        }
    }
}
