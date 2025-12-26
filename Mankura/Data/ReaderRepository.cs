using Microsoft.Data.SqlClient;
using Mankura.Models;

namespace Mankura.Data
{
    public class ReaderRepository
    {
        private readonly string _cs;

        public ReaderRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")!;
        }

        public List<ChapterPage> GetPages(int chapterId)
        {
            var list = new List<ChapterPage>();

            using var conn = new SqlConnection(_cs);
            conn.Open();

            var sql = @"
                SELECT ID_Page, ID_Chapter, PageNumber, ImageURL
                FROM ChapterPage
                WHERE ID_Chapter = @chapterId
                ORDER BY PageNumber
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@chapterId", chapterId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ChapterPage
                {
                    Id = r.GetInt32(0),
                    ChapterId = r.GetInt32(1),
                    PageNumber = r.GetInt32(2),
                    ImageURL = r.GetString(3)
                });
            }

            return list;
        }

        public int GetOrCreateChapter(int mangaId, int chapterNumber)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();

            var checkSql = @"
                SELECT ID_Chapter
                FROM Chapter
                WHERE ID_Manga = @mangaId AND ChapterNumber = @num
            ";

            using var checkCmd = new SqlCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@mangaId", mangaId);
            checkCmd.Parameters.AddWithValue("@num", chapterNumber);

            var existing = checkCmd.ExecuteScalar();
            if (existing != null)
                return (int)existing;

            var insertSql = @"
                INSERT INTO Chapter (ID_Manga, ChapterNumber)
                OUTPUT INSERTED.ID_Chapter
                VALUES (@mangaId, @num)
            ";

            using var insertCmd = new SqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@mangaId", mangaId);
            insertCmd.Parameters.AddWithValue("@num", chapterNumber);

            return (int)insertCmd.ExecuteScalar();
        }

        public void AddPage(int chapterId, int mangaId, int pageNumber, string imagePath)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();

            var sql = @"
                INSERT INTO ChapterPage
                (ID_Chapter, ID_Manga, PageNumber, ImageURL)
                VALUES
                (@ch, @m, @p, @img)
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ch", chapterId);
            cmd.Parameters.AddWithValue("@m", mangaId);
            cmd.Parameters.AddWithValue("@p", pageNumber);
            cmd.Parameters.AddWithValue("@img", imagePath);

            cmd.ExecuteNonQuery();
        }

        public int? GetPrevChapterId(int chapterId)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();

            var sql = @"
                SELECT TOP 1 c2.ID_Chapter
                FROM Chapter c1
                JOIN Chapter c2 
                    ON c2.ID_Manga = c1.ID_Manga
                WHERE c1.ID_Chapter = @chapterId
                  AND c2.ChapterNumber < c1.ChapterNumber
                ORDER BY c2.ChapterNumber DESC
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@chapterId", chapterId);

            var res = cmd.ExecuteScalar();
            return res == null ? null : (int)res;
        }


        public int? GetNextChapterId(int chapterId)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();

            var sql = @"
                SELECT TOP 1 c2.ID_Chapter
                FROM Chapter c1
                JOIN Chapter c2 
                    ON c2.ID_Manga = c1.ID_Manga
                WHERE c1.ID_Chapter = @chapterId
                  AND c2.ChapterNumber > c1.ChapterNumber
                ORDER BY c2.ChapterNumber ASC
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@chapterId", chapterId);

            var res = cmd.ExecuteScalar();
            return res == null ? null : (int)res;
        }

        public int? GetFirstChapterId(int mangaId)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();

            var sql = @"
                SELECT TOP 1 ID_Chapter
                FROM Chapter
                WHERE ID_Manga = @mangaId
                ORDER BY ChapterNumber ASC
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mangaId", mangaId);

            var res = cmd.ExecuteScalar();
            return res == null ? null : (int)res;
        }

        public List<Chapter> GetChaptersByManga(int mangaId)
        {
            var list = new List<Chapter>();

            using var con = new SqlConnection(_cs);
            con.Open();

            using var cmd = new SqlCommand(@"
        SELECT ID_Chapter, ChapterNumber, VolumeNumber
        FROM Chapter
        WHERE ID_Manga = @m
        ORDER BY ChapterNumber
    ", con);

            cmd.Parameters.AddWithValue("@m", mangaId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Chapter
                {
                    Id = (int)r["ID_Chapter"],
                    ChapterNumber = (int)r["ChapterNumber"],
                    VolumeNumber = r["VolumeNumber"] as int?
                });
            }

            return list;
        }
    }
}
