namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class NurseLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int NurseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
