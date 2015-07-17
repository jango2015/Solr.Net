using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Solr.Client.Serialization;

namespace Solr.Client.WebService
{
    public interface ISolrQueryFacet
    {
        object Translate(SolrExpressionTranslator translator);
    }

    public class SolrQueryStatisticsFacet<TDocument> : ISolrQueryFacet
    {
        private readonly Expression<Func<TDocument, object>> _expression;

        public SolrQueryStatisticsFacet(string expression)
        {
            _expression = document => SolrLiteral.String(expression);
        }

        public SolrQueryStatisticsFacet(Expression<Func<TDocument, object>> expression)
        {
            _expression = expression;
        }

        public object Translate(SolrExpressionTranslator translator)
        {
            return translator.Translate(_expression);
        }
    }

    public abstract class SolrQueryFacet : ISolrQueryFacet
    {
        private readonly Dictionary<string, ISolrQueryFacet> _facets = new Dictionary<string, ISolrQueryFacet>();
        protected SolrExpressionTranslator Translator;

        [JsonProperty(PropertyName = "facet", NullValueHandling = NullValueHandling.Ignore)]
        internal IDictionary<string, object> Facets
        {
            get
            {
                return _facets.ToDictionary(queryFacet => queryFacet.Key,
                    queryFacet => queryFacet.Value.Translate(Translator));
            }
        }

        public SolrQueryFacet Facet(string name, ISolrQueryFacet facet)
        {
            _facets.Add(name, facet);
            return this;
        }

        public object Translate(SolrExpressionTranslator translator)
        {
            Translator = translator;
            return this;
        }
    }

    public class SolrQueryTermsFacet<TDocument> : SolrQueryFacet
    {
        private readonly Expression<Func<TDocument, object>> _field;

        [JsonProperty(PropertyName = "type")]
        internal static string Type
        {
            get { return "terms"; }
        }

        [JsonProperty(PropertyName = "field")]
        internal string Field
        {
            get { return Translator.Translate(_field); }
        }

        [JsonProperty(PropertyName = "offset", NullValueHandling = NullValueHandling.Ignore)]
        public int? Offset { get; set; }
        [JsonProperty(PropertyName = "limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? Limit { get; set; }
        [JsonProperty(PropertyName = "mincount", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinCount { get; set; }
        [JsonProperty(PropertyName = "numBuckets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NumBuckets { get; set; }
        [JsonProperty(PropertyName = "allBuckets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllBuckets { get; set; }
        [JsonProperty(PropertyName = "prefix", NullValueHandling = NullValueHandling.Ignore)]
        public string Prefix { get; set; }

        public SolrQueryTermsFacet(string field)
        {
            _field = document => SolrLiteral.String(field);
        }

        public SolrQueryTermsFacet(Expression<Func<TDocument, object>> expression)
        {
            _field = expression;
        }

        public SolrQueryTermsFacet<TDocument> Skip(int n)
        {
            Offset = n;
            return this;
        }

        public SolrQueryTermsFacet<TDocument> Take(int n)
        {
            Limit = n;
            return this;
        }
    }
}
