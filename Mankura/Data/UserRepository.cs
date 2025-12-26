using Microsoft.Data.SqlClient;
using Mankura.Models;

namespace Mankura.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public bool Exists(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM [User] WHERE Email = @email",
                conn
            );

            cmd.Parameters.AddWithValue("@email", email);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public void Create(User user)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
               INSERT INTO [User]
                (Email, UserName, Password, RegistrationDate, ID_Role)
                VALUES
                (@email, @username, @password, @regDate, @roleId)
            ";
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@username", user.UserName);
            cmd.Parameters.AddWithValue("@password", user.PasswordHash);
            cmd.Parameters.AddWithValue("@regDate", user.RegistrationDate);
            cmd.Parameters.AddWithValue("@roleId", 2);

            cmd.ExecuteNonQuery();
        }

        public User? GetByEmail(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT
                    ID_User,
                    Email,
                    UserName,
                    Password,
                    RegistrationDate,
                    ID_Role,
                    Avatar
                FROM [User]
                WHERE Email = @email
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = (int)r["ID_User"],
                Email = (string)r["Email"],
                UserName = (string)r["UserName"],
                PasswordHash = (string)r["Password"],
                RegistrationDate = (DateTime)r["RegistrationDate"],
                RoleId = (int)r["ID_Role"],
                Avatar = r["Avatar"] as string
            };
        }

        public User GetByEmailOrUserName(string login)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT *
                FROM [User]
                WHERE Email = @login OR UserName = @login
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@login", login);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = (int)r["ID_User"],
                Email = (string)r["Email"],
                UserName = (string)r["UserName"],
                PasswordHash = (string)r["Password"],
                RoleId = (int)r["ID_Role"]
            };
        }

        public User GetByUserName(string username)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT * FROM [User] WHERE UserName = @username";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = (int)r["ID_User"],
                Email = (string)r["Email"],
                UserName = (string)r["UserName"],
                PasswordHash = (string)r["Password"],
                RoleId = (int)r["ID_Role"],
                Avatar = r["Avatar"] as string
            };
        }

        public void UpdateProfile(int userId, string userName, string? avatar)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                UPDATE [User]
                SET UserName = @userName,
                    Avatar = COALESCE(@avatar, Avatar)
                WHERE ID_User = @id
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@userName", userName);
            cmd.Parameters.AddWithValue("@avatar", (object?)avatar ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public User? GetById(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT
                    ID_User,
                    Email,
                    UserName,
                    Password,
                    RegistrationDate,
                    ID_Role,
                    Avatar
                FROM [User]
                WHERE ID_User = @id
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = (int)r["ID_User"],
                Email = (string)r["Email"],
                UserName = (string)r["UserName"],
                PasswordHash = (string)r["Password"],
                RegistrationDate = (DateTime)r["RegistrationDate"],
                RoleId = (int)r["ID_Role"],
                Avatar = r["Avatar"] as string
            };
        }

        public bool UserNameExists(string userName, int exceptUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT COUNT(*)
                FROM [User]
                WHERE UserName = @userName
                  AND ID_User <> @exceptUserId
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userName", userName);
            cmd.Parameters.AddWithValue("@exceptUserId", exceptUserId);

            return (int)cmd.ExecuteScalar() > 0;
        }

    }
}
