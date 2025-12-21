using Mankura.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly Db _db;
    public AdminController(Db db) => _db = db;

    public IActionResult Panel()
    {
        using var con = _db.GetConnection();
        con.Open();

        int usersCount = ExecScalarInt(con, "SELECT COUNT(*) FROM [User]");
        int mangaCount = ExecScalarInt(con, "SELECT COUNT(*) FROM Manga");
        int commentsCount = ExecScalarInt(con, "SELECT COUNT(*) FROM Comment");

        var users = new List<(int Id, string Username, string Email, string Role)>();

        var cmd = new SqlCommand(@"
            SELECT u.ID_User, u.UserName, u.Email, r.NameRole
            FROM [User] u
            JOIN UserRole r ON u.ID_Role = r.ID_Role
            ORDER BY u.ID_User DESC", con);

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            users.Add((
                (int)rd["ID_User"],
                rd["UserName"].ToString(),
                rd["Email"].ToString(),
                rd["NameRole"].ToString()
            ));
        }

        ViewBag.UsersCount = usersCount;
        ViewBag.MangaCount = mangaCount;
        ViewBag.CommentsCount = commentsCount;

        return View(users);
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        using var con = _db.GetConnection();
        con.Open();

        var cmd = new SqlCommand("DELETE FROM [User] WHERE ID_User = @id", con);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();

        return RedirectToAction("Panel");
    }

    private int ExecScalarInt(SqlConnection con, string sql)
    {
        using var cmd = new SqlCommand(sql, con);
        return (int)cmd.ExecuteScalar();
    }

    private readonly CloudinaryImportService _import;

    public AdminController(CloudinaryImportService import)
    {
        _import = import;
    }

    [HttpPost]
    public IActionResult ImportFromCloudinary(
        int mangaId,
        string publicId,
        string imageUrl
    )
    {
        _import.ImportImage(mangaId, publicId, imageUrl);
        return Ok();
    }
}
