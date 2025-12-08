namespace clinical.APIs.DTOs
{
    public class SupplyBasicInfo
    {
        public int Supply_ID { get; set; }
        public string Supply_Name { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
    }
}
