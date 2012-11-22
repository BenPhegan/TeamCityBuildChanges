using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;

namespace TeamCityBuildChanges.ExternalApi.Jira
{
    public class JiraApi
    {
        private readonly string _jiraAuthToken;
        private readonly Lazy<RestClient> _client;

        public JiraApi(string url, string jiraAuthToken)
        {
            _jiraAuthToken = jiraAuthToken;
            _client = new Lazy<RestClient>(() => new RestClient(url));
        }

        public static string GetEncodedCredentials(string username, string password)
        {
            var mergedCredentials = string.Format("{0}:{1}", username, password);
            var byteCredentials = Encoding.UTF8.GetBytes(mergedCredentials);
            return byteCredentials.ToString();
        }

        public RootObject GetJiraIssue(string key)
        {
            var request = new RestRequest(string.Format("/rest/api/2/issue/{0}", key), Method.GET);
            request.AddHeader("Authorization", "Basic " + _jiraAuthToken);
            try
            {
                IRestResponse<RootObject> response = _client.Value.Execute<RootObject>(request);
                return response.Data;
            }
            catch (Exception e)
            {
                return null;
            }
        }


    }



    #region Nested type: Aggregateprogress

    public class Aggregateprogress
    {
        public int Progress { get; set; }
        public int Total { get; set; }
    }

    #endregion

    #region Nested type: Assignee

    public class Assignee
    {
        public string Self { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
    }

    #endregion

    #region Nested type: Author

    public class Author
    {
        public string Self { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
    }

    #endregion

    #region Nested type: Comment

    public class Comment
    {
        public int StartAt { get; set; }
        public int MaxResults { get; set; }
        public int Total { get; set; }
        public List<Comment2> Comments { get; set; }
    }

    #endregion

    #region Nested type: Comment2

    public class Comment2
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public Author Author { get; set; }
        public string Body { get; set; }
        public UpdateAuthor UpdateAuthor { get; set; }
        public string Created { get; set; }
        public string Updated { get; set; }
    }

    #endregion


    #region Nested type: Fields

    public class Fields
    {
        public string Summary { get; set; }
        public Progress Progress { get; set; }
        public Issuetype Issuetype { get; set; }
        public Votes Votes { get; set; }
        public Resolution Resolution { get; set; }
        public List<FixVersion> FixVersions { get; set; }
        public string Resolutiondate { get; set; }
        public object Timespent { get; set; }
        public Reporter Reporter { get; set; }
        public object AggregateTimeOriginalEstimate { get; set; }
        public string Created { get; set; }
        public string Updated { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public object DueDate { get; set; }
        public List<object> IssueLinks { get; set; }
        public Watches Watches { get; set; }
        public List<Subtask> Subtasks { get; set; }
        public Status2 Status { get; set; }
        public List<object> Labels { get; set; }
        public int Workratio { get; set; }
        public Assignee Assignee { get; set; }
        public List<object> Attachment { get; set; }
        public object AggregateTimeEstimate { get; set; }
        public Project Project { get; set; }
        public object Environment { get; set; }
        public object TimeEstimate { get; set; }
        public Aggregateprogress AggregateProgress { get; set; }
        public Comment Comment { get; set; }
        public object TimeOriginalEstimate { get; set; }
        public object AggregateTimeSpent { get; set; }
    }

    #endregion

    #region Nested type: Fields2

    public class Fields2
    {
        public string Summary { get; set; }
        public Status Status { get; set; }
        public Issuetype2 Issuetype { get; set; }
    }

    #endregion

    #region Nested type: FixVersion

    public class FixVersion
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Archived { get; set; }
        public bool Released { get; set; }
        public string ReleaseDate { get; set; }
    }

    #endregion

    #region Nested type: Issuetype

    public class Issuetype
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public bool Subtask { get; set; }
    }

    #endregion

    #region Nested type: Issuetype2

    public class Issuetype2
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public bool Subtask { get; set; }
    }

    #endregion

    #region Nested type: Priority

    public class Priority
    {
        public string Self { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    #endregion

    #region Nested type: Progress

    public class Progress
    {
        public int progress { get; set; }
        public int Total { get; set; }
    }

    #endregion

    #region Nested type: Project

    public class Project
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region Nested type: Reporter

    public class Reporter
    {
        public string Self { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
    }

    #endregion

    #region Nested type: Resolution

    public class Resolution
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region Nested type: RootObject

    public class RootObject
    {
        public string Expand { get; set; }
        public string Id { get; set; }
        public string Self { get; set; }
        public string Key { get; set; }
        public Fields Fields { get; set; }
    }

    #endregion

    #region Nested type: Status

    public class Status
    {
        public string Self { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    #endregion

    #region Nested type: Status2

    public class Status2
    {
        public string Self { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    #endregion

    #region Nested type: Subtask

    public class Subtask
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Self { get; set; }
        public Fields2 Fields { get; set; }
    }

    #endregion

    #region Nested type: UpdateAuthor

    public class UpdateAuthor
    {
        public string Self { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool Active { get; set; }
    }

    #endregion

    #region Nested type: Votes

    public class Votes
    {
        public string Self { get; set; }
        public int votes { get; set; }
        public bool HasVoted { get; set; }
    }

    #endregion

    #region Nested type: Watches

    public class Watches
    {
        public string Self { get; set; }
        public int WatchCount { get; set; }
        public bool IsWatching { get; set; }
    }

    #endregion
}


