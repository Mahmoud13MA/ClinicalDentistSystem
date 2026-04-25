namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class EquipmentResponse
    {
        public int EquipmentID { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public List<ImagingAppointmentBasicInfo>? ImagingAppointments { get; set; }
    }
}
