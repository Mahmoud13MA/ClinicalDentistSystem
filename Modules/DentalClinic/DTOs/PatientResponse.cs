namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class PatientResponse
    {
        public int Patient_ID { get; set; }
        public string First { get; set; } = string.Empty;
        public string Middle { get; set; } = string.Empty;
        public string Last { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public string? Phone { get; set; }
    }
}
