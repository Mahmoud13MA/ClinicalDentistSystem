namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class ImagingAppointmentResponse
    {
        public int ImagingID { get; set; }
        public DateTime Datetime { get; set; }
        public string Type { get; set; } = string.Empty;
        public int PatientID { get; set; }
        public PatientBasicInfoRadiology Patient { get; set; }
        public int RadiologistID { get; set; }
        public RadiologistBasicInfo Radiologist { get; set; }
        public int EquipmentID { get; set; }
        public EquipmentResponseBasicInfo Equipment { get; set; }
    }

    public class PatientBasicInfoRadiology
    {
        public int PatientID { get; set; }
    }

    public class RadiologistBasicInfo
    {
        public int RadiologistID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
    }

    public class EquipmentResponseBasicInfo
    {
        public int EquipmentID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
