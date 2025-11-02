using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models

{
    public class Supply
    {
        [Key]
        public int Supply_ID { get; set; }
        public string Supply_Name { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }

        public ICollection<Stock_Transaction> StockTransactions { get; set; }
    }
}

