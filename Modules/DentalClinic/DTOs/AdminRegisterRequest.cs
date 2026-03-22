using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class AdminRegisterRequest
    {

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        [MinLength(6)]
        public string Password { get; set; } 

        [Required]
        public string AdminRegistrationKey { get; set; }


        

    }
}