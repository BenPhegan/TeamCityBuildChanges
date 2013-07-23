using System;
using System.Text;

namespace TeamCityBuildChanges.ExternalApi
{
    public static class UriExtensions
    {
        /// <summary>
        /// Resolves an encoded auth token based on the uri if username and password are specified.  Also provides a clean uri (out param) without a username and password.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="cleanUri">The new, clean URI with no auth details.</param>
        /// <returns></returns>
        public static string TryResolveAuthToken(this Uri uri, out Uri cleanUri)
        {
            cleanUri = uri;

            var uriBuilder = new UriBuilder(uri);
            var userName = uriBuilder.UserName;
            var password = uriBuilder.Password;

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                uriBuilder.UserName = string.Empty;
                uriBuilder.Password = string.Empty;
                cleanUri = new Uri(uriBuilder.ToString());

                // extract the authtoken based on the url params
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", userName, password)));
            }
            return null;
        }
    }
}