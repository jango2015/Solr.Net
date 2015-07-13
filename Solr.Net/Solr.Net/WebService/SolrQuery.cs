using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Solr.Net.Linq;

namespace Solr.Net.WebService
{
    public class SolrQuery<TDocument> where TDocument : new()
    {
        private readonly SolrClient _client;
        private readonly string _query;
        private int _limit = 50;
        private int _offset = 0;
        private readonly List<string> _filters = new List<string>();

        public SolrQuery(SolrClient client, string query)
        {
            _client = client;
            _query = query;
        }

        public SolrQuery<TDocument> Filter(Expression<Func<TDocument, bool>> predicate)
        {
            _filters.Add(new SolrQueryTranslator().Translate(predicate));
            return this;
        }

        public SolrQuery<TDocument> Skip(int n)
        {
            _offset = n;
            return this;
        }

        public SolrQuery<TDocument> Take(int n)
        {
            _limit = n;
            return this;
        }

        public void Execute()
        {
            var requestObject = new
            {
                Query = _query,
                Offset = _offset,
                Limit = _limit,
                Filters = _filters
            };
            var serializeObject = JsonConvert.SerializeObject(requestObject);
            Console.WriteLine(serializeObject);
        }
    }
}
