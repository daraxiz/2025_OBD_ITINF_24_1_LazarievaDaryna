using System.Security.Claims;
using Mankura.Data;
using Mankura.Models;
using Mankura.Models.ViewModels;
using Mankura.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Mankura.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _users;
        private readonly EmailService _emailService;

        public AccountController(
            UserRepository users,
            EmailService emailService)
        {
            _users = users;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_users.GetByEmail(model.Email) != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered"
                );
            }

            if (_users.GetByUserName(model.UserName) != null)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "This username is already taken"
                );
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                Email = model.Email,
                UserName = model.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RegistrationDate = DateTime.Now,
                RoleId = 2
            };

            _users.Create(user);

            try
            {
                _emailService.Send(
                    user.Email,
                    "Welcome to Mankura",
                    "You have successfully registered!"
                );
            }
            catch
            {
            }

            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _users.GetByEmailOrUserName(model.Login);

            if (user == null ||
                string.IsNullOrEmpty(user.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email/username or password"
                );
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim("Avatar", user.Avatar ?? "/img/Avatar.svg")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("Profile", "User");
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }
    }
}