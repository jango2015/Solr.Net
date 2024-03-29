﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Solr.Client.Serialization
{
    public class SolrDateTimeConverter : IsoDateTimeConverter
    {
        public SolrDateTimeConverter()
        {
            DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dateTime = value as DateTime?;
            if (dateTime.HasValue && dateTime.Value.Kind != DateTimeKind.Utc)
                base.WriteJson(writer, dateTime.Value.ToUniversalTime(), serializer);
            else base.WriteJson(writer, value, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dateTime = reader.Value as DateTime?;
            return dateTime.HasValue
                ? dateTime.Value.ToLocalTime()
                : base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
