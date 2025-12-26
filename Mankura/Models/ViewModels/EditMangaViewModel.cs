using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mankura.Models.ViewModels
{
    public class EditMangaViewModel
    {
        public int Id { get; set; }

        public string PublisherName { get; set; }
        public string NameManga { get; set; }
        public string? Description { get; set; }

        public string? Cover { get; set; }
        public IFormFile? CoverFile { get; set; }

        public int StatusId { get; set; }
        public int TypeId { get; set; }

        public int ReleaseYear { get; set; }

        public List<int> SelectedGenres { get; set; } = new();

        public IEnumerable<SelectListItem>? Statuses { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
        public IEnumerable<SelectListItem>? Genres { get; set; }
    }
}