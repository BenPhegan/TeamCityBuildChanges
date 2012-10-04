﻿using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace TeamCityBuildChanges
{
    internal class TeamCityApi
    {
        private readonly string _teamCityServer;
        private readonly RestClient _client;

        public TeamCityApi(string server)
        {
            _teamCityServer = server;
            _client = new RestClient(_teamCityServer + "/guestAuth/");
        }

        public List<BuildType> GetBuildTypes()
        {
            var request = new RestRequest("app/rest/buildTypes", Method.GET) {RequestFormat = DataFormat.Xml};
            request.AddHeader("Accept", "application/xml");

            var buildConfigs = _client.Execute<List<BuildType>>(request);
            return buildConfigs.Data;
        }

        public BuildTypeDetails GetBuildTypeDetailsById(string id)
        {
            var buildDetails = GetXmlBuildRequest("app/rest/buildTypes/id:{ID}","ID",id);
            var response = _client.Execute<BuildTypeDetails>(buildDetails);
            return response.Data;
        }

        private static RestRequest GetXmlBuildRequest(string endpoint, string variable, string replacement)
        {
            var request = new RestRequest(endpoint, Method.GET);
            request.AddParameter(variable, replacement, ParameterType.UrlSegment);
            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("Accept", "application/xml");
            return request;
        }

        public IEnumerable<Artifact> GetArtifactListByBuildType(string buildType)
        {
            var request = new RestRequest("repository/download/{ID}/lastSuccessful/teamcity-ivy.xml");
            request.AddParameter("ID", buildType, ParameterType.UrlSegment);
            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("Accept", "application/xml");
            var response = _client.Execute<IvyModule>(request);
            return response.Data != null ? response.Data.Publications : new List<Artifact>();
        }

        public IEnumerable<ChangeDetail> GetReleaseNotesForLastBuildByBuildType(string buildType)
        {
            var builds = GetBuildsByBuildType(buildType);
            var latestBuild = builds.OrderByDescending(b => b.BuildTypeId).FirstOrDefault();
            return GetReleaseNotesByBuildId(latestBuild.Id);
        }

        public IEnumerable<ChangeDetail> GetReleaseNotesByBuildId(string buildId)
        {
            var changeList = GetChangeListByBuildId(buildId);
            var changeDetails = changeList.Changes.Select(c => GetChangeDetailsByChangeId(c.Id)).ToList();
            return changeDetails;
        }

        public IEnumerable<ChangeDetail> GetReleaseNotesForCurrentBuildByBuildType(string buildType)
        {
            var request = GetXmlBuildRequest("app/rest/builds/?locator=buildType:{BT},running:true", "BT", buildType);
            var response = _client.Execute<List<Build>>(request);
            if (response.Data == null)
                return new List<ChangeDetail>();

            var releaseNotes = GetReleaseNotesByBuildId(response.Data.FirstOrDefault().Id);
            return releaseNotes;
        }

        private IEnumerable<ChangeDetail> GetReleaseNotesByBuildTypeAndBuildId(string buildType, string from, string to, Func<Build, string, bool> comparitor)
        {
            var builds = GetBuildsByBuildType(buildType);
            var changeDeltas = new List<ChangeDetail>();
            var captureChanges = false;
            foreach (var build in builds.OrderBy(b => b.Id))
            {
                if (comparitor(build, from))
                    captureChanges = true;

                if (captureChanges)
                    changeDeltas.AddRange(GetReleaseNotesByBuildId(build.Id));

                if (comparitor(build, to))
                    break;
            }
            return changeDeltas;
        }

        public IEnumerable<ChangeDetail> GetReleaseNotesByBuildTypeAndBuildId(string buildType, string from, string to)
        {
            return GetReleaseNotesByBuildTypeAndBuildId(buildType, from, to, (build, s) => build.Id.Equals(s, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<ChangeDetail> GetReleaseNotesByBuildTypeAndBuildNumber(string buildType, string from, string to)
        {
            return GetReleaseNotesByBuildTypeAndBuildId(buildType, from, to, (build, s) => build.Number.Equals(s, StringComparison.InvariantCultureIgnoreCase));
        }

        public BuildDetails GetBuildDetailsByBuildId(string id)
        {
            var request = GetXmlBuildRequest("app/rest/builds/id:{ID}", "ID", id);
            var response = _client.Execute<BuildDetails>(request);
            return response.Data;
        }

        public ChangeList GetChangeListByBuildId(string id)
        {
            var request = GetXmlBuildRequest("app/rest/changes?build=id:{ID}", "ID", id);
            var response = _client.Execute<ChangeList>(request);
            return response.Data;
        }

        public ChangeDetail GetChangeDetailsByChangeId(string id)
        {
            var request = GetXmlBuildRequest("app/rest/changes/id:{ID}", "ID", id);
            var response = _client.Execute<ChangeDetail>(request);
            return response.Data;
        }

        public IEnumerable<Build> GetBuildsByBuildType(string buildType)
        {
            var request = GetXmlBuildRequest("app/rest/builds/?locator=buildType:{ID}", "ID", buildType);
            var response = _client.Execute<List<Build>>(request);
            return response.Data;
        }
    }

    public class BuildType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public Uri WebUrl { get; set; }

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

    public class Feature : GenericTeamCityPropertyGroup {}

    public class Step : GenericTeamCityPropertyGroup
    {
        public string Name { get; set; }
    }

    public class Trigger : GenericTeamCityPropertyGroup {}

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
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public Agent Agent { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Property> Properties { get; set; }
        public List<Revision> Revisions { get; set; }
        public Triggered Triggered { get; set; }
        public ChangeSummary ChangeSummary { get; set; }
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
        public DateTime Date { get; set; }
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
        public string afterrevision{ get; set; }
        public string File { get; set; }
        public string relativefile { get; set; }
    }
}
