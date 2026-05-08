using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Nurse
    {
        [Key]
        public int NURSE_ID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
    }
}


