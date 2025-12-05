using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models.DTOs
{
    public class DoctorRegisterRequest
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Phone { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
