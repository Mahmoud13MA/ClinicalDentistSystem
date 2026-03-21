using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class UpdateCredentialsRequest
    {
        [Required]
        [EmailAddress]
         public string Email { get; set; }
       [MinLength(6)]
       public string Password { get; set; }


    }
}
