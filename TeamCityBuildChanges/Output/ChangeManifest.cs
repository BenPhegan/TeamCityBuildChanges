using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TeamCityBuildChanges.ExternalApi.TeamCity;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Output
{
    public class ChangeManifest
    {
        public List<ChangeDetail> ChangeDetails { get; set; }
        public List<ExternalIssueDetails> IssueDetails { get; set; }
        public DateTime Generated { get; set; }
        public string FromVersion { get; set; }
        public string ToVersion { get; set; }
        public BuildTypeDetails ReferenceBuildConfiguration { get; set; }
        public BuildTypeDetails BuildConfiguration { get; set; }

        public ChangeManifest()
        {
            ChangeDetails = new List<ChangeDetail>();
            IssueDetails = new List<ExternalIssueDetails>();
        }

        public List<ExternalIssueDetails> FlattenedIssueDetails
        {
            get
            {
                var returnList = new List<ExternalIssueDetails>();
                var flattened = IssueDetails.Map(p => true, n => n.SubIssues).Distinct().Where(f => f != null).ToList();
                foreach (var issue in flattened)
                {
                    //Root node check
                    if ((issue.SubIssues == null || (issue.SubIssues != null && !issue.SubIssues.Any())) && !returnList.Contains(issue))
                    {
                        returnList.Add(issue);
                        continue;
                    }
                    
                }
                return flattened.ToList();
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ChangeManifest) obj);
        }

        protected bool Equals(ChangeManifest other)
        {
            return Equals(ChangeDetails, other.ChangeDetails)
                   && Equals(IssueDetails, other.IssueDetails)
                   && Generated.Equals(other.Generated)
                   && string.Equals(FromVersion, other.FromVersion)
                   && string.Equals(ToVersion, other.ToVersion)
                   && string.Equals(ReferenceBuildConfiguration, other.ReferenceBuildConfiguration)
                   && string.Equals(BuildConfiguration, other.BuildConfiguration);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (ChangeDetails != null ? ChangeDetails.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (IssueDetails != null ? IssueDetails.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Generated.GetHashCode();
                hashCode = (hashCode*397) ^ (FromVersion != null ? FromVersion.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ToVersion != null ? ToVersion.GetHashCode() : 0);
                hashCode = (hashCode*397) ^
                           (ReferenceBuildConfiguration != null ? ReferenceBuildConfiguration.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (BuildConfiguration != null ? BuildConfiguration.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public static class Extensions
    {
        public static IEnumerable<TSource> Map<TSource>(this IEnumerable<TSource> source,Func<TSource, bool> selectorFunction,Func<TSource, IEnumerable<TSource>> getChildrenFunction)
        {
            // Add what we have to the stack
            if (source == null)
                return new List<TSource>();

            var flattenedList = source.Where(selectorFunction);

            // Go through the input enumerable looking for children,
            // and add those if we have them
            foreach (TSource element in source)
            {
                flattenedList = flattenedList.Concat(getChildrenFunction(element).Map(selectorFunction, getChildrenFunction));
            }
            return flattenedList;
        }
    }
}