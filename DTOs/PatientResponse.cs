namespace clinical.APIs.DTOs
{
    public class PatientResponse
    {
        public int Patient_ID { get; set; }
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string? Phone { get; set; }
    }
}
