using Microsoft.AspNetCore.Mvc;
using Mankura.Data;
using Mankura.Models.ViewModels;

namespace Mankura.Controllers
{
    public class CatalogController : Controller
    {
        private readonly MangaRepository _repo;

        public CatalogController(MangaRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index(
            string? search,
            int[]? genreIds,
            int[]? typeIds,
            int[]? statusIds,
            int? releaseYear)
        {
            var vm = new CatalogViewModel
            {
                Search = search,
                Mangas = _repo.GetFiltered(search, genreIds, typeIds, statusIds, releaseYear),

                Genres = _repo.GetGenres(),
                Types = _repo.GetTypes(),
                Statuses = _repo.GetStatuses(),
                ReleaseYears = _repo.GetReleaseYears(),

                GenreIds = genreIds,
                TypeIds = typeIds,
                StatusIds = statusIds,
                ReleaseYear = releaseYear
            };

            return View(vm);
        }
    }
}
