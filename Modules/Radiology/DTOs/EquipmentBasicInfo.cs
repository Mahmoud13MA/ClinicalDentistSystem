namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class EquipmentBasicInfo
    {
        public int EquipmentID { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public DateTime ServiceDate { get; set; }
    }
}
