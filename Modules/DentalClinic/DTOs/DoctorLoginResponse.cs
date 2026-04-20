namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class DoctorLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
