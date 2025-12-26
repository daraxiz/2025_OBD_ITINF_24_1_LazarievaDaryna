namespace Mankura.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public int MangaId { get; set; }

        public int ChapterNumber { get; set; }

        public int? VolumeNumber { get; set; }
    }
}
