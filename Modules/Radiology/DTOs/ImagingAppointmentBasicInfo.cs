namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class ImagingAppointmentBasicInfo
    {
        public int ImagingID { get; set; }
        public DateTime Datetime { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
