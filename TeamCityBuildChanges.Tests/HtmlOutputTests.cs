using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Tests
{
    [TestFixture]
    public class HtmlOutputTests
    {
        [Test]
        public void Test()
        {
            var result = HtmlOutput.Render(CreateChangeManifest());
            Assert.True(true);//Giddyup.
        }

        private static ChangeManifest CreateChangeManifest()
        {
            return new ChangeManifest()
                {
                    Generated = DateTime.Now,
                    ChangeDetails = new List<ChangeDetail>()
                        {
                            new ChangeDetail
                                {
                                    Comment = "Test1",
                                    Id = "1"
                                },
                            new ChangeDetail
                                {
                                    Comment = "Test2",
                                    Id = "2"
                                }
                        },
                    IssueDetails = new List<ExternalIssueDetails>()
                        {
                            new ExternalIssueDetails
                                {
                                    Id = "TEST-1",
                                    Status = "Open",
                                    SubIssues = new List<ExternalIssueDetails>()
                                        {
                                            new ExternalIssueDetails
                                                {
                                                    Id = "TEST-3",
                                                    Status = "Resolved",
                                                }
                                        }
                                },
                            new ExternalIssueDetails
                                {
                                    Id = "Test-2",
                                    Status = "Resolved"
                                }
                        }
                };
        }
    }
}
