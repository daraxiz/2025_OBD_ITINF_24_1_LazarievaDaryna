namespace Mankura.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int MangaId { get; set; }   
        public int UserId { get; set; }   

        public string Text { get; set; } = "";
        public DateTime CommentDate { get; set; }
        public string UserName { get; set; } = "";

        public string? Avatar { get; set; }
    }
}