using Mankura.Models;
using Microsoft.Data.SqlClient;

namespace Mankura.Data
{
    public class CommentRepository
    {
        private readonly string _cs;

        public CommentRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")!;
        }

        public List<Comment> GetByManga(int mangaId)
        {
            var list = new List<Comment>();

            using var con = new SqlConnection(_cs);
            con.Open();

            var sql = @"
            SELECT
                c.ID_Comment,
                c.ID_User,
                c.CommentText,
                c.CommentDate,
                u.UserName,
                u.Avatar
            FROM Comment c
            JOIN [User] u ON u.ID_User = c.ID_User
            WHERE c.ID_Manga = @mangaId
            ORDER BY c.CommentDate DESC;
            ";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@mangaId", mangaId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Comment
                {
                    Id = (int)r["ID_Comment"],
                    UserId = (int)r["ID_User"],
                    Text = r["CommentText"]?.ToString() ?? "",
                    CommentDate = (DateTime)r["CommentDate"],
                    UserName = r["UserName"]?.ToString() ?? "",
                    Avatar = r["Avatar"]?.ToString() ?? "",
                });
            }

            return list;
        }

        public void Add(int mangaId, int userId, string text)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var sql = @"
                INSERT INTO Comment (CommentText, ID_Manga, ID_User, CommentDate)
                VALUES (@text, @mangaId, @userId, GETDATE());
            ";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@text", text);
            cmd.Parameters.AddWithValue("@mangaId", mangaId);
            cmd.Parameters.AddWithValue("@userId", userId);

            cmd.ExecuteNonQuery();
        }

        public Comment? GetById(int commentId)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            var cmd = new SqlCommand(@"
                SELECT ID_Comment, ID_User
                FROM Comment
                WHERE ID_Comment = @id
            ", con);

            cmd.Parameters.AddWithValue("@id", commentId);

            using var r = cmd.ExecuteReader();
            if (!r.Read())
                return null;

            return new Comment
            {
                Id = (int)r["ID_Comment"],
                UserId = (int)r["ID_User"]
            };
        }

        public void Delete(int id, int userId)
        {
            using var con = new SqlConnection(_cs);
            con.Open();

            using var cmd = new SqlCommand(@"
                DELETE FROM Comment
                WHERE ID_Comment = @id AND ID_User = @user
            ", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@user", userId);

            cmd.ExecuteNonQuery();
        }

    }
}
