using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class ReportCreateRequest
    {
        [Required(ErrorMessage = "Findings are required")]
        [StringLength(500, ErrorMessage = "Findings cannot exceed 500 characters")]
        public string Findings { get; set; }

        [Required(ErrorMessage = "Diagnosis is required")]
        [StringLength(500, ErrorMessage = "Diagnosis cannot exceed 500 characters")]
        public string Diagnosis { get; set; }

        [Required(ErrorMessage = "Imaging Appointment ID is required")]
        public int ImagingID { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "Radiologist ID is required")]
        public int RadiologistID { get; set; }
    }
}
