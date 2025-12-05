using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models

{
    public class Doctor
    {
        [Key]
        public int ID { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Phone { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Stock_Transaction>? StockTransactions { get; set; }
    }
}
