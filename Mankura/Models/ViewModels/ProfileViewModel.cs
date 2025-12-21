using Mankura.Models;

public class ProfileViewModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string AvatarUrl { get; set; } = "/img/Avatar.svg";

    public List<Manga> ReadingList { get; set; } = new();
}