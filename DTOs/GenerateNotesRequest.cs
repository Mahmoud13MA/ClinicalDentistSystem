namespace clinical.APIs.DTOs
{
    public class GenerateNotesRequest
    {
        public string BulletPoints { get; set; } = string.Empty;
        public string? PatientContext { get; set; }
    }
}
