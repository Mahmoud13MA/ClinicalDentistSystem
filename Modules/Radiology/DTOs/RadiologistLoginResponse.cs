namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class RadiologistLoginResponse
    {
        public string Token { get; set; }
        public int RadiologistID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Specialty { get; set; }
    }
}
