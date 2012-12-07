using System;
using System.Collections.Generic;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Output
{
    public class ExternalIssueDetailsComparer : IComparer<ExternalIssueDetails>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
        /// </returns>
        public int Compare(ExternalIssueDetails x, ExternalIssueDetails y)
        {
            return String.Compare(x.Id, y.Id, StringComparison.Ordinal);
        }
    }
}