using CryptoPortfolioService_Data.Services;
using CryptoPortfolioService_WebRole.Models;
using CryptoPortfolioService_WebRole.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class HomeController : Controller
    {
        Class1 classTest = new Class1();
        public async Task<ActionResult> Index()
        {
            CexIOProvider apiIntegration = new CexIOProvider();
            var result = await apiIntegration.MakeGetRequest();

            //var x = Deserialize<CryptoCurrencyListItem>(result);
            //CryptoCurrencyListItem list = new CryptoCurrencyListItem();                        
            return await Task.Run(() => View());
        }

        private static T Deserialize<T>(string json)
        {
            Newtonsoft.Json.JsonSerializer s = new JsonSerializer();
            return s.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}