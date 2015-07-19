using System;
using System.Collections.Generic;
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

        public async Task Commit()
        {
            var request = new SolrUpdateRequest {Commit = new object()};
            await PostAsync<SolrResponse>(_configuration.UpdateUrl, request);
        }

        public async Task Add<TDocument>(TDocument document, bool commit = true)
        {
            var request = new SolrUpdateRequest
            {
                Add = new SolrAddRequest(document),
                Commit = commit ? new object() : null
            };
            var settings = GetSettings();
            await PostAsync<SolrResponse>(_configuration.UpdateUrl, request, settings);
        }

        public async Task<SolrQueryResponse<TDocument>> Get<TDocument>(SolrQueryRequest request)
        {
            var settings = GetSettings();
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("json", JsonConvert.SerializeObject(request.Json, settings)),
                new KeyValuePair<string, string>("deftype", request.QueryType)
            });
            // post
            return await PostAsync<SolrQueryResponse<TDocument>>(_configuration.QueryUrl, content);
        }

        public async Task Remove(object id, bool commit = true)
        {
            var request = new SolrUpdateRequest
            {
                Remove = new SolrDeleteRequest(id),
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
            _logger.Debug(string.Format("Requesting '{0}' with post data: {1}", url, await content.ReadAsStringAsync()));
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
