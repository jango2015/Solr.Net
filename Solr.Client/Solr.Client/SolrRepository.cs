using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solr.Client.Linq;
using Solr.Client.Serialization;
using Solr.Client.WebService;

namespace Solr.Client
{
    public class SolrRepository : IDisposable
    {
        private readonly ISolrFieldResolver _fieldResolver;
        private readonly JsonSerializer _serializer;
        private readonly SolrClient _client;

        public SolrRepository(ISolrConfiguration configruation, ISolrFieldResolver fieldResolver = null)
        {
            _client = new SolrClient(configruation);
            _fieldResolver = fieldResolver ?? new DefaultSolrFieldResolver();
            _serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SolrContractResolver(_fieldResolver)
            };
            _serializer.Converters.Add(new SolrDateTimeConverter());
        }

        public virtual async Task AddAsync<TDocument>(TDocument document)
        {
            var mappedObject = JToken.FromObject(document, _serializer);
            await _client.AddAsync(mappedObject);
        }

        public virtual async Task<SolrSearchResult<TDocument>> SearchAsync<TDocument>(IQueryable<TDocument> query)
        {
            return await SearchAsync<TDocument, TDocument>(query, _fieldResolver);
        }

        public virtual async Task<SolrSearchResult<TResult>> SearchAsync<TDocument, TResult>(IQueryable<TDocument> query,
            ISolrFieldResolver resultFieldResolver = null)
        {
            var request = new SolrQueryExpressionVisitor(_fieldResolver).Translate(query.Expression);
            var response = await _client.GetAsync<JToken>(request);
            var resultSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SolrContractResolver(resultFieldResolver)
            };
            resultSerializer.Converters.Add(new SolrDateTimeConverter());
            var result = new SolrSearchResult<TResult>
            {
                Documents = response.Response.Documents.Select(x => x.ToObject<TResult>(resultSerializer)),
                NumFound = response.Response.NumFound,
                Start = response.Response.Start,
                Raw = response
            };
            return result;
        }

        public async Task RemoveAsync<TDocument>(Expression<Func<TDocument, object>> query)
        {
            var queryString = new SolrLuceneExpressionVisitor(_fieldResolver).Translate(query);
            await _client.RemoveByQueryAsync(queryString);
        }

        public async Task RemoveAsync(object id)
        {
            await _client.RemoveByIdAsync(id);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    public class SolrSearchResult<TDocument>
    {
        public IEnumerable<TDocument> Documents { get; set; }
        public long NumFound { get; set; }
        public long Start { get; set; }
        public SolrQueryResponse<JToken> Raw { get; set; }
    }
}