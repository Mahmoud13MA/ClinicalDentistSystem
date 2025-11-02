using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models
{
    public class Stock_Transaction
    {
        [Key]
        public int T_ID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Quantity { get; set; }

        // Foreign Keys
        public int Doctor_ID { get; set; }
        public int Supply_ID { get; set; }

        // Navigation Properties
        public Doctor Doctor { get; set; }
        public Supply Supply { get; set; }
    }
}

