using System;
using Newtonsoft.Json;

namespace Solr.Net.Serialization
{
    public class SolrJsonConverter<TDocument> : JsonConverter
    {
        private readonly IFieldResolver _fieldResolver;

        public SolrJsonConverter(IFieldResolver fieldResolver)
        {
            _fieldResolver = fieldResolver;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var mappingSerializer = new JsonSerializer { ContractResolver = new SolrContractResolver(_fieldResolver) };
            mappingSerializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var mappingSerializer = new JsonSerializer {ContractResolver = new SolrContractResolver(_fieldResolver)};
            return mappingSerializer.Deserialize<TDocument>(reader);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (TDocument);
        }
    }
}