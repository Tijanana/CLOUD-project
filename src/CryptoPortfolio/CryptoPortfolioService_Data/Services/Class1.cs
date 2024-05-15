using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CryptoPortfolioService_Data.Services
{
    public class Class1
    {
        private HttpClient Client { get; set; }

        /// <<summary>>
        /// Setup http client
        /// <</summary>>
        public Class1()
        {
            Client = new HttpClient();
        }

        /// Make a dummy request
        public async Task MakeGetRequest()
        {
            string url = "https://cex.io/api/ticker/{symbol1}/{symbol2}";
            await GetAsync(url);
        }

        /// Performs a GET Request
        public async Task GetAsync(string url)
        {
            //Start the request
            HttpResponseMessage response = await Client.GetAsync(url);

            //Validate result
            response.EnsureSuccessStatusCode();
        }
    }
}
