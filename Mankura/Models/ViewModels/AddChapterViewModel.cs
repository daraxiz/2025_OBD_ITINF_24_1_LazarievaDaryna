using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AddChapterViewModel
{
    public int MangaId { get; set; }
    public int ChapterNumber { get; set; }
    public int? VolumeNumber { get; set; }

    public List<IFormFile> Pages { get; set; } = new();

    public List<SelectListItem> Mangas { get; set; } = new();
}
