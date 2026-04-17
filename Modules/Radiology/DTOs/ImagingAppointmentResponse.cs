namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class ImagingAppointmentResponse
    {
        public int ImagingID { get; set; }
        public DateTime Datetime { get; set; }
        public string Type { get; set; }
        public int PatientID { get; set; }
        public PatientBasicInfoRadiology Patient { get; set; }
        public int RadiologistID { get; set; }
        public RadiologistBasicInfo Radiologist { get; set; }
        public int EquipmentID { get; set; }
        public EquipmentResponseBasicInfo Equipment { get; set; }
    }

    public class PatientBasicInfoRadiology
    {
        public int Patient_ID { get; set; }
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
        public string FullName => $"{First} {Middle} {Last}".Replace("  ", " ").Trim();
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Phone { get; set; }
    }

    public class RadiologistBasicInfo
    {
        public int RadiologistID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Specialty { get; set; }
    }

    public class EquipmentResponseBasicInfo
    {
        public int EquipmentID { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
    }
}
