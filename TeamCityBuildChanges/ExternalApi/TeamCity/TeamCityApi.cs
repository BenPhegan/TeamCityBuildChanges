using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using RestSharp;
using ServiceStack.CacheAccess;
using TeamCityBuildChanges.ExtensionMethods;

namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    public class TeamCityApi : ITeamCityApi
    {
        private readonly IAuthenticatedRestClient _client;
        private readonly ICacheClient _cache;

        public TeamCityApi(IAuthenticatedRestClient client, ICacheClient cache)
        {
            _client = client;
            _cache = cache;
        }

        public string Url
        {
            get { return _client.BaseUrl; }
        }

        public List<BuildType> GetBuildTypes()
        {
            return _cache.GetFromCacheOrFunc("AllBuildTypes", key =>
                {
                    var request = new RestRequest("app/rest/buildTypes", Method.GET) {RequestFormat = DataFormat.Xml};
                    request.AddHeader("Accept", "application/xml");

                    var buildConfigs = _client.Execute<List<BuildType>>(request);
                    return buildConfigs.Data;
                });
        }

        public BuildTypeDetails GetBuildTypeDetailsById(string id)
        {
            return _cache.GetFromCacheOrFunc(id, query =>
                {
                    var buildDetails = GetXmlBuildRequest("app/rest/buildTypes/id:{ID}", "ID", id);
                    var response = _client.Execute<BuildTypeDetails>(buildDetails);
                    return response.Data;
                });
        }

        private static RestRequest GetXmlBuildRequest(string endpoint, string variable = null, string replacement = null)
        {
            var request = new RestRequest(endpoint, Method.GET);
            if (variable != null && replacement != null)
                request.AddParameter(variable, replacement, ParameterType.UrlSegment);
            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("Accept", "application/xml");
            return request;
        }

        public IEnumerable<Artifact> GetArtifactListByBuildType(string buildType)
        {
            return _cache.GetFromCacheOrFunc(buildType, key =>
                {
                    var request = new RestRequest("repository/download/{ID}/lastSuccessful/teamcity-ivy.xml");
                    request.AddParameter("ID", buildType, ParameterType.UrlSegment);
                    request.RequestFormat = DataFormat.Xml;
                    request.AddHeader("Accept", "application/xml");

                    var response = _client.Execute<IvyModule>(request);
                    return response.Data != null ? response.Data.Publications : new List<Artifact>();
                });
        }

        public List<PackageDetails> GetNuGetDependenciesByBuildTypeAndBuildId(string buildType, string buildId)
        {
            return _cache.GetFromCacheOrFunc(buildType, buildId, (key1, key2) =>
                {
                    var restUrl = new StringBuilder();
                    restUrl.AppendFormat("{0}/repository/download/{1}/{2}:id/.teamcity/nuget/nuget.xml", _client.BaseUrl, buildType, buildId);

                    var restRequest = (HttpWebRequest) WebRequest.Create(restUrl.ToString());

                    if (!string.IsNullOrEmpty(_client.AuthenticationToken))
                    {
                        restRequest.Headers.Add(HttpRequestHeader.Authorization, "Basic " + _client.AuthenticationToken);
                    }

                    try
                    {
                        var restResponse = (HttpWebResponse) restRequest.GetResponse();
                        string response;
                        using (var reader = new StreamReader(restResponse.GetResponseStream()))
                        {
                            response = reader.ReadToEnd();
                        }
                        var xDoc = XDocument.Parse(response.Normalize());
                        var packageList = xDoc.Root.Element("packages").Elements("package").Select(p => new PackageDetails
                            {
                                Id = p.Attribute("id").Value,
                                Version = p.Attribute("version").Value
                            }).ToList();

                        return packageList;
                    }
                    catch (WebException)
                    {
                        //Evil?  Yes :)
                        return new List<PackageDetails>();
                    }
                });
        }

        public class NuGetDependencies
        {
            public List<PackageDetails> Packages { get; set; }
        }

        public class PackageDetails
        {
            public string Id { get; set; }
            public string Version { get; set; }
        }

        public IEnumerable<ChangeDetail> GetChangeDetailsForLastBuildByBuildType(string buildType)
        {
            var latestBuild = GetLatestBuildByBuildType(buildType);
            return GetChangeDetailsByBuildId(latestBuild.Id);
        }

        public Build GetLatestBuildByBuildType(string buildType)
        {
            //TODO call directly using build locator for last successful, this is not fast....
            var builds = GetBuildsByBuildType(buildType);
            var latestBuild = builds.OrderByDescending(b => b.BuildTypeId).FirstOrDefault();
            return latestBuild;
        }

        public Build GetLatestSuccessfulBuildByBuildType(string buildType, string branchName = null)
        {
            //TODO call directly using build locator for last successful, this is not fast....
            var builds = GetBuildsByBuildType(buildType, branchName);
            var latestBuild = builds.Where(b => b.Status == "SUCCESS").OrderByDescending(b => b.BuildTypeId).FirstOrDefault();
            return latestBuild;
        }

        public IEnumerable<ChangeDetail> GetChangeDetailsByBuildId(string buildId)
        {
            var changeList = GetChangeListByBuildId(buildId);
            var changeDetails = changeList.Changes.Select(c => GetChangeDetailsByChangeId(c.Id)).ToList();
            return changeDetails;
        }

        public IEnumerable<ChangeDetail> GetChangeDetailsForCurrentBuildByBuildType(string buildType)
        {
            var response = GetRunningBuildByBuildType(buildType);

            if (response == null)
                return new List<ChangeDetail>();

            var firstResponse = response.FirstOrDefault();
            if (firstResponse == null)
                return new List<ChangeDetail>();

            var releaseNotes = GetChangeDetailsByBuildId(firstResponse.Id);
            return releaseNotes;
        }

        public IEnumerable<Build> GetRunningBuildByBuildType(string buildType, string branchName = null)
        {
            //NO CACHING HERE.  It says "running" on the tin, so caching is probably a bad idea....
            RestRequest request;
            if (string.IsNullOrEmpty(branchName))
            {
                request = GetXmlBuildRequest("app/rest/builds/?locator=buildType:{BT},running:true", "BT", buildType);
            }
            else
            {
                request = GetXmlBuildRequest(string.Format("app/rest/builds/?locator=buildType:{{BT}},running:true,branch:(name:{0})", branchName), "BT", buildType);
            }
            var response = _client.Execute<List<Build>>(request).Data;
            return response;
        }

        public IEnumerable<BuildType> GetBuildTypeByProjectAndName(string project, string buildName)
        {
            return _cache.GetFromCacheOrFunc(String.Format("{0}:{1}", project, buildName), query =>
                {
                    var request = GetXmlBuildRequest("app/rest/buildTypes");
                    var response = _client.Execute<List<BuildType>>(request);
                    return response.Data.Where(b => b.ProjectName.Equals(project, StringComparison.InvariantCultureIgnoreCase) && b.Name.Equals(buildName, StringComparison.InvariantCultureIgnoreCase));
                });
        }

        public IEnumerable<ChangeDetail> GetChangeDetailsByBuildTypeAndBuildId(string buildType, string from, string to, Func<Build, string, bool> comparitor, IEnumerable<Build> buildList = null, string branchName = null)
        {
            var results = GetByBuildTypeAndBuildRange(buildType, @from, to, comparitor, buildList, b => GetChangeDetailsByBuildId(b.Id), branchName: branchName);
            return results;
        }

        public IEnumerable<Issue> GetIssuesByBuildTypeAndBuildRange(string buildType, string from, string to, IEnumerable<Build> buildList = null, string branchName = null)
        {
            var results = GetByBuildTypeAndBuildRange(buildType, @from, to, BuildNumberComparitor(), buildList, b => GetIssuesFromBuild(b.Id), branchName: branchName);
            return results;
        }

        public IEnumerable<Issue> GetIssuesFromBuild(string buildId)
        {
            var buildDetails = GetBuildDetailsByBuildId(buildId);
            if (buildDetails != null)
            {
                return buildDetails.RelatedIssues.Select(i => i.Issue).Distinct().ToList();
            }
            return new List<Issue>();
        }

        private IEnumerable<T> GetByBuildTypeAndBuildRange<T>(string buildType, string @from, string to, Func<Build, string, bool> comparitor, IEnumerable<Build> buildList, Func<Build, IEnumerable<T>> retriever, bool excludeResultsFromLowerBound = true, string branchName = null)
        {
            var builds = buildList ?? GetBuildsByBuildType(buildType, branchName);
            var results = new List<T>();

            var captureChanges = false;
            foreach (var build in builds.OrderBy(b => Convert.ToInt32(b.Id)))
            {
                if (comparitor(build, from))
                {
                    captureChanges = true;
                    if (excludeResultsFromLowerBound) continue;
                }

                if (captureChanges)
                    results.AddRange(retriever(build));

                if (comparitor(build, to))
                    break;
            }
            return results;
        }

        public IEnumerable<ChangeDetail> GetChangeDetailsByBuildTypeAndBuildId(string buildType, string from, string to, string branchName = null)
        {
            return GetChangeDetailsByBuildTypeAndBuildId(buildType, from, to, BuildIdComparitor(), branchName: branchName);
        }

        private static Func<Build, string, bool> BuildIdComparitor()
        {
            return (build, s) => build.Id.Equals(s, StringComparison.InvariantCultureIgnoreCase);
        }

        public IEnumerable<ChangeDetail> GetChangeDetailsByBuildTypeAndBuildNumber(string buildType, string from, string to, IEnumerable<Build> buildList = null, string branchName = null)
        {
            return GetChangeDetailsByBuildTypeAndBuildId(buildType, from, to, BuildNumberComparitor(), buildList, branchName);
        }

        private static Func<Build, string, bool> BuildNumberComparitor()
        {
            return (build, s) => build.Number.Equals("None", StringComparison.InvariantCultureIgnoreCase) ? build.Id.ToString(CultureInfo.InvariantCulture).Equals(s, StringComparison.InvariantCultureIgnoreCase) : build.Number.Equals(s, StringComparison.InvariantCultureIgnoreCase);
        }

        public BuildDetails GetBuildDetailsByBuildId(string id)
        {
            return _cache.GetFromCacheOrFunc(id, query =>
                {
                    var request = GetXmlBuildRequest("app/rest/builds/id:{ID}", "ID", id);
                    var response = _client.Execute<BuildDetails>(request);
                    return response.Data;
                });
        }

        public ChangeList GetChangeListByBuildId(string id)
        {
            return _cache.GetFromCacheOrFunc(id, query =>
                {
                    var request = GetXmlBuildRequest("app/rest/changes?build=id:{ID}", "ID", id);
                    var response = _client.Execute<ChangeList>(request);
                    return response.Data;
                });
        }

        public ChangeDetail GetChangeDetailsByChangeId(string id)
        {
            return _cache.GetFromCacheOrFunc(id, query =>
                {

                    var request = GetXmlBuildRequest("app/rest/changes/id:{ID}", "ID", id);
                    var response = _client.Execute<ChangeDetail>(request);
                    return response.Data;
                });
        }

        public IEnumerable<Build> GetBuildsByBuildType(string buildType, string branchName = null)
        {
            return _cache.GetFromCacheOrFunc(buildType, branchName, (key1, key2) =>
                {
                    RestRequest request;
                    if (string.IsNullOrEmpty(branchName))
                    {
                        request = GetXmlBuildRequest("app/rest/builds/?locator=buildType:{ID}", "ID", buildType);
                    }
                    else
                    {
                        request = GetXmlBuildRequest(string.Format("app/rest/builds/?locator=buildType:{{ID}},branch:(name:{0})", branchName), "ID", buildType);
                    }
                    var response = _client.Execute<List<Build>>(request);
                    return response.Data;
                });
        }
    }

    public class BuildType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string WebUrl { get; set; }

        public override string ToString()
        {
            return string.Format("Id:{0} Name:{1} HREF:{2} ProjectName:{3} ProjectID:{4} WebUrl:{5}", Id, Name, Href, ProjectName, ProjectId, WebUrl);
        }
    }

    public class BuildTypeDetails : BuildType
    {
        public string Description { get; set; }
        public bool Paused { get; set; }
        public Project Project { get; set; }
        public List<VcsRootEntry> VcsRootEntries { get; set; }
        public Settings Settings { get; set; }
        public List<string> Builds { get; set; }
        public List<Trigger> Triggers { get; set; }
        public List<Step> Steps { get; set; }
        public List<Feature> Features { get; set; }
    }

    public class Feature : GenericTeamCityPropertyGroup
    {
    }

    public class Step : GenericTeamCityPropertyGroup
    {
        public string Name { get; set; }
    }

    public class Trigger : GenericTeamCityPropertyGroup
    {
    }

    public class GenericTeamCityPropertyGroup
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public List<Property> Properties { get; set; }
    }

    public class Settings
    {
        public List<Property> Properties { get; set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string value { get; set; }
    }

    public class VcsRootEntry
    {
        public string Id { get; set; }
        public string CheckoutRules { get; set; }
        public List<VcsRoot> VcsRoot { get; set; }
    }

    public class Project : GenericTeamCityStubValue
    {
    }

    public class VcsRoot : GenericTeamCityStubValue
    {
    }

    public class GenericTeamCityStubValue : GenericTeamCityBase
    {
        public string Name { get; set; }
    }

    public class GenericTeamCityBase
    {
        public string Id { get; set; }
        public string Href { get; set; }
    }

    public class IvyModule
    {
        public List<Artifact> Publications { get; set; }
    }

    public class Artifact
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Ext { get; set; }
    }

    public class Build : GenericTeamCityStubValue
    {
        public string Number { get; set; }
        public string Status { get; set; }
        public string BuildTypeId { get; set; }
        public string WebUrl { get; set; }
    }

    public class BuildDetails : Build
    {
        public Boolean Personal { get; set; }
        public Boolean History { get; set; }
        public Boolean Pinned { get; set; }
        public string StatusText { get; set; }
        public BuildType BuildType { get; set; }
        public string StartDate { get; set; }
        public string FinishDate { get; set; }
        public Agent Agent { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Property> Properties { get; set; }
        public List<Revision> Revisions { get; set; }
        public Triggered Triggered { get; set; }
        public ChangeSummary ChangeSummary { get; set; }
        public List<IssueUsage> RelatedIssues { get; set; }


        public static DateTimeOffset GetDateTimeOffset(string value)
        {
            //Values come in looking like this: 20121022T215947+1100
            var splitString = value.Split("+-".ToCharArray()).ToList();
            if (splitString.Count == 2)
            {
                var datePortion = DateTime.ParseExact(splitString[0], "yyyymmddTHHmmss", CultureInfo.CurrentCulture);
            }
            return new DateTimeOffset();
        }
    }

    public class IssueUsage
    {
        public Issue Issue { get; set; }
        public List<Changes> Changes { get; set; }
    }

    public class Changes
    {
        public int Count { get; set; }
        public List<Change> ChangeList { get; set; }
    }

    public class Issue : IEquatable<Issue>
    {
        public string Id { get; set; }
        public string Url { get; set; }

        public bool Equals(Issue other)
        {
            if (other == this)
                return true;

            return other.Id == Id && other.Url == Url;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType() && Equals((Issue) obj);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = Id != null ? (hash * 7) + Id.GetHashCode() : hash;
            hash = Url != null ? (hash * 7) + Url.GetHashCode() : hash;
            return hash;
        }
    }

    public class ChangeSummary
    {
        public string Href { get; set; }
        public int Count { get; set; }
    }

    public class Triggered
    {
        public string Type { get; set; }
        public string Details { get; set; }
        public string Date { get; set; }
        public User User { get; set; }
    }

    public class User : GenericTeamCityStubValue
    {
        public string UserName { get; set; }
    }

    public class Revision
    {
        public string Version { get; set; }
        public VcsRootInstance VcsRootInstance { get; set; }
    }

    public class VcsRootInstance : GenericTeamCityStubValue
    {
        public string VcsRootId { get; set; }
    }


    public class Tag
    {
        public string Name { get; set; }
    }

    public class Agent : GenericTeamCityStubValue
    {
    }

    public class ChangeList
    {
        public int Count { get; set; }
        public List<Change> Changes { get; set; }
    }

    public class Change : GenericTeamCityBase
    {
        public string Version { get; set; }
        public string Weblink { get; set; }
    }

    public class ChangeDetail : Change
    {
        public string Username { get; set; }
        public string Comment { get; set; }
        public List<FileDetails> Files { get; set; }
    }

    public class FileDetails
    {
        public string beforerevision { get; set; }
        public string afterrevision { get; set; }
        public string File { get; set; }
        public string relativefile { get; set; }
    }
}
