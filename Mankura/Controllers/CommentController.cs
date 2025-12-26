using System.Security.Claims;
using Mankura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CommentController : Controller
{
    private readonly CommentRepository _comments;
    private readonly UserRepository _users;

    public CommentController(
        CommentRepository comments,
        UserRepository users)
    {
        _comments = comments;
        _users = users;
    }

    [HttpPost]
    public IActionResult Add(int mangaId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return RedirectToAction("Details", "Manga", new { id = mangaId });

        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        _comments.Add(mangaId, userId, text);

        return RedirectToAction("Details", "Manga", new { id = mangaId });
    }

    [Authorize]
    [HttpPost]
    public IActionResult Delete([FromBody] CommentDeleteRequest req)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        _comments.Delete(req.Id, userId);

        return Ok();
    }

    public class CommentDeleteRequest
    {
        public int Id { get; set; }
        public int MangaId { get; set; }
    }
}
