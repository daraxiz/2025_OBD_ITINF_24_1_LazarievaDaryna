namespace Mankura.Models.ViewModels
{
    public class AdminUserViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
