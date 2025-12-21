namespace Mankura.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Manga> NewItems { get; set; } = new();
        public Dictionary<string, List<Manga>> MangasByGenre { get; set; } = new();
    }
}