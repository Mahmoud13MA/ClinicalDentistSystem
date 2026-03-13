namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class StockTransactionBasicInfo
    {
        public int T_ID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Quantity { get; set; }
        public int Doctor_ID { get; set; }
        public string? DoctorName { get; set; }
    }
}
