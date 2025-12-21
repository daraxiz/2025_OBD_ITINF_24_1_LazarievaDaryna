namespace Mankura.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime RegistrationDate { get; set; }

        public int RoleId { get; set; }

        public string? Avatar { get; set; }
    }
}
