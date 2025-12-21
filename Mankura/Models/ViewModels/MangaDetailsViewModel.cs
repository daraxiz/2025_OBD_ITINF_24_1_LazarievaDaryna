namespace Mankura.Models.ViewModels
{
    public class MangaDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cover { get; set; }
        public string Description { get; set; }

        public int ReleaseYear { get; set; }

        public string Status { get; set; }
        public string Type { get; set; }
        public string Publisher { get; set; }
    }
}