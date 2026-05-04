namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class RadiologistLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int RadiologistID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
    }
}
