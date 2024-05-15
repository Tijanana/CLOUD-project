using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class CryptoController : Controller
    {
        // GET: Crypto
        public ActionResult Index()
        {
            return View();
        }
    }
}