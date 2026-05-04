using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class RadiologistUpdateRequest
    {
        [Required]
        public int RadiologistID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialty is required")]
        public string Specialty { get; set; } = string.Empty;
    }
}
