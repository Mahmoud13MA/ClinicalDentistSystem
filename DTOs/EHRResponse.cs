namespace clinical.APIs.DTOs
{
    public class EHRResponse
    {
        public int EHR_ID { get; set; }
        public string Medications { get; set; }
        public string Allergies { get; set; }
        public string History { get; set; }
        public string Treatments { get; set; }
        public DateTime Last_Updated { get; set; }
        public int Patient_ID { get; set; }
        public int AppointmentId { get; set; }
        public PatientBasicInfo? Patient { get; set; }
        public AppointmentBasicInfo? Appointment { get; set; }
    }
}
