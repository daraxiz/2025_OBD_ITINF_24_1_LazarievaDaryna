using Microsoft.AspNetCore.Mvc;
using Mankura.Data;
using Mankura.Models.ViewModels;

public class HomeController : Controller
{
    private readonly MangaRepository _repo;

    public HomeController(MangaRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Index()
    {
        var model = new HomeViewModel
        {
            NewItems = _repo.GetNewestMangas(10),
            MangasByGenre = _repo.GetTopGenresWithMangas(3, 10)
        };

        return View(model);
    }

}