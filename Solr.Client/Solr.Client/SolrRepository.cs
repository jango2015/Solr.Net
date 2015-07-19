using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solr.Client.Linq;
using Solr.Client.Serialization;
using Solr.Client.WebService;

namespace Solr.Client
{
    public class SolrRepository
    {
        private readonly ISolrConfiguration _configruation;
        private readonly ISolrFieldResolver _fieldResolver;
        private readonly JsonSerializer _serializer;

        public SolrRepository(ISolrConfiguration configruation, ISolrFieldResolver fieldResolver = null)
        {
            _configruation = configruation;
            _fieldResolver = fieldResolver ?? new DefaultSolrFieldResolver();
            _serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new SolrContractResolver(_fieldResolver)
            };
            _serializer.Converters.Add(new SolrDateTimeConverter());
        }

        public virtual async Task Add<TDocument>(TDocument document)
        {
            var mappedObject = JToken.FromObject(document, _serializer);
            await Client.Add(mappedObject);
        }

        public virtual async Task<SolrSearchResult<TDocument>> Search<TDocument>(IQueryable<TDocument> query)
        {
            return await Search<TDocument, TDocument>(query, _fieldResolver);
        }

        public virtual async Task<SolrSearchResult<TResult>> Search<TDocument, TResult>(IQueryable<TDocument> query,
            ISolrFieldResolver resultFieldResolver = null)
        {
            var request = new SolrQueryExpressionVisitor(_fieldResolver).Translate(query.Expression);
            var response = await Client.Get<JToken>(request);
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
                Start = response.Response.Start
            };
            return result;
        }

        public SolrClient Client
        {
            get
            {
                return new SolrClient(_configruation);
            }
        }

        public async Task Remove(object id)
        {
            await Client.Remove(id);
        }

    }

    public class SolrSearchResult<TDocument>
    {
        public IEnumerable<TDocument> Documents { get; set; }
        public long NumFound { get; set; }
        public long Start { get; set; }
    }
}