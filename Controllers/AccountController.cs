using IPOPulse.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPOPulse.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            


            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() { 
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            // Registration logic here...

            return RedirectToAction("Login");
        }
    }
}
