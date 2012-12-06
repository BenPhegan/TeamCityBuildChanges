using System;
using System.Collections.Generic;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.Tests
{
    public class TestHelpers
    {
        public static ChangeManifest CreateChangeManifest()
        {
            return new ChangeManifest
                {
                    Generated = DateTime.Now,
                    FromVersion = "1.0",
                    ToVersion = "1.1",
                    BuildConfiguration = new BuildTypeDetails
                        {
                            Description = "Test",
                            Name = "Test"
                        },
                    ReferenceBuildConfiguration = new BuildTypeDetails
                        {
                        Description = "Test",
                        Name = "Test"
                    },
                    ChangeDetails = new List<ChangeDetail>
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
                    IssueDetails = new List<ExternalIssueDetails>
                        {
                            new ExternalIssueDetails
                                {
                                    Id = "TEST-1",
                                    Status = "Open",
                                    SubIssues = new List<ExternalIssueDetails>
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
                                    Status = "Resolved",
                                    SubIssues = new List<ExternalIssueDetails>
                                        {
                                            new ExternalIssueDetails
                                                {
                                                    Id = "TEST-5",
                                                    Status = "Resolved",
                                                }
                                        }
                                },
                            new ExternalIssueDetails
                                {
                                    Id = "TEST-1",
                                    Status = "Open",
                                    SubIssues = new List<ExternalIssueDetails>
                                        {
                                            new ExternalIssueDetails
                                                {
                                                    Id = "TEST-4",
                                                    Status = "Resolved",
                                                }
                                        }
                                }
                        }
                };
        }
    }
}