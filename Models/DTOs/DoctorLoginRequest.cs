using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models.DTOs
{
    public class DoctorLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
