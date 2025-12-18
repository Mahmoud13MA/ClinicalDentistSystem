namespace clinical.APIs.DTOs
{
    public class ParseEHRRequest
    {
        public string LargeText { get; set; } = string.Empty;
        public string PatientContext { get; set; } = string.Empty;
    }

    public class ParseEHRResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public EHRFieldsResponse? ExtractedFields { get; set; }
    }

    public class EHRFieldsResponse
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
        
        // Legacy Fields
        public string? History { get; set; }
        public string? Treatments { get; set; }
        
        // Structured Data
        public List<MedicationData>? Medications { get; set; }
        public List<ProcedureData>? Procedures { get; set; }
        public List<ToothData>? AffectedTeeth { get; set; }
        public List<XRayData>? XRays { get; set; }
    }

    public class MedicationData
    {
        public string? Name { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
    }

    public class ProcedureData
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ToothData
    {
        public int ToothNumber { get; set; }
        public string? Condition { get; set; }
        public string? Treatment { get; set; }
    }

    public class XRayData
    {
        public string? Type { get; set; }
        public string? Findings { get; set; }
        public DateTime? Date { get; set; }
    }
}
