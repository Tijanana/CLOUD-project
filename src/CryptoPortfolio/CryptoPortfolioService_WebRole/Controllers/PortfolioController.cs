using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Entities.Enums;
using CryptoPortfolioService_Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class PortfolioController : Controller
    {
        private const string LoginViewPath = "~/Views/Authentication/Login.cshtml";
        UserRepository _userRepository = new UserRepository();
        TransactionRepository _transactionRepository = new TransactionRepository();

        [HttpPost]
        public ActionResult AddTransactionEntity(string Symbol1, int Quantity, double Price)
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            Transaction transaction = new Transaction();
            transaction.CurrencyName = Symbol1;
            transaction.Quantity = Quantity;
            transaction.PricePerUnit = Price;
            transaction.UserId = user.RowKey;
            transaction.TransactionType = TransactionType.BUYING.ToString();

            _transactionRepository.AddTransaction(transaction);

            return RedirectToAction("CryptoCurrencies", "Crypto");
        }

        public ActionResult Transactions()
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            List<Transaction> transactions = _transactionRepository.RetrieveAllTransactionsForUser(user.RowKey);
            return View(transactions);
        }

        public ActionResult DeleteTransaction(string id)
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            _transactionRepository.RemoveTransaction(id);

            return RedirectToAction("Transactions");
        }

        private User GetUserFromSession()
        {
            string userRowKey = (string)HttpContext.Session["userRowKey"];
            if (userRowKey == null)
                return null;

            return _userRepository.GetUser(userRowKey);
        }
    }
}