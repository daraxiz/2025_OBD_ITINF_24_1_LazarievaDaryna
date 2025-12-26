using Microsoft.Data.SqlClient;
using Mankura.Models;

namespace Mankura.Data
{
    public class ReadingProcessRepository
    {
        private readonly string _cs;

        public ReadingProcessRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")!;
        }

        public bool Exists(int userId, int mangaId, string type)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT 1
                FROM ReadingProcess
                WHERE ID_User = @u AND ID_Manga = @m
            ", con);

            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mangaId);

            return cmd.ExecuteScalar() != null;
        }

        public void AddOrUpdate(int userId, int mangaId, int chapterId, string type)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
        IF EXISTS (
            SELECT 1 FROM ReadingProcess
            WHERE ID_User = @u AND ID_Manga = @m
        )
        BEGIN
            UPDATE ReadingProcess
            SET Type = @t,
                ID_Chapter = @ch,
                AddedDate = GETDATE()
            WHERE ID_User = @u AND ID_Manga = @m
        END
        ELSE
        BEGIN
            INSERT INTO ReadingProcess
            (ID_User, Type, AddedDate, ID_Chapter, ID_Manga)
            VALUES (@u, @t, GETDATE(), @ch, @m)
        END
    ", con);

            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mangaId);
            cmd.Parameters.AddWithValue("@ch", chapterId);
            cmd.Parameters.AddWithValue("@t", type);

            cmd.ExecuteNonQuery();
        }

        public void Remove(int userId, int mangaId, string type)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            using var cmd = new SqlCommand(@"
                DELETE FROM ReadingProcess
                WHERE ID_User = @u AND ID_Manga = @m AND Type = @t
            ", con);

            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mangaId);
            cmd.Parameters.AddWithValue("@t", type);

            cmd.ExecuteNonQuery();
        }

        public List<Manga> GetByUser(int userId, string type)
        {
            var list = new List<Manga>();

            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT
                    m.ID_Manga,
                    m.NameManga,
                    m.Cover,
                    m.Description
                FROM ReadingProcess rp
                JOIN Manga m ON m.ID_Manga = rp.ID_Manga
                WHERE rp.ID_User = @u AND rp.Type = @t
                ORDER BY rp.AddedDate DESC
            ", con);

            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@t", type);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Manga
                {
                    Id = (int)r["ID_Manga"],
                    Name = r["NameManga"].ToString()!,
                    Cover = r["Cover"]?.ToString(),
                    Description = r["Description"]?.ToString()
                });
            }

            return list;
        }

        public string? GetUserStatus(int userId, int mangaId)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT Type
                FROM ReadingProcess
                WHERE ID_User = @u AND ID_Manga = @m
            ", con);

            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mangaId);

            return cmd.ExecuteScalar() as string;
        }

        public void Remove(int userId, int mangaId)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();

            var sql = @"
                DELETE FROM ReadingProcess
                WHERE ID_User = @u AND ID_Manga = @m
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mangaId);

            cmd.ExecuteNonQuery();
        }

    }
}
