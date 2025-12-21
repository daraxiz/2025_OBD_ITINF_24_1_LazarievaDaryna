using Mankura.Models;

namespace Mankura.Models.ViewModels
{
    public class CatalogViewModel
    {
        public List<Manga> Mangas { get; set; } = new();

        public List<Genre> Genres { get; set; } = new();
        public List<MangaType> Types { get; set; } = new();
        public List<Status> Statuses { get; set; } = new();
        public List<int> ReleaseYears { get; set; } = new();

        public string? Search { get; set; }

        public int[]? GenreIds { get; set; }
        public int[]? TypeIds { get; set; }
        public int[]? StatusIds { get; set; }

        public int? ReleaseYear { get; set; }
    }
}