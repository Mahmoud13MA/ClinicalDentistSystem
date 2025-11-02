
using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models

{
    public class Doctor
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }


        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Stock_Transaction> StockTransactions { get; set; }
    }
}
