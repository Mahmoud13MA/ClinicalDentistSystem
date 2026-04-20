namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class SupplyResponse
    {
        public int Supply_ID { get; set; }
        public string Supply_Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public List<StockTransactionBasicInfo>? StockTransactions { get; set; }
    }
}
