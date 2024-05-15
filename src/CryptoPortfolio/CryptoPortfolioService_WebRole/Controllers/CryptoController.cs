using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using CryptoPortfolioService_WebRole.Models;
using CryptoPortfolioService_WebRole.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class CryptoController : Controller
    {
        private const string LoginViewPath = "~/Views/Authentication/Login.cshtml";
        UserRepository _userRepository = new UserRepository();
        TransactionRepository _transactionRepository = new TransactionRepository();
        CexIOProvider _cexIoProvider = new CexIOProvider();

        public async Task<ActionResult> CryptoCurrencies()
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            CurrencyLimitsResponse model = await GetCurrencyLimits();
            List<CurrencyPairListItem> viewModel = new List<CurrencyPairListItem>();

            model.Data.Pairs.ForEach(pairs =>
            {
                if (pairs.Symbol1 != "USD" && pairs.Symbol2 == "USD")
                    viewModel.Add(new CurrencyPairListItem(pairs.Symbol1, pairs.Symbol2, pairs.MinPrice, pairs.MaxPrice));
            });

            return await Task.Run(() => View(viewModel));
        }

        public ActionResult CurrencyDetails(string symbol1, string symbol2, string minPrice, string maxPrice)
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            CurrencyDetailsModel currencyDetails = new CurrencyDetailsModel(symbol1, symbol2, minPrice, maxPrice);
            return View(currencyDetails);
        }

        [HttpPost]
        public ActionResult AddEntity(string Symbol1, int Quantity, double Price)
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            Transaction transaction = new Transaction();
            transaction.CurrencyName = Symbol1;
            transaction.Quantity = Quantity;
            transaction.PricePerUnit = Price;
            transaction.UserId = user.RowKey;

            _transactionRepository.AddTransaction(transaction);

            return View("Error");
        }

        private User GetUserFromSession()
        {
            string userRowKey = (string)HttpContext.Session["userRowKey"];
            if (userRowKey == null)
                return null;

            return _userRepository.GetUser(userRowKey);
        }

        private async Task<CurrencyLimitsResponse> GetCurrencyLimits()
            => await _cexIoProvider.GetCurrencyLimits();
    }
}