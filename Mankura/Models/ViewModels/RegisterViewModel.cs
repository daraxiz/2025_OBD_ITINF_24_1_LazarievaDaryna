using System.ComponentModel.DataAnnotations;

namespace Mankura.Models.ViewModels
{
    public class RegisterViewModel
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
