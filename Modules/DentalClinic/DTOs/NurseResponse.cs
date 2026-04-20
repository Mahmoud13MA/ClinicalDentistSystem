namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class NurseResponse
    {
        public int NURSE_ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
