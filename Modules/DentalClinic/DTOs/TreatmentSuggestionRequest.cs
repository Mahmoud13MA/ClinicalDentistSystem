namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class TreatmentSuggestionRequest
    {
        public string Diagnosis { get; set; } = string.Empty;
        public string? PatientHistory { get; set; }
    }
}
