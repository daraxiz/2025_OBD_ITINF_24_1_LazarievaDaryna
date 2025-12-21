using System.Data;
using Mankura.Models;
using Mankura.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace Mankura.Data
{
    public class MangaRepository
    {
        private readonly string _connectionString;

        public MangaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<Manga> GetAll()
        {
            var mangas = new List<Manga>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
            SELECT
                m.ID_Manga,
                m.NameManga,
                m.Description,
                m.Cover,
                m.ReleaseDate AS ReleaseYear,
                s.NameStatus,
                t.NameType,
                p.NamePublish
            FROM Manga m
            JOIN Status s ON s.ID_Status = m.ID_Status
            JOIN MangaType t ON t.ID_Type = m.ID_Type
            JOIN Publish p ON p.ID_Publish = m.ID_Publish
            ";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                mangas.Add(new Manga
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID_Manga")),
                    Name = reader.GetString(reader.GetOrdinal("NameManga")),
                    Description = reader["Description"] as string,
                    Cover = reader["Cover"] as string,

                    ReleaseYear = reader.GetInt16(reader.GetOrdinal("ReleaseYear")),
                    Status = reader.GetString(reader.GetOrdinal("NameStatus")),
                    Type = reader.GetString(reader.GetOrdinal("NameType")),
                    Publisher = reader.GetString(reader.GetOrdinal("NamePublish"))
                });
            }


            return mangas;
        }

        public Manga? GetById(int id)
        {
            Manga? manga = null;

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT
                    m.ID_Manga,
                    m.NameManga,
                    m.Description,
                    m.Cover,
                    m.ReleaseDate AS ReleaseYear,
                    s.NameStatus,
                    t.NameType,
                    p.NamePublish

                FROM Manga m
                JOIN Status s ON s.ID_Status = m.ID_Status
                JOIN MangaType t ON t.ID_Type = m.ID_Type
                JOIN Publish p ON p.ID_Publish = m.ID_Publish

                WHERE m.ID_Manga = @id;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                manga = new Manga
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID_Manga")),
                    Name = reader.GetString(reader.GetOrdinal("NameManga")),
                    Description = reader["Description"] as string,
                    Cover = reader["Cover"] as string,
                    ReleaseYear = reader.GetInt16(reader.GetOrdinal("ReleaseYear")),
                    Status = reader.GetString(reader.GetOrdinal("NameStatus")),
                    Type = reader.GetString(reader.GetOrdinal("NameType")),
                    Publisher = reader.GetString(reader.GetOrdinal("NamePublish"))
                };
            }
           
            reader.Close();

            if (manga != null)
            {
                manga.Genres = GetGenresForManga(id, conn);
            }

            return manga;
        }

        private List<string> GetGenresForManga(int mangaId, SqlConnection conn)
        {
            var genres = new List<string>();

            var sql = @"
                SELECT g.NameGenre
                FROM MangaGenres mg
                JOIN Genre g ON g.ID_Genre = mg.ID_Genre
                WHERE mg.ID_Manga = @id;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", mangaId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                genres.Add(reader.GetString(0));
            }

            return genres;
        }

        public List<Manga> GetFiltered(
    string? search,
    int[]? genreIds,
    int[]? typeIds,
    int[]? statusIds,
    int? releaseYear)
        {
            var mangas = new List<Manga>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT DISTINCT
                    m.ID_Manga,
                    m.NameManga,
                    m.Cover,
                    m.ReleaseDate AS ReleaseYear,
                    t.NameType,
                    s.NameStatus
                FROM Manga m
                JOIN MangaType t ON t.ID_Type = m.ID_Type
                JOIN Status s ON s.ID_Status = m.ID_Status
                LEFT JOIN MangaGenres mg ON mg.ID_Manga = m.ID_Manga
                WHERE 1 = 1
            ";

            if (!string.IsNullOrWhiteSpace(search))
                sql += " AND m.NameManga LIKE @search";

            if (genreIds?.Any() == true)
                sql += $" AND mg.ID_Genre IN ({string.Join(",", genreIds)})";

            if (typeIds?.Any() == true)
                sql += $" AND m.ID_Type IN ({string.Join(",", typeIds)})";

            if (statusIds?.Any() == true)
                sql += $" AND m.ID_Status IN ({string.Join(",", statusIds)})";

            if (releaseYear.HasValue)
                sql += " AND m.ReleaseDate = @ReleaseYear";

            using var cmd = new SqlCommand(sql, conn);

            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@search", "%" + search + "%");

            if (releaseYear.HasValue)
                cmd.Parameters.AddWithValue("@ReleaseYear", releaseYear);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                mangas.Add(new Manga
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Cover = reader["Cover"] as string,
                    ReleaseYear = reader["ReleaseYear"] == DBNull.Value
                        ? null
                        : Convert.ToInt16(reader["ReleaseYear"]),
                    Type = reader.GetString(4),
                    Status = reader.GetString(5)
                });
            }

            return mangas;
        }

        public List<Genre> GetGenres()
        {
            var list = new List<Genre>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT ID_Genre, NameGenre FROM Genre ORDER BY NameGenre";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Genre { Id = r.GetInt32(0), Name = r.GetString(1) });

            return list;
        }

        public List<MangaType> GetTypes()
        {
            var list = new List<MangaType>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT ID_Type, NameType FROM MangaType ORDER BY NameType";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new MangaType { Id = r.GetInt32(0), Name = r.GetString(1) });

            return list;
        }

        public List<Status> GetStatuses()
        {
            var list = new List<Status>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT ID_Status, NameStatus FROM Status ORDER BY NameStatus";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Status { Id = r.GetInt32(0), Name = r.GetString(1) });

            return list;
        }

        public List<int> GetReleaseYears()
        {
            var list = new List<int>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
            SELECT DISTINCT ReleaseDate AS Y
            FROM Manga
            WHERE ReleaseDate IS NOT NULL
            ORDER BY Y DESC";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(r.GetInt16(0));

            return list;
        }

        public List<Manga> GetNewestMangas(int limit = 10)
        {
            var list = new List<Manga>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT TOP (@limit)
                    m.ID_Manga,
                    m.NameManga,
                    m.Cover,
                    m.ReleaseDate AS ReleaseYear
                FROM Manga m
                WHERE m.ReleaseDate IS NOT NULL
                ORDER BY m.ReleaseDate DESC
    ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Manga
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Cover = reader["Cover"] as string,
                    ReleaseYear = reader.GetInt16(3)
                });
            }

            return list;
        }

        public Dictionary<string, List<Manga>> GetTopGenresWithMangas(int genreCount = 3, int mangaLimit = 10)
        {
            var result = new Dictionary<string, List<Manga>>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var topGenresSql = @"
                SELECT TOP (@genreCount)
                    g.ID_Genre,
                    g.NameGenre
                FROM Genre g
                JOIN MangaGenres mg ON mg.ID_Genre = g.ID_Genre
                GROUP BY g.ID_Genre, g.NameGenre
                ORDER BY COUNT(mg.ID_Manga) DESC
            ";

            var genres = new List<(int Id, string Name)>();

            using (var cmd = new SqlCommand(topGenresSql, conn))
            {
                cmd.Parameters.AddWithValue("@genreCount", genreCount);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    genres.Add((r.GetInt32(0), r.GetString(1)));
            }

            foreach (var g in genres)
            {
                var mangas = new List<Manga>();

                var mangaSql = @"
                    SELECT TOP (@limit)
                        m.ID_Manga,
                        m.NameManga,
                        m.Cover,
                        m.ReleaseDate AS ReleaseYear
                    FROM Manga m
                    JOIN MangaGenres mg ON mg.ID_Manga = m.ID_Manga
                    WHERE mg.ID_Genre = @genreId
                    ORDER BY m.ReleaseDate DESC
                ";

                using var cmd = new SqlCommand(mangaSql, conn);
                cmd.Parameters.AddWithValue("@limit", mangaLimit);
                cmd.Parameters.AddWithValue("@genreId", g.Id);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    mangas.Add(new Manga
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Cover = reader["Cover"] as string,
                        ReleaseYear = reader["ReleaseYear"] == DBNull.Value
                            ? null
                            : Convert.ToInt16(reader["ReleaseYear"])
                    });
                }

                if (mangas.Any())
                    result[g.Name] = mangas;
            }

            return result;
        }

        public List<Manga> GetUserReadingList(string email)
        {
            var mangas = new List<Manga>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT DISTINCT
                    m.ID_Manga,
                    m.NameManga,
                    m.Cover,
                    m.ReleaseDate AS ReleaseYear
                FROM ReadingProcess rp
                JOIN [User] u ON u.ID_User = rp.ID_User
                JOIN Manga m ON m.ID_Manga = rp.ID_Manga
                WHERE u.Email = @email
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                mangas.Add(new Manga
                {
                    Id = (int)reader["ID_Manga"],
                    Name = (string)reader["NameManga"],
                    Cover = reader["Cover"] as string,
                    ReleaseYear = (short)reader["ReleaseYear"]
                });
            }

            return mangas;
        }

        public void AddToReadingProcess(int userId, int mangaId, int type)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                IF NOT EXISTS (
                    SELECT 1 FROM ReadingProcess
                    WHERE ID_User = @userId AND ID_Manga = @mangaId
                )
                INSERT INTO ReadingProcess
                (ID_User, ID_Manga, Type, AddedDate)
                VALUES
                (@userId, @mangaId, @type, GETDATE())
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@mangaId", mangaId);
            cmd.Parameters.AddWithValue("@type", type);

            cmd.ExecuteNonQuery();
        }

        public int? FindMangaIdByName(string q)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT TOP 1 ID_Manga
                FROM Manga
                WHERE NameManga = @q
                ORDER BY ID_Manga DESC;
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", q.Trim());

            var res = cmd.ExecuteScalar();
            return res == null ? null : (int)res;
        }

        public Manga? GetByExactName(string name)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT TOP 1
                    ID_Manga,
                    NameManga,
                    Cover,
                    ReleaseDate AS ReleaseYear
                FROM Manga
                WHERE NameManga = @name
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", name);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            short? year = null;
            if (reader["ReleaseYear"] != DBNull.Value)
                year = Convert.ToInt16(reader["ReleaseYear"]);

            return new Manga
            {
                Id = (int)reader["ID_Manga"],
                Name = (string)reader["NameManga"],
                Cover = reader["Cover"] as string,
                ReleaseYear = year
            };
        }

        public List<Manga> Search(string query)
        {
            var list = new List<Manga>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT
                    m.ID_Manga,
                    m.NameManga,
                    m.Cover,
                    m.ReleaseDate AS ReleaseYear
                FROM Manga m
                WHERE LOWER(m.NameManga) LIKE LOWER(@q)
                ORDER BY m.NameManga
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@q", "%" + query.Trim() + "%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Manga
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Cover = reader["Cover"] as string,
                    ReleaseYear = reader["ReleaseYear"] == DBNull.Value
                        ? null
                        : Convert.ToInt16(reader["ReleaseYear"])
                });
            }

            return list;
        }

    }

}
