using System.Text.RegularExpressions;
using Mankura.Data;
using Mankura.Models;
using Mankura.Models.ViewModels;
using Mankura.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly MangaRepository _mangaRepository;
    private readonly Db _db;
    private readonly ChapterImportService _chapterImport;
    public AdminController(Db db, ChapterImportService chapterImport, MangaRepository mangaRepository)
    {
        _db = db;
        _chapterImport = chapterImport;
        _mangaRepository = mangaRepository;
        
    }

    public IActionResult Panel()
    {
        using var con = _db.GetConnection();
        con.Open();

        ViewBag.UsersCount = ExecScalarInt(con, "SELECT COUNT(*) FROM [User]");
        ViewBag.MangaCount = ExecScalarInt(con, "SELECT COUNT(*) FROM Manga");
        ViewBag.CommentsCount = ExecScalarInt(con, "SELECT COUNT(*) FROM Comment");

        ViewBag.Mangas = _mangaRepository.GetAll();

        return View();
    }

    public IActionResult Users()
    {
        using var con = _db.GetConnection();
        con.Open();

        var users = new List<Mankura.Models.ViewModels.AdminUserViewModel>();

        var cmd = new SqlCommand(@"
            SELECT u.ID_User, u.UserName, u.Email, u.ID_Role, r.NameRole
            FROM [User] u
            JOIN UserRole r ON r.ID_Role = u.ID_Role
            ORDER BY u.ID_User DESC", con);

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            users.Add(new Mankura.Models.ViewModels.AdminUserViewModel
            {
                Id = (int)rd["ID_User"],
                UserName = rd["UserName"].ToString()!,
                Email = rd["Email"].ToString()!,
                RoleId = (int)rd["ID_Role"],
                RoleName = rd["NameRole"].ToString()!
            });
        }

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteUser(int id)
    {
        using var con = _db.GetConnection();
        con.Open();

        var cmd = new SqlCommand(
            "DELETE FROM [User] WHERE ID_User = @id AND ID_Role != 1",
            con
        );

        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        return RedirectToAction(nameof(Users));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult AddManga()
    {
        var model = new AddMangaViewModel
        {
            ReleaseYear = DateTime.Now.Year,

            Statuses = _mangaRepository.GetStatuses()
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }),

            Types = _mangaRepository.GetTypes()
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }),

            Genres = _mangaRepository.GetGenres()
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
        };

        return View(model);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddManga(AddMangaViewModel model)
    {
        if (!ModelState.IsValid)
            return AddManga();

        if (_mangaRepository.ExistsByName(model.NameManga))
        {
            ModelState.AddModelError("NameManga",
                "Manga with this title already exists");
            return AddManga();
        }

        string? coverPath = null;

        if (model.CoverFile != null)
        {
            var dir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "img", "covers"
            );

            Directory.CreateDirectory(dir);

            var fileName = Guid.NewGuid() + Path.GetExtension(model.CoverFile.FileName);
            var savePath = Path.Combine(dir, fileName);

            using var fs = new FileStream(savePath, FileMode.Create);
            model.CoverFile.CopyTo(fs);

            coverPath = "/img/covers/" + fileName;
        }

        int publisherId = _mangaRepository.GetOrCreatePublisher(model.PublisherName);

        int mangaId = _mangaRepository.AddManga(
            publisherId,
            model.NameManga,
            model.Description,
            coverPath,
            model.StatusId,
            model.TypeId,
            model.ReleaseYear
        );

        if (!string.IsNullOrWhiteSpace(model.Authors))
        {
            var authors = model.Authors
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim());

            _mangaRepository.AddAuthorshipByNames(mangaId, authors, roleId: 1);
        }
        if (!string.IsNullOrWhiteSpace(model.Artists))
        {
            var artists = model.Artists
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim());

            _mangaRepository.AddAuthorshipByNames(mangaId, artists, roleId: 2);
        }

        _mangaRepository.AddGenres(mangaId, model.GenreIds);

        TempData["Success"] = "Manga successfully added";
        return RedirectToAction("Panel");
    }


    private int ExecScalarInt(SqlConnection con, string sql)
    {
        using var cmd = new SqlCommand(sql, con);
        return (int)cmd.ExecuteScalar();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleAdmin(int id)
    {
        using var con = _db.GetConnection();
        con.Open();

        var cmd = new SqlCommand(@"
        UPDATE [User]
        SET ID_Role = CASE 
            WHEN ID_Role = 1 THEN 2
            ELSE 1
        END
        WHERE ID_User = @id
    ", con);

        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public IActionResult AddChapter()
    {
        var mangas = _mangaRepository.GetAll()
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            })
            .ToList();

        return View(new AddChapterViewModel
        {
            Mangas = mangas
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddChapter(AddChapterViewModel model)
    {
        if (!ModelState.IsValid || model.Pages.Count == 0)
            return View(model);

        var manga = _mangaRepository.GetById(model.MangaId);
        if (manga == null)
            return NotFound();

        using var con = _db.GetConnection();
        con.Open();

        using (var check = new SqlCommand(@"
        SELECT ID_Chapter
        FROM Chapter
        WHERE ID_Manga = @manga AND ChapterNumber = @num
    ", con))
        {
            check.Parameters.AddWithValue("@manga", model.MangaId);
            check.Parameters.AddWithValue("@num", model.ChapterNumber);

            var exists = check.ExecuteScalar();
            if (exists != null)
            {
                ModelState.AddModelError("", "This chapter already exists.");

                model.Mangas = _mangaRepository.GetAll()
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Name
                    })
                    .ToList();

                return View(model);
            }
        }

        int chapterId;
        using (var cmd = new SqlCommand(@"
            INSERT INTO Chapter (ID_Manga, ChapterNumber, VolumeNumber)
            OUTPUT INSERTED.ID_Chapter
            VALUES (@manga, @chapter, @volume)
        ", con))
        {
            cmd.Parameters.AddWithValue("@manga", model.MangaId);
            cmd.Parameters.AddWithValue("@chapter", model.ChapterNumber);
            cmd.Parameters.AddWithValue("@volume", (object?)model.VolumeNumber ?? DBNull.Value);

            chapterId = (int)cmd.ExecuteScalar();
        }

        int page = 1;

        foreach (var file in model.Pages
    .OrderBy(f => GetPageNumberFromFileName(f.FileName)))
        {
            int pageNumber = GetPageNumberFromFileName(file.FileName);

            string fileName = $"{pageNumber:D3}{Path.GetExtension(file.FileName)}";
            string fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "manga",
                manga.Name,
                model.ChapterNumber.ToString(),
                fileName
            );

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using var stream = new FileStream(fullPath, FileMode.Create);
            file.CopyTo(stream);

            string imageUrl = $"/uploads/manga/{manga.Name}/{model.ChapterNumber}/{fileName}";

            using var pageCmd = new SqlCommand(@"
                INSERT INTO ChapterPage
                (ID_Chapter, ID_Manga, PageNumber, ImageURL)
                VALUES (@ch, @m, @p, @url)
            ", con);

            pageCmd.Parameters.AddWithValue("@ch", chapterId);
            pageCmd.Parameters.AddWithValue("@m", model.MangaId);
            pageCmd.Parameters.AddWithValue("@p", pageNumber);
            pageCmd.Parameters.AddWithValue("@url", imageUrl);

            pageCmd.ExecuteNonQuery();
        }


        return RedirectToAction("Panel");
    }

    private int GetPageNumberFromFileName(string fileName)
    {
        if (!fileName.Contains("("))
            return 1;

        var match = Regex.Match(fileName, @"\((\d+)\)");
        if (!match.Success)
            return 1;

        return int.Parse(match.Groups[1].Value) + 1;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteManga(int id)
    {
        using var con = _db.GetConnection();
        con.Open();

        using var tr = con.BeginTransaction();

        try
        {
            ExecNonQuery(con, tr, @"
            DELETE FROM ChapterPage
            WHERE ID_Manga = @id
        ", id);

            ExecNonQuery(con, tr, @"
            DELETE FROM Chapter
            WHERE ID_Manga = @id
        ", id);

            ExecNonQuery(con, tr, @"
            DELETE FROM ReadingProcess
            WHERE ID_Manga = @id
        ", id);

            ExecNonQuery(con, tr, @"
            DELETE FROM Comment
            WHERE ID_Manga = @id
        ", id);

            ExecNonQuery(con, tr, @"
            DELETE FROM MangaGenres
            WHERE ID_Manga = @id
        ", id);

            ExecNonQuery(con, tr, @"
            DELETE FROM Authorship
            WHERE ID_Manga = @id
        ", id);

            ExecNonQuery(con, tr, @"
            DELETE FROM Manga
            WHERE ID_Manga = @id
        ", id);

            tr.Commit();
        }
        catch
        {
            tr.Rollback();
            throw;
        }



        var manga = _mangaRepository.GetById(id);
        if (manga != null)
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "manga",
                manga.Name
            );

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        return RedirectToAction("Panel");
    }

    private void ExecNonQuery(
    SqlConnection con,
    SqlTransaction tr,
    string sql,
    int id
)
    {
        using var cmd = new SqlCommand(sql, con, tr);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult EditManga(int id)
    {
        var manga = _mangaRepository.GetById(id);
        if (manga == null)
            return NotFound();

        var statuses = _mangaRepository.GetStatuses();
        var types = _mangaRepository.GetTypes();
        var genres = _mangaRepository.GetGenres();

        var vm = new EditMangaViewModel
        {
            Id = manga.Id,
            PublisherName = manga.Publisher,
            NameManga = manga.Name,
            Description = manga.Description,
            Cover = manga.Cover,
            ReleaseYear = manga.ReleaseYear ?? DateTime.Now.Year,

            StatusId = statuses.First(s => s.Name == manga.Status).Id,
            TypeId = types.First(t => t.Name == manga.Type).Id,

            SelectedGenres = _mangaRepository.GetGenreIdsForManga(id),

            Statuses = statuses.Select(s => new SelectListItem(s.Name, s.Id.ToString())),
            Types = types.Select(t => new SelectListItem(t.Name, t.Id.ToString())),
            Genres = genres.Select(g => new SelectListItem(g.Name, g.Id.ToString()))
        };

        return View(vm);
    }



    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult EditManga(EditMangaViewModel m)
    {
        if (!ModelState.IsValid)
        {
            m.Statuses = _mangaRepository.GetStatuses()
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            m.Types = _mangaRepository.GetTypes()
                .Select(t => new SelectListItem(t.Name, t.Id.ToString()));
            m.Genres = _mangaRepository.GetGenres()
                .Select(g => new SelectListItem(g.Name, g.Id.ToString()));

            return View(m);
        }

        string? coverPath = m.Cover; 

        if (m.CoverFile != null && m.CoverFile.Length > 0)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "covers");
            Directory.CreateDirectory(dir);

            var fileName = Guid.NewGuid() + Path.GetExtension(m.CoverFile.FileName);
            var savePath = Path.Combine(dir, fileName);

            using var fs = new FileStream(savePath, FileMode.Create);
            m.CoverFile.CopyTo(fs);

            coverPath = "/img/covers/" + fileName;
        }

        var publisherId = _mangaRepository.GetOrCreatePublisher(m.PublisherName);

        _mangaRepository.Update(
            m.Id,
            m.NameManga,
            m.Description ?? "",
            m.ReleaseYear,
            m.StatusId,
            m.TypeId,
            coverPath
        );

        _mangaRepository.UpdateGenres(m.Id, m.SelectedGenres ?? new List<int>());

        TempData["Success"] = "Manga updated";
        return RedirectToAction("Details", "Manga", new { id = m.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult SelectMangaToEdit()
    {
        var mangas = _mangaRepository.GetAll();
        return View(mangas);
    }

}