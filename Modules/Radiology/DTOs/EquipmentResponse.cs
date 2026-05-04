namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class EquipmentResponse
    {
        public int EquipmentID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public List<ImagingAppointmentBasicInfo>? ImagingAppointments { get; set; }
    }
}
