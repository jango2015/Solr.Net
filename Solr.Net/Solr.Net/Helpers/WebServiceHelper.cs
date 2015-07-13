using System.Collections.Generic;
using System.Linq;

namespace Solr.Net.Helpers
{
    class WebServiceHelper
    {
        public static IQueryable<T> GetResultsFromServerAsQueryable<T>(object query)
        {
            // Call the Web service method "GetPlaceList".
            return new T[10].AsQueryable();
        }
    }
}
