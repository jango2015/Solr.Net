using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solr.Net.Serialization;

namespace Solr.Net.WebService
{
    public class SolrClient
    {
        public String UpdateUrl { get; set; }
        public String QueryUrl { get; set; }
        public IFieldResolver FieldResolver { get; set; }

        public SolrClient(string coreAddress)
        {
            var baseAddress = coreAddress.TrimEnd('/');
            UpdateUrl = string.Format("{0}/update", baseAddress);
            QueryUrl = string.Format("{0}/query", baseAddress);
            FieldResolver = new DefaultFieldResolver();
        }

        public async Task Add<TDocument>(TDocument document)
        {
            var request = new SolrUpdateRequest { Add = new SolrAddRequest(document) };
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new SolrJsonConverter<TDocument>(FieldResolver)}
            };
            await PostAsJsonAsync<SolrUpdateRequest, SolrResponse>(UpdateUrl, request, settings);
        }

        private static async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest request, JsonSerializerSettings settings = null)
            where TResponse : SolrResponse
        {
            var client = new HttpClient();
            var requestString = JsonConvert.SerializeObject(request, settings);
            Console.WriteLine("< {0}", requestString);
            var content = new StringContent(requestString);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("> {0}", responseString);
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

        public async Task<SolrQueryResponse<TDocument>> Get<TDocument>(SolrQuery<TDocument> query) where TDocument : new()
        {
            var translator = new SolrExpressionTranslator(FieldResolver);
            var request = new SolrQueryRequest
            {
                Query = query.Query,
                Offset = query.Offset,
                Limit = query.Limit,
                Filters = query.Filters.Select(translator.Translate)
            };
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new SolrJsonConverter<TDocument>(FieldResolver)}
            };
            return await PostAsJsonAsync<SolrQueryRequest, SolrQueryResponse<TDocument>>(QueryUrl, request, settings);
        }
    }
}