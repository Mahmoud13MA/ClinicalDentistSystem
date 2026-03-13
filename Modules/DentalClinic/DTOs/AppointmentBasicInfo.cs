namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class AppointmentBasicInfo
    {
        public int Appointment_ID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Ref_Num { get; set; }
        public string Type { get; set; }
    }
}
