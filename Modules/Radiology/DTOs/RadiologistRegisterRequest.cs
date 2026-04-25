using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class RadiologistRegisterRequest
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Phone { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Specialty { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Registration key is required")]
        public string RegistrationKey { get; set; }
    }
}
