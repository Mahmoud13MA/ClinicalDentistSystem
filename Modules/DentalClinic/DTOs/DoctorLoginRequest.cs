using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class DoctorLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
