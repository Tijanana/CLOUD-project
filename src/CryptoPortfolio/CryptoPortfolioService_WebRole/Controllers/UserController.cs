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
        // GET: User
        public ActionResult Index()
        {
            return View(_userRepository.RetrieveAllUsers()); // ne treba nam ova metoda (test da li radi)
        }

        public ActionResult Login()
        {
            return View("Login");
        }        

        public ActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public ActionResult AddEntity(string Name, string Surname, 
                                      string Address, string City, string Country,
                                      string Phone, string Email, string Password,
                                      HttpPostedFileBase file)
        {
            try
            {
                if (_userRepository.Exists(Email))
                {
                    return View("Error");
                }

                // blob, queue i sta jos treba
                User user = new User() 
                {
                    Name = Name,
                    Surname = Surname,
                    Address = Address,
                    City = City,
                    Country = Country,
                    Phone = Phone,
                    Email = Email,
                    Password = Password,
                    PhotoUrl = "BlobUrl"                    
                };

                _userRepository.AddUsear(user);
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Register");
            }
        }
    }
}