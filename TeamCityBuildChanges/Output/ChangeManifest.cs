using System;
using System.Collections.Generic;
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
                hashCode = (hashCode * 397) ^ (IssueDetails != null ? IssueDetails.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Generated.GetHashCode();
                hashCode = (hashCode * 397) ^ (FromVersion != null ? FromVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToVersion != null ? ToVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReferenceBuildConfiguration != null ? ReferenceBuildConfiguration.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BuildConfiguration != null ? BuildConfiguration.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}