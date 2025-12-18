namespace clinical.APIs.Services
{
    public interface ILlamaService
    {
        /// <summary>
        /// Generate auto-complete suggestions for dental clinical notes
        /// </summary>
        Task<List<string>> GetAutoCompleteSuggestionsAsync(string partialText, string context);

        /// <summary>
        /// Generate complete clinical notes from bullet points
        /// </summary>
        Task<string> GenerateClinicalNotesAsync(string bulletPoints, string patientContext);

        /// <summary>
        /// Suggest treatments based on diagnosis
        /// </summary>
        Task<List<string>> SuggestTreatmentsAsync(string diagnosis, string patientHistory);

        /// <summary>
        /// Extract structured data from free-text clinical notes
        /// </summary>
        Task<ClinicalDataExtractionResult> ExtractClinicalDataAsync(string freeText);

        /// <summary>
        /// Generate dental terminology suggestions
        /// </summary>
        Task<List<string>> GetDentalTerminologySuggestionsAsync(string partialTerm);

        /// <summary>
        /// Parse large doctor's text and extract ALL EHR fields automatically
        /// </summary>
        Task<CompleteEHRExtractionResult> ParseToCompleteEHRAsync(string largeText, string patientContext, CancellationToken cancellationToken = default);
    }

    public class ClinicalDataExtractionResult
    {
        public string? Diagnosis { get; set; }
        public List<string>? Symptoms { get; set; }
        public List<string>? Treatments { get; set; }
        public string? PeriodontalStatus { get; set; }
        public List<string>? Medications { get; set; }
        public List<int>? AffectedTeeth { get; set; }
    }

    public class CompleteEHRExtractionResult
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
        public List<MedicationExtraction>? Medications { get; set; }
        public List<ProcedureExtraction>? Procedures { get; set; }
        public List<ToothExtraction>? AffectedTeeth { get; set; }
        public List<XRayExtraction>? XRays { get; set; }
    }

    public class MedicationExtraction
    {
        public string? Name { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
    }

    public class ProcedureExtraction
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ToothExtraction
    {
        public int ToothNumber { get; set; }
        public string? Condition { get; set; }
        public string? Treatment { get; set; }
    }

    public class XRayExtraction
    {
        public string? Type { get; set; }
        public string? Findings { get; set; }
        public DateTime? Date { get; set; }
    }
}
