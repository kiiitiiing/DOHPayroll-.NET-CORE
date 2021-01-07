using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DOHPayroll.Models.PostModels;
using DOHPayroll.ViewModels;
using DOHPayroll.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace DOHPayroll.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }
        #region LOGIN
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View(new LoginModel
                {
                    ReturnUrl = returnUrl
                });
            }
            else
            {
                return RedirectToAction("JobOrder", "Home", new { index = 0 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if(ModelState.IsValid)
            {
                var (isValid, user) = await _userService.ValidateUserCredentialsAsync(model.Username, model.Password);
                if(isValid)
                {
                    if(BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                    {
                        await LoginAsync(user, model.RememberMe);
                        return RedirectToAction("JobOrder", "Employees", new { index = 0 });
                    }
                }
                if (user == null)
                {
                    ModelState.AddModelError("Username", "User does not exists");
                    ViewBag.Username = "invalid";
                }
                else
                {
                    ModelState.AddModelError("Username", "Wrong Password");
                    ViewBag.Password = "invalid";
                }
            }
            ViewBag.Result = false;
            return View(model);
        }
        #endregion

        #region LOGOUT
        public async Task<IActionResult> Logout(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return await Logout();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region NOT FOUND
        public IActionResult NotFound()
        {
            return View();
        }
        #endregion

        #region HELPERS
        private async Task LoginAsync(CookiesModel user, bool rememberMe)
        {
            var properties = new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = rememberMe
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.Fname),
                new Claim(ClaimTypes.Surname, user.Lname),
                new Claim(ClaimTypes.Role, user.Username.Equals("hr_admin")?"admin":"user"),
                new Claim("Position", user.Position),
                new Claim("Designation", user.Designation),
                new Claim("Division", user.Division),
                new Claim("Section", user.Section),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal, properties);
        }
        #endregion
    }
}
