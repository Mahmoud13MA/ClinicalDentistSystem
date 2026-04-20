using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class NurseRegisterRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration key is required")]
        public string RegistrationKey { get; set; }
    }
}
