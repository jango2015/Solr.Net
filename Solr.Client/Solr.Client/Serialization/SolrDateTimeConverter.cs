using Newtonsoft.Json.Converters;

namespace Solr.Client.Serialization
{
    public class SolrDateTimeConverter : IsoDateTimeConverter
    {
        public SolrDateTimeConverter()
        {
            DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
        }
    }
}
