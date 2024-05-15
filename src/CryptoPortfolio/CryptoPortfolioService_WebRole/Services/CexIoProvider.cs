using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace CryptoPortfolioService_WebRole.Services
{
    public class CexIoProvider
    {
        private HttpClient Client { get; set; }

        public CexIoProvider()
        {
            Client = new HttpClient();
        }

        public async Task<object> MakeGetRequest()
        {
            string url = "https://cex.io/api/currency_limits";
            var result = await GetAsync(url);

            return result;
        }

        public async Task<object> GetAsync(string url)
        {
            HttpResponseMessage response = await Client.GetAsync(url);
            
            response.EnsureSuccessStatusCode();

            return await DeserializeObject(response);
        }

        private async Task<object> DeserializeObject(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject(responseBody);
            return result;
        }
    }
}