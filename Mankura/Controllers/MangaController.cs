using System.Security.Claims;
using Mankura.Data;
using Mankura.Models;
using Mankura.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Mankura.Controllers
{
    public class MangaController : Controller
    {
        private readonly MangaRepository _mangaRepo;
        private readonly ReaderRepository _reader;
        private readonly CommentRepository _commentRepo;
        private readonly ReadingProcessRepository _rp;

        public MangaController(
            MangaRepository mangaRepo,
            ReaderRepository reader,
            CommentRepository commentRepo,
            ReadingProcessRepository rp
        )
        {
            _mangaRepo = mangaRepo;
            _reader = reader;
            _commentRepo = commentRepo;
            _rp = rp;
        }

        public IActionResult Details(int id)
        {
            var manga = _mangaRepo.GetById(id);
            if (manga == null)
                return NotFound();
            var chapters = _reader.GetChaptersByManga(id);
            ViewBag.Chapters = chapters;

            ViewBag.Comments = _commentRepo.GetByManga(id);

            if (User.Identity!.IsAuthenticated)
            {
                int userId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!
                );

                ViewBag.ReadingStatus =
                    _rp.GetUserStatus(userId, id);
            }

            ViewBag.FirstChapterId = _reader.GetFirstChapterId(id);

            return View(manga);
        }

        public IActionResult Read(int mangaId)
        {
            var firstChapterId = _reader.GetFirstChapterId(mangaId);

            if (firstChapterId == null)
                return NotFound("No chapters found");

            return RedirectToAction(
                nameof(ReadChapter),
                new { chapterId = firstChapterId }
            );
        }

        public IActionResult ReadChapter(int chapterId)
        {
            var pages = _reader.GetPages(chapterId);

            if (!pages.Any())
                return NotFound("No pages in this chapter");

            var vm = new ChapterReadViewModel
            {
                Pages = pages,
                PrevChapterId = _reader.GetPrevChapterId(chapterId),
                NextChapterId = _reader.GetNextChapterId(chapterId)
            };

            return View("Read", vm);
        }

        [HttpGet]
        public IActionResult Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Index");

            var results = _mangaRepo.SearchByName(q);
            return View("SearchResults", results);
        }



    }
}