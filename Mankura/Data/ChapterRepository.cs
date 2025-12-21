using Microsoft.Data.SqlClient;
using Mankura.Models;

namespace Mankura.Data
{
    public class ChapterRepository
    {
        private readonly string _connectionString;

        public ChapterRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public List<ChapterPage> GetPages(int chapterId)
        {
            var pages = new List<ChapterPage>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT 
                    ID_Page,
                    ID_Chapter,
                    PageNumber,
                    ImageURL
                FROM ChapterPage
                WHERE ID_Chapter = @chapterId
                ORDER BY PageNumber
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@chapterId", chapterId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pages.Add(new ChapterPage
                {
                    Id = reader.GetInt32(0),
                    ChapterId = reader.GetInt32(1),
                    PageNumber = reader.GetInt32(2),
                    ImageURL = reader.GetString(3)
                });
            }

            return pages;
        }
    }
}
