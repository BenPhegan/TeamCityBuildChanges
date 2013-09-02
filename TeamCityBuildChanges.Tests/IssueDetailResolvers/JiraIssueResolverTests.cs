using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.Jira;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Tests.IssueDetailResolvers
{
    [TestFixture]
    public class JiraIssueResolverTests
    {
        [Test]
        public void Test()
        {
            var mockApi = A.Fake<IJiraApi>();
            var changes = new List<Issue>
                {
                    new Issue {Id = "1", Url = "http://qwerty.io/1"},
                    new Issue {Id = "2", Url = "http://qwerty.io/2"},
                };

            var resolver = new JiraIssueResolver(mockApi);
            var results = resolver.GetDetails(changes);
            Assert.AreEqual(0, results.Count());
            A.CallTo(() => mockApi.GetJiraIssue(A<string>.Ignored)).MustNotHaveHappened();

        }
    }
}
