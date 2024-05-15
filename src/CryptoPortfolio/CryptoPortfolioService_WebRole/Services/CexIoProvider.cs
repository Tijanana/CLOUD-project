using CryptoPortfolioService_WebRole.Constants;
using CryptoPortfolioService_WebRole.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace CryptoPortfolioService_WebRole.Services
{
    public class CexIOProvider
    {
        private HttpClient Client { get; set; }

        public CexIOProvider()
        {
            Client = new HttpClient();
        }

        public async Task<CurrencyLimitsResponse> GetCurrencyLimits()
        {
            HttpResponseMessage response = await Client.GetAsync(CexIoUrlConstants.GetLimitsUrl);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CurrencyLimitsResponse>(responseBody);
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