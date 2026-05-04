using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.Models

{
    public class Supply
    {
        [Key]
        public int Supply_ID { get; set; }
        public string Supply_Name { get; set; } = String.Empty;
        public string Category { get; set; } = String.Empty;
        public string Unit { get; set; }= String.Empty;
        public int Quantity { get; set; }
        public string? Description { get; set; }

        public ICollection<Stock_Transaction>? StockTransactions { get; set; }
    }
}

