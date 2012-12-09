using System.Collections.Generic;
using TeamCityBuildChanges.IssueDetailResolvers;

namespace TeamCityBuildChanges.Output
{
    public class ExternalIssueDetailsEqualityComparer : IEqualityComparer<ExternalIssueDetails>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(ExternalIssueDetails x, ExternalIssueDetails y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
        public int GetHashCode(ExternalIssueDetails obj)
        {
            return obj.GetHashCode();
        }
    }
}