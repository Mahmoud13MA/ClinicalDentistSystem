namespace clinical.APIs.DTOs
{
    public class AppointmentResponse
    {
        public int Appointment_ID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Ref_Num { get; set; }
        public string Type { get; set; }
        public int Patient_ID { get; set; }
        public PatientBasicInfo Patient { get; set; }
        public int Doctor_ID { get; set; }
        public DoctorBasicInfo Doctor { get; set; }
        public int Nurse_ID { get; set; }
        public NurseBasicInfo Nurse { get; set; }
    }

    public class PatientBasicInfo
    {
        public int Patient_ID { get; set; }
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
        public string FullName => $"{First} {Middle} {Last}".Replace("  ", " ").Trim();
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string? Phone { get; set; }
    }

    public class DoctorBasicInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class NurseBasicInfo
    {
        public int NURSE_ID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
