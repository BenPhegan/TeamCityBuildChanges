using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rally.RestApi;

namespace TeamCityBuildChanges.ExternalApi.Rally
{
    public class RallyApi : IRallyApi
    {
        private readonly RallyRestApi _rallyRestApi;
        private readonly Regex _defectIdResolver;
        private readonly Regex _UserStoryIdResolver;

        public RallyApi(string username, string password, string endpoint)
        {
            _rallyRestApi = new RallyRestApi(username, password, endpoint);
            _defectIdResolver = new Regex("/defect/(?<Id>[0-9]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _UserStoryIdResolver = new Regex("/PortfolioItem/Feature/UserStories/(?<Id>[0-9]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public Defect GetRallyDefect(string key)
        {
            var request = new Request("defect")
                {
                    Fetch = new List<string> { "Name", "Description", "State", "CreationDate" },
                    Query = new Query("FormattedId", Query.Operator.Equals, key)
                };

            var queryResult = _rallyRestApi.Query(request);

            var firstResult = queryResult.Results.FirstOrDefault();

            if (firstResult == null)
                return null;

            // Extract the defect id
            var defectIdMatch = _defectIdResolver.Match(firstResult["_ref"] as string);
            if (!defectIdMatch.Success)
                return null;

            var defectId = defectIdMatch.Groups["Id"].Value;

            var baseUri = new Uri(_rallyRestApi.WebServiceUrl);
            var baseUrl = string.Format("{0}://{1}#/detail/defect/{2}", baseUri.Scheme, baseUri.Host, defectId);

            return new Defect
                {
                    Id = defectId,
                    FormattedId = key,
                    Created = DateTime.Parse(firstResult["CreationDate"]),
                    State = firstResult["State"],
                    Name = firstResult["Name"],
                    Description = firstResult["Description"],
                    Url = baseUrl
                };
        }

        public UserStory GetRallyUserStory(string key)
        {
            var request = new Request("PortfolioItem/Feature/UserStory")
            {
                Fetch = new List<string> { "Name", "Description", "State", "CreationDate" },
                Query = new Query("FormattedId", Query.Operator.Equals, key)
            };

            var queryResult = _rallyRestApi.Query(request);

            var firstResult = queryResult.Results.FirstOrDefault();

            if (firstResult == null)
                return null;

            // Extract the user story id
            var userStoryIdMatch = _UserStoryIdResolver.Match(firstResult["_ref"] as string);
            if (!userStoryIdMatch.Success)
                return null;

            var userStoryId = userStoryIdMatch.Groups["Id"].Value;

            var baseUri = new Uri(_rallyRestApi.WebServiceUrl);
            var baseUrl = string.Format("{0}://{1}#/detail/userstory/{2}", baseUri.Scheme, baseUri.Host, userStoryId);

            return new UserStory
                {
                    Id = userStoryId,
                    FormattedId = key,
                    Created = DateTime.Parse(firstResult["CreationDate"]),
                    State = firstResult["State"],
                    Name = firstResult["Name"],
                    Description = firstResult["Description"],
                    Url = baseUrl
                };
        }
    }
}