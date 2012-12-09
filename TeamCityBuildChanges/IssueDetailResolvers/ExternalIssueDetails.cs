using System;
using System.Collections.Generic;
using System.Linq;
using TeamCityBuildChanges.Output;

namespace TeamCityBuildChanges.IssueDetailResolvers
{
    public class ExternalIssueDetails
    {

        public string Id { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Created { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int Depth { get; set; }
        public List<string> Comments { get; set; }
        public List<ExternalIssueDetails> SubIssues { get; set; }

        public bool ContainsSubIssue(ExternalIssueDetails issue)
        {
            return SubIssues.Contains(issue, new ExternalIssueDetailsEqualityComparer());
        }

        protected bool Equals(ExternalIssueDetails other)
        {
            return string.Equals(Id, other.Id)
                && string.Equals(Status, other.Status)
                && string.Equals(Type, other.Type)
                && string.Equals(Created, other.Created)
                && string.Equals(Summary, other.Summary)
                && string.Equals(Description, other.Description)
                && string.Equals(Url, other.Url);
                //&& Equals(Comments, other.Comments) 
                //&& Equals(SubIssues, other.SubIssues);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Status != null ? Status.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Created != null ? Created.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Summary != null ? Summary.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Url != null ? Url.GetHashCode() : 0);
                //hashCode = (hashCode * 397) ^ Depth.GetHashCode();
                //hashCode = (hashCode*397) ^ (Comments != null ? Comments.GetHashCode() : 0);
                //hashCode = (hashCode*397) ^ (SubIssues != null ? SubIssues.GetHashCode() : 0);
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