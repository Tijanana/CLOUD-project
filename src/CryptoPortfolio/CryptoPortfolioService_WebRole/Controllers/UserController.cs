using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using CryptoPortfolioService_WebRole.Models;
using CryptoPortfolioService_WebRole.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class UserController : Controller
    {
        UserRepository _userRepository = new UserRepository();
        CexIOProvider _cexIoProvider = new CexIOProvider();
        private const string LoginViewPath = "~/Views/Authentication/Login.cshtml";

        public ActionResult Index()
        {
            return View(_userRepository.RetrieveAllUsers()); // ne treba nam ova metoda (test da li radi)
        }

        public ActionResult Profile()
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            return View(user);
        }

        public ActionResult EditProfile()
        {
            User user = GetUserFromSession();
            if (user is null)
                return View(LoginViewPath);

            return View(user);
        }

        public ActionResult Portfolio()
        {
            return View();
        }

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

        [HttpPost]
        public ActionResult ModifyEntity(string RowKey, string Name, string Surname,
                                         string Address, string City, string Country,
                                         string Phone, string Email, string Password,
                                         HttpPostedFileBase file)
        {
            try
            {
                if (!_userRepository.Exists(RowKey))
                {
                    return View("Error");
                }

                User user = _userRepository.GetUser(RowKey);

                user.Name = Name;
                user.Surname = Surname;
                user.Address = Address;
                user.City = City;
                user.Country = Country;
                user.Phone = Phone;
                user.Email = Email;
                user.Password = Password;
                user.PhotoUrl = "hardcoded";

                _userRepository.UpdateUser(user);

                return View("Profile", user);
            }
            catch (Exception e)
            {
                string exceptionMessage = e.Message;
                return View("Error");
            }            
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