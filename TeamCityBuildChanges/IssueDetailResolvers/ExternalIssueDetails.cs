using System;
using System.Collections.Generic;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class ExternalIssueDetails
    {

        public string Id { get; set; }
        public string Status { get; set; }
        public string Created { get; set; }
        public List<string> Comments { get; set; }
        public List<ExternalIssueDetails> SubIssues { get; set; }

        protected bool Equals(ExternalIssueDetails other)
        {
            return string.Equals(Id, other.Id) 
                && string.Equals(Status, other.Status) 
                && string.Equals(Created, other.Created) 
                && Equals(Comments, other.Comments) 
                && Equals(SubIssues, other.SubIssues);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Status != null ? Status.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Created != null ? Created.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Comments != null ? Comments.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (SubIssues != null ? SubIssues.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ExternalIssueDetails) obj);
        }
    }
}