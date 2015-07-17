using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Solr.Client.Serialization;

namespace Solr.Client.WebService
{
    public class SolrClient : IDisposable
    {
        private readonly ISolrConfiguration _configuration;
        private readonly ISolrFieldResolver _fieldResolver;
        private readonly HttpClient _httpClient;
        private ILog _logger;

        public SolrClient(ISolrConfiguration configuration, ISolrFieldResolver fieldResolver)
        {
            _configuration = configuration;
            _fieldResolver = fieldResolver;
            _httpClient = new HttpClient();
            _logger = LogManager.GetLogger(GetType());
        }

        public async Task Commit()
        {
            var request = new SolrUpdateRequest {Commit = new object()};
            await PostAsJsonAsync<SolrUpdateRequest, SolrResponse>(_configuration.UpdateUrl, request);
        }

        public async Task Add<TDocument>(TDocument document, bool commit = true)
        {
            var request = new SolrUpdateRequest
            {
                Add = new SolrAddRequest(document),
                Commit = commit ? new object() : null
            };
            var settings = new JsonSerializerSettings
            {
                Converters =
                    new List<JsonConverter>
                    {
                        new SolrDateTimeConverter(),
                        new SolrJsonConverter<TDocument>(_fieldResolver)
                    }
            };
            await PostAsJsonAsync<SolrUpdateRequest, SolrResponse>(_configuration.UpdateUrl, request, settings);
        }

        private async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest request,
            JsonSerializerSettings settings = null)
            where TResponse : SolrResponse
        {
            var requestString = JsonConvert.SerializeObject(request, settings);
            _logger.Debug(string.Format("Requesting '{0}' with post data: {1}", url, requestString));
            var content = new StringContent(requestString);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.Debug(string.Format("Received response: {0}", responseString));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(string.Format("Request failed with statuscode {0} {1}: {2}",
                    (int) response.StatusCode, response.ReasonPhrase, responseString));
            }
            var responseObject = JsonConvert.DeserializeObject<TResponse>(responseString, settings);
            if (!responseObject.IsSuccessStatusCode)
            {
                throw new HttpRequestException(responseObject.Error == null
                    ? string.Format("Solr returned status {0} without further details.",
                        responseObject.ResponseHeader.Status)
                    : responseObject.Error.GetDescription());
            }
            return responseObject;
        }

        public async Task<SolrQueryResponse<TDocument>> Get<TDocument>(SolrQuery<TDocument> query)
        {
            return await Get<TDocument, TDocument>(query);
        }

        public async Task<SolrQueryResponse<TResult>> Get<TDocument, TResult>(SolrQuery<TDocument> query)
        {
            var translator = new SolrExpressionTranslator(_fieldResolver);
            // set post data
            var postRequest = new SolrQueryRequest
            {
                Query = translator.Translate(query.Query),
                Offset = query.Offset,
                Limit = query.Limit,
                Filters = query.Filters.Select(translator.Translate)
            };
            // set get data
            var url = new UriBuilder(_configuration.QueryUrl);
            var getRequest = url.Uri.ParseQueryString();
            getRequest.Add("defType", query.QueryType);
            if (query.QueryFields.Any())
            {
                getRequest.Add("qf", string.Join(" ", query.QueryFields.Select(translator.Translate)));
            }
            url.Query = getRequest.ToString();
            // post
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new SolrDateTimeConverter(),
                    new SolrJsonConverter<TResult>(_fieldResolver)
                }
            };
            return await PostAsJsonAsync<SolrQueryRequest, SolrQueryResponse<TResult>>(url.ToString(), postRequest, settings);
        }

        public async Task Remove(object id, bool commit = true)
        {
            var request = new SolrUpdateRequest
            {
                Remove = new SolrDeleteRequest(id),
                Commit = commit ? new object() : null
            };
            await PostAsJsonAsync<SolrUpdateRequest, SolrResponse>(_configuration.UpdateUrl, request);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
