using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using CryptoPortfolioService_WebRole.Constants;
using CryptoPortfolioService_WebRole.Services;
using CryptoPortfolioService_WebRole.Utils;
using System;
using System.Web;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class UserController : Controller
    {
        ControllerHelperMethods _helpers = new ControllerHelperMethods();
        UserRepository _userRepository = new UserRepository();                
        //private const string LoginViewPath = "~/Views/Authentication/Login.cshtml";

        public ActionResult Index()
        {
            return View(_userRepository.RetrieveAllUsers()); // ne treba nam ova metoda (test da li radi)
        }

        public ActionResult Profile()
        {
            User user = _helpers.GetUserFromSession();
            if (user is null)
                return View(PathConstants.LoginViewPath);

            return View(user);
        }

        public ActionResult EditProfile()
        {
            User user = _helpers.GetUserFromSession();
            if (user is null)
                return View(PathConstants.LoginViewPath);

            return View(user);
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
    }
}