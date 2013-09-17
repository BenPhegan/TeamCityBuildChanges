using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public void PurelyNumericIssueNumbersAreNotQueried()
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

        [Test]
        public void CaseDoesNotMatterForCorrectJiraIssues()
        {
            var mockApi = A.Fake<IJiraApi>();
            A.CallTo(() => mockApi.GetJiraIssue(A<string>.That.IsEqualTo("jira-1"))).Returns(GetJiraRootObject("JIRA-1"));
            A.CallTo(() => mockApi.GetJiraIssue(A<string>.That.IsEqualTo("JIRA-2"))).Returns(GetJiraRootObject("JIRA-2"));
            A.CallTo(() => mockApi.GetJiraIssue(A<string>.That.IsEqualTo("JirA-3"))).Returns(GetJiraRootObject("JIRA-3"));

            var changes = new List<Issue>
                {
                    new Issue {Id = "1", Url = "http://qwerty.io/1"},
                    new Issue {Id = "jira-1", Url = "http://qwerty.io/2"},
                    new Issue {Id = "JIRA-2", Url = "http://qwerty.io/2"},
                    new Issue {Id = "JirA-3", Url = "http://qwerty.io/2"},
                };

            var resolver = new JiraIssueResolver(mockApi);
            var results = resolver.GetDetails(changes);
            Assert.AreEqual(3, results.Count());
            A.CallTo(() => mockApi.GetJiraIssue(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(3));
        }

        [Test]
        public void OnlyDetectsActualJiraIssueIds()
        {
            var mockApi = A.Fake<IJiraApi>();

            var resolver = new JiraIssueResolver(mockApi);
            var issues = resolver.GetIssues(new List<ChangeDetail>
                {
                    new ChangeDetail { Comment = "This should not trigger-"},
                    new ChangeDetail { Comment = "Neither should -this"},
                    new ChangeDetail { Comment = "-"},
                    new ChangeDetail { Comment = "454-fdfd"}
                });

            Assert.AreEqual(0, issues.Count());
        }


        private RootObject GetJiraRootObject(string jiraId)
        {
            return new RootObject
                {
                    Id = jiraId,
                    Key = jiraId,
                    Fields = new Fields
                        {
                            Status = new Status2
                                {
                                    Name = "Resolved"
                                },
                            Created = DateTime.Today.ToString(CultureInfo.InvariantCulture),
                            Summary = "Summary",
                            Description = "Description",
                            Issuetype = new Issuetype
                                {
                                    Name = "Bug"
                                }

                        }
                };
        }
    }
}
