using Mankura.Data;
using Mankura.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Mankura.Controllers
{
    public class MangaController : Controller
    {
        private readonly MangaRepository _mangaRepo;
        private readonly ReaderRepository _reader;

        public MangaController(
            MangaRepository mangaRepo,
            ReaderRepository reader
        )
        {
            _mangaRepo = mangaRepo;
            _reader = reader;
        }

        public IActionResult Details(int id)
        {
            var manga = _mangaRepo.GetById(id);

            if (manga == null)
                return NotFound();

            return View(manga);
        }

        public IActionResult Read(int chapterId)
        {
            var pages = _reader.GetPages(chapterId);
            if (pages.Count == 0) return NotFound();

            var vm = new Mankura.Models.ViewModels.ChapterReadViewModel
            {
                ChapterId = chapterId,
                Pages = pages,
                PrevChapterId = _reader.GetPrevChapterId(chapterId),
                NextChapterId = _reader.GetNextChapterId(chapterId)
            };

            return View(vm);
        }
    }
}