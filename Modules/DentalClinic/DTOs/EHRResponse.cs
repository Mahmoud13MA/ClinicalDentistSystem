namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class EHRResponse
    {
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

        // Legacy fields
        public string? History { get; set; }
        public string? Treatments { get; set; }

        // Metadata
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        public int Patient_ID { get; set; }
        public int AppointmentId { get; set; }
        public PatientBasicInfo? Patient { get; set; }
        public AppointmentBasicInfo? Appointment { get; set; }

        // Normalized data collections
        public List<MedicationRecordResponse>? Medications { get; set; }
        public List<ProcedureRecordResponse>? Procedures { get; set; }
        public List<ToothRecordResponse>? Teeth { get; set; }
        public List<XRayRecordResponse>? XRays { get; set; }

        // Change log history (audit trail)
        public List<EHRChangeLogResponse>? ChangeLogs { get; set; }
    }

    public class MedicationRecordResponse
    {
        public int Medication_ID { get; set; }
        public string Name { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Route { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class ProcedureRecordResponse
    {
        public int Procedure_ID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public DateTime PerformedAt { get; set; }
        public string? ToothNumber { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    public class ToothRecordResponse
    {
        public int ToothRecord_ID { get; set; }
        public int ToothNumber { get; set; }
        public string? Condition { get; set; }
        public string? TreatmentPlanned { get; set; }
        public string? TreatmentCompleted { get; set; }
        public string? Surfaces { get; set; }
        public string? Notes { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class XRayRecordResponse
    {
        public int XRay_ID { get; set; }
        public string Type { get; set; }
        public string? Findings { get; set; }
        public string? ImagePath { get; set; }
        public bool HasImage { get; set; }
        public DateTime TakenAt { get; set; }
        public string? TakenBy { get; set; }
        public string? Notes { get; set; }
    }
}
