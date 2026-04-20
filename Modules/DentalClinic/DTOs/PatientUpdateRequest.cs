using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class PatientUpdateRequest
    {
        [Required]
        public int Patient_ID { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string First { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Middle { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string Last { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(50)]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DOB { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }
    }
}
