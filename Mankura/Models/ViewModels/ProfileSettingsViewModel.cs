using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Mankura.Models.ViewModels
{
    public class ProfileSettingsViewModel
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        public IFormFile? Avatar { get; set; }
    }
}
