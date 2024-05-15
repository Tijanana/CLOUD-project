using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class UserController : Controller
    {
        UserRepository _userRepository = new UserRepository();
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

        [HttpPost]
        public ActionResult ModifyEntity(string Id, string Name, string Surname,
                                         string Address, string City, string Country,
                                         string Phone, string Email, string Password,
                                         HttpPostedFileBase file)
        {
            User user = _userRepository.GetUser(Id);

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

        private User GetUserFromSession()        
            => (User)HttpContext.Session["user"];            
    }
}