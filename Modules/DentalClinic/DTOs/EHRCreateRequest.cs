using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class EHRCreateRequest
    {
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

    public class MedicationRecordDto
    {
        public string Name { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Route { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ProcedureRecordDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public DateTime PerformedAt { get; set; }
        public string? ToothNumber { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ToothRecordDto
    {
        public int ToothNumber { get; set; }
        public string? Condition { get; set; }
        public string? TreatmentPlanned { get; set; }
        public string? TreatmentCompleted { get; set; }
        public string? Surfaces { get; set; }
        public string? Notes { get; set; }
    }

    public class XRayRecordDto
    {
        public string Type { get; set; }
        public string? Findings { get; set; }
        public string? ImagePath { get; set; }
        public DateTime TakenAt { get; set; }
        public string? TakenBy { get; set; }
        public string? Notes { get; set; }
    }
}
