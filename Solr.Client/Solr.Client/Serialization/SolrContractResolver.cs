using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Solr.Client.Serialization
{
    public class SolrContractResolver : DefaultContractResolver
    {
        private readonly ISolrFieldResolver _fieldResolver;

        public SolrContractResolver(ISolrFieldResolver fieldResolver)
        {
            _fieldResolver = fieldResolver;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (_fieldResolver != null)
            {
                property.PropertyName = _fieldResolver.GetFieldName(member);
            }
            return property;
        }
    }
}