using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class EHRUpdateRequest
    {
        [Required]
        public int EHR_ID { get; set; }

        // Medical Information
        public string? Allergies { get; set; }
        public string? MedicalAlerts { get; set; }

        // Dental Information
        public string? Diagnosis { get; set; }
        public string? XRayFindings { get; set; }
        public string? PeriodontalStatus { get; set; }
        public string? ClinicalNotes { get; set; }
        public string? Recommendations { get; set; }

        // Legacy fields (optional for backwards compatibility)
        public string? History { get; set; }
        public string? Treatments { get; set; }

        [Required(ErrorMessage = "Patient_ID is required")]
        public int Patient_ID { get; set; }

        [Required(ErrorMessage = "AppointmentId is required")]
        public int AppointmentId { get; set; }

        // Optional collections for normalized data
        public List<MedicationRecordDto>? Medications { get; set; }
        public List<ProcedureRecordDto>? Procedures { get; set; }
        public List<ToothRecordDto>? Teeth { get; set; }
        public List<XRayRecordDto>? XRays { get; set; }
    }
}
