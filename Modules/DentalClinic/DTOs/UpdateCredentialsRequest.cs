using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class UpdateCredentialsRequest
    {
        [Required]
        [EmailAddress]
        string Email { get; set; }
        [MinLength(6)]
        string Password { get; set; }


    }
}
