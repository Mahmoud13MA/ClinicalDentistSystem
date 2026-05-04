using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.Models

{
    public class Doctor
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }= string.Empty;
        
        [Required]
        public string PasswordHash { get; set; }=string.Empty;

        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Stock_Transaction>? StockTransactions { get; set; }
    }
}
