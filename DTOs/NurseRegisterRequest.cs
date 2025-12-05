using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.DTOs
{
    public class NurseRegisterRequest
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
