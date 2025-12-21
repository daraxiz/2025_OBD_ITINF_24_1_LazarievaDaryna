using Microsoft.AspNetCore.Mvc;
using Mankura.Data;

public class ReaderController : Controller
{
    private readonly ReaderRepository _reader;

    public ReaderController(ReaderRepository reader)
    {
        _reader = reader;
    }

    public IActionResult Read(int chapterId)
    {
        var pages = _reader.GetPages(chapterId);

        if (!pages.Any())
            return NotFound();

        return View(pages);
    }
}