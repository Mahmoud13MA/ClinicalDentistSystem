namespace clinical.APIs.DTOs
{
    public class StockTransactionResponse
    {
        public int T_ID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Quantity { get; set; }
        public int Doctor_ID { get; set; }
        public int Supply_ID { get; set; }
        public DoctorBasicInfo? Doctor { get; set; }
        public SupplyBasicInfo? Supply { get; set; }
    }
}
