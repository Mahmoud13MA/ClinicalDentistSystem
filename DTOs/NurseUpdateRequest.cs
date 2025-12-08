using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.DTOs
{
    public class NurseUpdateRequest
    {
        [Required]
        public int NURSE_ID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}
