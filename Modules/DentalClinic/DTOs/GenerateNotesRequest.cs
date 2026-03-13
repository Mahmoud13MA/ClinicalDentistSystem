namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class GenerateNotesRequest
    {
        public string BulletPoints { get; set; } = string.Empty;
        public string? PatientContext { get; set; }
    }
}
