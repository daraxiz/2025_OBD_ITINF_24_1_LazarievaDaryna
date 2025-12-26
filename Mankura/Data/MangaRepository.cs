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
            ORDER BY m.NameManga ASC
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
                manga.Genres = GetGenresForManga(id); ;
                manga.Authors = GetAuthorsForManga(id);
                manga.Artists = GetArtistsForManga(id);
            }

            return manga;
        }
        public List<int> GetGenreIdsForManga(int mangaId)
        {
            var list = new List<int>();

            using var con = new SqlConnection(_connectionString);
            con.Open();

            var cmd = new SqlCommand(@"
        SELECT ID_Genre 
        FROM MangaGenres 
        WHERE ID_Manga = @m", con);

            cmd.Parameters.AddWithValue("@m", mangaId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(r.GetInt32(0));

            return list;
        }

        public List<Manga> GetFiltered(
        string? search,
        int[]? genreIds,
        int[]? typeIds,
        int[]? statusIds,
        int? releaseYear,
        int[]? authorIds,
        int[]? artistIds)
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
                LEFT JOIN Authorship au ON au.ID_Manga = m.ID_Manga
                LEFT JOIN AuthorRole ar ON ar.ID_Role = au.ID_Role
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
            if (authorIds?.Any() == true)
            {
                sql += $@"
        AND EXISTS (
            SELECT 1 FROM Authorship au2
            JOIN AuthorRole ar2 ON ar2.ID_Role = au2.ID_Role
            WHERE au2.ID_Manga = m.ID_Manga
              AND ar2.NameRole = 'Author'
              AND au2.ID_Author IN ({string.Join(",", authorIds)})
        )";
            }

            if (artistIds?.Any() == true)
            {
                sql += $@"
        AND EXISTS (
            SELECT 1 FROM Authorship au2
            JOIN AuthorRole ar2 ON ar2.ID_Role = au2.ID_Role
            WHERE au2.ID_Manga = m.ID_Manga
              AND ar2.NameRole = 'Artist'
              AND au2.ID_Author IN ({string.Join(",", artistIds)})
        )";
            }

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

        public List<Manga> SearchByName(string query)
        {
            var result = new List<Manga>();

            using var con = new SqlConnection(_connectionString);
            con.Open();

            var cmd = new SqlCommand(@"
        SELECT ID_Manga, NameManga, Cover, ReleaseDate
        FROM Manga
        WHERE NameManga LIKE '%' + @q + '%'
        ORDER BY NameManga", con);

            cmd.Parameters.AddWithValue("@q", query);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                result.Add(new Manga
                {
                    Id = (int)rd["ID_Manga"],
                    Name = rd["NameManga"].ToString(),
                    Cover = rd["Cover"] as string,
                    ReleaseYear = rd["ReleaseDate"] == DBNull.Value ? null: Convert.ToInt32(rd["ReleaseDate"])
                });
            }

            return result;
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

        public List<Author> GetAuthors()
        {
            var list = new List<Author>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT DISTINCT a.ID_Author, a.NameAuthor
                FROM Author a
                JOIN Authorship au ON au.ID_Author = a.ID_Author
                JOIN AuthorRole r ON r.ID_Role = au.ID_Role
                WHERE r.NameRole = 'Writer'
                ORDER BY a.NameAuthor
            ";

            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Author
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1)
                });
            }

            return list;
        }

        public List<Author> GetArtists()
        {
            var list = new List<Author>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT DISTINCT a.ID_Author, a.NameAuthor
                FROM Author a
                JOIN Authorship au ON au.ID_Author = a.ID_Author
                JOIN AuthorRole r ON r.ID_Role = au.ID_Role
                WHERE r.NameRole = 'Artist'
                ORDER BY a.NameAuthor
            ";

            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Author
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1)
                });
            }

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
                    m.ReleaseDate AS ReleaseYear,
                    rp.[Type] AS ReadingType
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
                    ReleaseYear = reader["ReleaseYear"] == DBNull.Value ? null : (short)reader["ReleaseYear"],
                    ReadingType = reader["ReadingType"].ToString()
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

        public int AddManga(
            int publisherId,
            string name,
            string description,
            string? cover,
            int statusId,
            int typeId,
            int releaseYear
        )
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            var cmd = new SqlCommand(@"
                INSERT INTO Manga (ID_Publish, NameManga, Description, Cover, ID_Status, ID_Type, ReleaseDate)
                OUTPUT INSERTED.ID_Manga
                VALUES (@pid, @n, @d, @c, @s, @t, @y)", con);

            cmd.Parameters.AddWithValue("@pid", publisherId);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@d", description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@c", cover ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@s", statusId);
            cmd.Parameters.AddWithValue("@t", typeId);
            cmd.Parameters.AddWithValue("@y", releaseYear);

            return (int)cmd.ExecuteScalar();
        }

        public List<string> GetAuthorsForManga(int mangaId)
        {
            var list = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT a.NameAuthor
                FROM Authorship au
                JOIN Author a ON a.ID_Author = au.ID_Author
                WHERE au.ID_Manga = @id AND au.ID_Role = 2
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", mangaId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(r.GetString(0));

            return list;
        }

        public void AddGenreToManga(int mangaId, int genreId)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            var cmd = new SqlCommand(@"
                INSERT INTO MangaGenres (ID_Manga, ID_Genre)
                VALUES (@m, @g)
            ", con);

            cmd.Parameters.AddWithValue("@m", mangaId);
            cmd.Parameters.AddWithValue("@g", genreId);

            cmd.ExecuteNonQuery();
        }

        public List<string> GetArtistsForManga(int mangaId)
        {
            var list = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"
                SELECT a.NameAuthor
                FROM Authorship au
                JOIN Author a ON a.ID_Author = au.ID_Author
                WHERE au.ID_Manga = @id AND au.ID_Role = 1
            ";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", mangaId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(r.GetString(0));

            return list;
        }

        public void AddGenres(int mangaId, List<int> genreIds)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            foreach (var genreId in genreIds)
            {
                using var cmd = new SqlCommand(
                    "INSERT INTO MangaGenres (ID_Manga, ID_Genre) VALUES (@m, @g)", con);
                cmd.Parameters.AddWithValue("@m", mangaId);
                cmd.Parameters.AddWithValue("@g", genreId);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddAuthorship(int mangaId, List<int> authorIds, int roleId)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            foreach (var authorId in authorIds)
            {
                using var cmd = new SqlCommand(@"
            INSERT INTO Authorship (ID_Manga, ID_Author, ID_Role)
            VALUES (@m, @a, @r)", con);

                cmd.Parameters.AddWithValue("@m", mangaId);
                cmd.Parameters.AddWithValue("@a", authorId);
                cmd.Parameters.AddWithValue("@r", roleId);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddAuthorshipByNames(int mangaId, IEnumerable<string> names, int roleId)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            foreach (var name in names)
            {
                var getCmd = new SqlCommand(
                    "SELECT ID_Author FROM Author WHERE NameAuthor = @name",
                    con
                );
                getCmd.Parameters.AddWithValue("@name", name);

                var authorId = getCmd.ExecuteScalar() as int?;

                if (authorId == null)
                {
                    var insertCmd = new SqlCommand(
                        @"INSERT INTO Author (NameAuthor)
                  OUTPUT INSERTED.ID_Author
                  VALUES (@name)",
                        con
                    );
                    insertCmd.Parameters.AddWithValue("@name", name);
                    authorId = (int)insertCmd.ExecuteScalar();
                }

                var linkCmd = new SqlCommand(
                    @"INSERT INTO Authorship (ID_Author, ID_Manga, ID_Role)
              VALUES (@a, @m, @r)",
                    con
                );

                linkCmd.Parameters.AddWithValue("@a", authorId);
                linkCmd.Parameters.AddWithValue("@m", mangaId);
                linkCmd.Parameters.AddWithValue("@r", roleId);
                linkCmd.ExecuteNonQuery();
            }
        }

        public bool ExistsByName(string name)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Manga WHERE NameManga = @name", con);

            cmd.Parameters.AddWithValue("@name", name);

            return (int)cmd.ExecuteScalar() > 0;
        }

        public int GetOrCreatePublisher(string name)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            using (var check = new SqlCommand(
                "SELECT ID_Publish FROM Publish WHERE NamePublish = @n", con))
            {
                check.Parameters.AddWithValue("@n", name);
                var result = check.ExecuteScalar();

                if (result != null)
                    return (int)result;
            }

            using (var insert = new SqlCommand(
                "INSERT INTO Publish(NamePublish) OUTPUT INSERTED.ID_Publish VALUES(@n)", con))
            {
                insert.Parameters.AddWithValue("@n", name);
                return (int)insert.ExecuteScalar();
            }
        }

        public int GetOrCreateAuthor(string name)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            using (var check = new SqlCommand(
                "SELECT ID_Author FROM Author WHERE NameAuthor = @n", con))
            {
                check.Parameters.AddWithValue("@n", name);
                var res = check.ExecuteScalar();
                if (res != null)
                    return (int)res;
            }

            using (var insert = new SqlCommand(
                "INSERT INTO Author(NameAuthor) OUTPUT INSERTED.ID_Author VALUES(@n)", con))
            {
                insert.Parameters.AddWithValue("@n", name);
                return (int)insert.ExecuteScalar();
            }
        }

        public void Update(int id, string name, string desc, int year,
                   int statusId, int typeId, string? cover)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            var sql = @"
        UPDATE Manga SET
            NameManga = @name,
            Description = @desc,
            ReleaseDate = @year,
            ID_Status = @status,
            ID_Type = @type,
            Cover = @cover
        WHERE ID_Manga = @id";

            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@desc", desc);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@status", statusId);
            cmd.Parameters.AddWithValue("@type", typeId);
            cmd.Parameters.AddWithValue("@cover", (object?)cover ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void UpdateGenres(int mangaId, List<int> genreIds)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            var del = new SqlCommand(
                "DELETE FROM MangaGenres WHERE ID_Manga = @m", con);
            del.Parameters.AddWithValue("@m", mangaId);
            del.ExecuteNonQuery();

            foreach (var gid in genreIds)
            {
                var ins = new SqlCommand(
                    "INSERT INTO MangaGenres (ID_Manga, ID_Genre) VALUES (@m,@g)", con);
                ins.Parameters.AddWithValue("@m", mangaId);
                ins.Parameters.AddWithValue("@g", gid);
                ins.ExecuteNonQuery();
            }
        }

        public List<string> GetGenresForManga(int mangaId)
        {
            var list = new List<string>();

            using var con = new SqlConnection(_connectionString);
            con.Open();

            var cmd = new SqlCommand(@"
        SELECT g.NameGenre
        FROM MangaGenres mg
        JOIN Genre g ON g.ID_Genre = mg.ID_Genre
        WHERE mg.ID_Manga = @m", con);

            cmd.Parameters.AddWithValue("@m", mangaId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(r.GetString(0));

            return list;
        }

    }
}
