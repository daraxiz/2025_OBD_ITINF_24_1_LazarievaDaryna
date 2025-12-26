using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AddMangaViewModel
{
    public string PublisherName { get; set; } = ""; 
    public string NameManga { get; set; } = "";
    public string Description { get; set; } = "";

    public IFormFile? CoverFile { get; set; }

    public int StatusId { get; set; }
    public int TypeId { get; set; }
    public int ReleaseYear { get; set; }

    public string Authors { get; set; } = "";
    public string Artists { get; set; } = "";

    public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Types { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Genres { get; set; } = Enumerable.Empty<SelectListItem>();

    public List<int> GenreIds { get; set; } = new();
}

