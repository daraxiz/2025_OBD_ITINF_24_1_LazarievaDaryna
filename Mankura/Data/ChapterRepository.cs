using Microsoft.Data.SqlClient;

namespace Mankura.Data
{
    public class ChapterRepository
    {
        private readonly string _cs;

        public ChapterRepository(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("DefaultConnection")!;
        }

        public int CreateChapter(int mangaId, int number)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
                IF NOT EXISTS (
                    SELECT 1 FROM Chapter 
                    WHERE ID_Manga = @m AND ChapterNumber = @n
                )
                INSERT INTO Chapter (ID_Manga, ChapterNumber)
                OUTPUT INSERTED.ID_Chapter
                VALUES (@m,@n)
                ELSE
                SELECT ID_Chapter 
                FROM Chapter 
                WHERE ID_Manga = @m AND ChapterNumber = @n",
                con);

            cmd.Parameters.AddWithValue("@m", mangaId);
            cmd.Parameters.AddWithValue("@n", number);

            return (int)cmd.ExecuteScalar();
        }

        public void AddPage(int chapterId, int page, string path)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
                INSERT INTO ChapterPage (ID_Chapter, PageNumber, ImageURL)
                VALUES (@c,@p,@i)",
                con);

            cmd.Parameters.AddWithValue("@c", chapterId);
            cmd.Parameters.AddWithValue("@p", page);
            cmd.Parameters.AddWithValue("@i", path);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int chapterId, int mangaId)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
                DELETE FROM ReadingProcess WHERE ID_Chapter = @c;
                DELETE FROM ChapterPage   WHERE ID_Chapter = @c;
                DELETE FROM Chapter       WHERE ID_Chapter = @c AND ID_Manga = @m;
            ", con);

            cmd.Parameters.AddWithValue("@c", chapterId);
            cmd.Parameters.AddWithValue("@m", mangaId);

            cmd.ExecuteNonQuery();
        }
    }
}

