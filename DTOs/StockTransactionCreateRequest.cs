using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.DTOs
{
    public class StockTransactionCreateRequest
    {
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time is required")]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Doctor_ID is required")]
        public int Doctor_ID { get; set; }

        [Required(ErrorMessage = "Supply_ID is required")]
        public int Supply_ID { get; set; }
    }
}
