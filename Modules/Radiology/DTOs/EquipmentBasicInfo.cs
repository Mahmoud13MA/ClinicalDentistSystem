namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class EquipmentBasicInfo
    {
        public int EquipmentID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
    }
}
