using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using System.Web;
using System.Web.Mvc;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class AuthenticationController : Controller
    {
        UserRepository _userRepository = new UserRepository();
        //// GET: Authentication
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
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

                // TODO: blob, queue i sta jos treba
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
                return RedirectToAction("Login");
            }
            catch
            {
                return View("Register");
            }
        }

        [HttpPost]
        public ActionResult SetSession(string Email, string Password)
        {
            User user = _userRepository.GetUserByCredentials(Email, Password);
            if (user is null)
                return View("Login");

            Session["user"] = user;
            return RedirectToAction("Profile", "User");
        }
    }
}