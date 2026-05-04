using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class RadiologistLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
