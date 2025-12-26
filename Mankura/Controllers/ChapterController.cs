using System.Security.Claims;
using Mankura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class ChapterController : Controller
{
    private readonly ChapterRepository _chapters;

    public ChapterController(ChapterRepository chapters)
    {
        _chapters = chapters;
    }

    [HttpPost]
    public IActionResult Delete([FromBody] DeleteChapterRequest req)
    {
        _chapters.Delete(req.Id, req.MangaId);
        return Ok();
    }

    public class DeleteChapterRequest
    {
        public int Id { get; set; }
        public int MangaId { get; set; }
    }
}
