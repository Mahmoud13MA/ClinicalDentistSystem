using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.DTOs
{
    public class EHRUpdateRequest
    {
        [Required]
        public int EHR_ID { get; set; }

        public string? Medications { get; set; }
        public string? Allergies { get; set; }
        public string? History { get; set; }
        public string? Treatments { get; set; }

        [Required(ErrorMessage = "Patient_ID is required")]
        public int Patient_ID { get; set; }

        [Required(ErrorMessage = "AppointmentId is required")]
        public int AppointmentId { get; set; }
    }
}
