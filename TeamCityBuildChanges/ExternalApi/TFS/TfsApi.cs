using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityBuildChanges.ExternalApi.TFS
{
    public class TfsApi
    {
        private string _connectionUri;

        public TfsApi(string connectionUri)
        {
            _connectionUri = connectionUri;
        }



        internal TfsWorkItem GetWorkItem(int workItemId)
        {
            throw new NotImplementedException();
        }
    }
}
