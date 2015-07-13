using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Solr.Net.WebService
{
    public class SolrClient
    {
        public String UpdateUrl { get; set; }
        public String QueryUrl { get; set; }

        public SolrClient(string coreAddress)
        {
            var baseAddress = coreAddress.TrimEnd('/');
            UpdateUrl = string.Format("{0}/update", baseAddress);
            QueryUrl = string.Format("{0}/query", baseAddress);
        }

        public async void Add(object document)
        {
            var request = new SolrAddRequest(document);
            await PostAsJsonAsync<SolrAddRequest, SolrResponse>(UpdateUrl, request);
        }

        private async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest request)
            where TResponse : SolrResponse
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                var responseString = response.Content.ReadAsStringAsync();
                throw new HttpRequestException(string.Format("Request failed with statuscode {0} {1}: {2}",
                    (int) response.StatusCode, response.ReasonPhrase, responseString));
            }
            var responseObject = await response.Content.ReadAsAsync<TResponse>();
            if (!responseObject.IsSuccessStatusCode)
            {
                throw new HttpRequestException(responseObject.Error == null
                    ? string.Format("Solr returned status {0} without further details.",
                        responseObject.ResponseHeader.Status)
                    : responseObject.Error.GetDescription());
            }
            return responseObject;
        }

        public SolrQueryResponse<TDocument> Get<TDocument>(SolrRequest query)
        {
            var queryString = JsonConvert.SerializeObject(query);
            Console.WriteLine(queryString);
            return new SolrQueryResponse<TDocument> {Documents = new TDocument[10]};
        }
    }
}