namespace clinical.APIs.DTOs
{
    public class DoctorLoginResponse
    {
        public string Token { get; set; }
        public int DoctorId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
