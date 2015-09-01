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
        private readonly HttpClient _httpClient;
        private readonly ILog _logger;

        public SolrClient(ISolrConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _logger = LogManager.GetLogger(GetType());
        }

        public async Task CommitAsync()
        {
            var request = new SolrUpdateRequest {Commit = new object()};
            await PostAsync<SolrResponse>(_configuration.UpdateUrl, request);
        }

        public async Task AddAsync<TDocument>(TDocument document, bool commit = true)
        {
            var request = new SolrUpdateRequest
            {
                Add = new SolrAddRequest(document),
                Commit = commit ? new object() : null
            };
            var settings = GetSettings();
            await PostAsync<SolrResponse>(_configuration.UpdateUrl, request, settings);
        }

        public async Task<SolrQueryResponse<TDocument>> GetAsync<TDocument>(SolrQueryRequest request)
        {
            var settings = GetSettings();
            var keyValuePairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("json", JsonConvert.SerializeObject(request.Json, settings)),
                new KeyValuePair<string, string>("deftype", request.QueryType)
            };
            if (request.QueryFields.Any())
            {
                keyValuePairs.Add(new KeyValuePair<string, string>("qf", string.Join(" ", request.QueryFields)));
            }
            if (!string.IsNullOrWhiteSpace(request.Sort))
            {
                keyValuePairs.Add(new KeyValuePair<string, string>("sort", request.Sort));
            }
            var content = new FormUrlEncodedContent(keyValuePairs);
            // post
            return await PostAsync<SolrQueryResponse<TDocument>>(_configuration.QueryUrl, content);
        }

        public async Task RemoveByIdAsync(object id, bool commit = true)
        {
            var request = new SolrUpdateRequest
            {
                Remove = new SolrDeleteIdRequest(id),
                Commit = commit ? new object() : null
            };
            await PostAsync<SolrResponse>(_configuration.UpdateUrl, request);
        }

        public async Task RemoveByQueryAsync(string query, bool commit = true)
        {
            var request = new SolrUpdateRequest
            {
                Remove = new SolrDeleteQueryRequest(query),
                Commit = commit ? new object() : null
            };
            await PostAsync<SolrResponse>(_configuration.UpdateUrl, request);
        }

        private async Task<TResponse> PostAsync<TResponse>(string url, object request,
            JsonSerializerSettings settings = null)
            where TResponse : SolrResponse
        {
            var content = new StringContent(JsonConvert.SerializeObject(request, settings));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            return await PostAsync<TResponse>(url, content, settings);
        }

        private async Task<TResponse> PostAsync<TResponse>(string url, HttpContent content,
            JsonSerializerSettings settings = null)
            where TResponse : SolrResponse
        {
            var data = await content.ReadAsStringAsync();
            _logger.Debug(string.Format("Requesting '{0}' with post data: {1}", url, data));
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.Debug(string.Format("Received response: {0}", responseString));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(string.Format("Request failed with statuscode {0} {1}: {2}",
                    (int)response.StatusCode, response.ReasonPhrase, responseString));
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

        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new SolrDateTimeConverter()},
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
