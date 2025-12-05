namespace clinical.APIs.DTOs
{
    public class NurseLoginResponse
    {
        public string Token { get; set; }
        public int NurseId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
