using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class UpdateCredentialsRequest : IValidatableObject
    {
        [EmailAddress]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(
                    "At least one of Email or Password must be provided.",
                    new[] { nameof(Email), nameof(Password) });
            }
        }


    }
}
