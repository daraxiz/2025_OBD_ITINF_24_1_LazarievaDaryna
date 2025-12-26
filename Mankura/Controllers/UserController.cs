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
    private readonly ReadingProcessRepository _rp;

    public UserController(
    MangaRepository mangaRepo,
    UserRepository users,
    ReadingProcessRepository rp
)
    {
        _mangaRepo = mangaRepo;
        _users = users;
        _rp = rp;
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

    private ProfileViewModel BuildProfileViewModel(int userId)
    {
        var user = _users.GetById(userId);

        return new ProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            AvatarUrl = string.IsNullOrEmpty(user.Avatar)
                ? "/img/Avatar.svg"
                : user.Avatar,
            ReadingList = _mangaRepo.GetUserReadingList(user.Email)
        };
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileSettingsViewModel model)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        if (!ModelState.IsValid)
            return View("Profile", BuildProfileViewModel(userId));

        var currentUser = _users.GetById(userId);

        if (currentUser.UserName != model.UserName &&
            _users.UserNameExists(model.UserName, userId))
        {
            ModelState.AddModelError(
                nameof(model.UserName),
                "Username is already taken"
            );

            return View("Profile", BuildProfileViewModel(userId));
        }

        string? avatar = null;

        if (model.Avatar != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Avatar.FileName)}";
            var savePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "avatars",
                fileName
            );

            using var stream = new FileStream(savePath, FileMode.Create);
            await model.Avatar.CopyToAsync(stream);

            avatar = "/avatars/" + fileName;
        }

        _users.UpdateProfile(userId, model.UserName, avatar);

        return RedirectToAction("Profile");
    }

    [Authorize]
    public IActionResult Reading(string type = "Will be read")
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var mangas = _rp.GetByUser(userId, type);

        return View(mangas);
    }


}