namespace Mankura.Models
{
    public class ChapterPage
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public int PageNumber { get; set; }
        public string ImageURL { get; set; } = string.Empty;
    }
}