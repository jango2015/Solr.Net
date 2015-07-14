using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Solr.Net.Serialization
{
    public class SolrContractResolver : DefaultContractResolver
    {
        private readonly IFieldResolver _fieldResolver;

        public SolrContractResolver(IFieldResolver fieldResolver)
        {
            _fieldResolver = fieldResolver;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.PropertyName = _fieldResolver.GetFieldName(member);
            return property;
        }
    }
}