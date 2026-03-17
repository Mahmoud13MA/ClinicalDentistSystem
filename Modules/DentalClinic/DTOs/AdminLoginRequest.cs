using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class AdminLoginRequest
    {
        [Required]
        [EmailAddress]
       public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }

   
    }
}
