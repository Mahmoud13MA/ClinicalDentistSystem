using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.DTOs
{
    public class NurseLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
