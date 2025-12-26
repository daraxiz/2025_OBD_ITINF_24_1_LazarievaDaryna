using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mankura.Data;

[Authorize]
public class ReadingProcessController : Controller
{
    private readonly ReaderRepository _reader;
    private readonly ReadingProcessRepository _rp;

    public ReadingProcessController(ReaderRepository reader, ReadingProcessRepository rp)
    {
        _reader = reader;
        _rp = rp;
    }

    [HttpPost]
    [Authorize]
    public IActionResult Add([FromBody] AddReadingProcessDto dto)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        int? firstChapterId = _reader.GetFirstChapterId(dto.MangaId);
        if (firstChapterId == null)
            return BadRequest("No chapters");

        _rp.AddOrUpdate(
            userId,
            dto.MangaId,
            firstChapterId.Value,
            dto.Type
        );

        return Ok();
    }

    [Authorize]
    [HttpPost]
    public IActionResult Remove([FromBody] int mangaId)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        _rp.Remove(userId, mangaId);

        return Ok();
    }


    public class AddReadingProcessDto
    {
        public int MangaId { get; set; }
        public string Type { get; set; } = null!;
    }
}

