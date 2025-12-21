namespace Mankura.Models
{
    public class Manga
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Cover { get; set; }

        public int? ReleaseYear { get; set; }

        public string Status { get; set; }
        public string Type { get; set; }
        public string Publisher { get; set; }
        public List<string> Genres { get; set; } = new();
    }
}