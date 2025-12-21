using System.Security.Claims;
using Mankura.Data;
using Mankura.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class UserController : Controller
{
    private readonly MangaRepository _mangaRepo;
    private readonly UserRepository _users;

    public UserController(
        MangaRepository mangaRepo,
        UserRepository users
    )
    {
        _mangaRepo = mangaRepo;
        _users = users;
    }

    [HttpGet]
    public IActionResult Profile()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = _users.GetByEmail(email!);

        var model = new ProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            AvatarUrl = string.IsNullOrEmpty(user.Avatar)
                ? "/img/Avatar.svg"
                : user.Avatar,
            ReadingList = _mangaRepo.GetUserReadingList(email!)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileSettingsViewModel model)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        string? avatarPath = null;

        if (model.Avatar != null)
        {
            var fileName =
                $"{Guid.NewGuid()}{Path.GetExtension(model.Avatar.FileName)}";

            var savePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "avatars",
                fileName
            );

            using var stream = new FileStream(savePath, FileMode.Create);
            await model.Avatar.CopyToAsync(stream);

            avatarPath = "/avatars/" + fileName;
        }

        _users.UpdateProfile(userId, model.UserName, avatarPath);

        var claims = new List<Claim>
{
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, model.UserName),
            new Claim(ClaimTypes.Email, User.FindFirstValue(ClaimTypes.Email)!),
            new Claim("Avatar", avatarPath ?? "/img/Avatar.svg")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return RedirectToAction("Profile");
    }
}
