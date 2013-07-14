namespace TeamCityBuildChanges.IssueDetailResolvers
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using TeamCityBuildChanges.ExternalApi.TeamCity;

    public class IssueEqualityComparer : IEqualityComparer<Issue>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
        public bool Equals(Issue x, Issue y)
        {
            return x.Id.Equals(y.Id);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(Issue obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}