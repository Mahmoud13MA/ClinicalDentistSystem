using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models
{
    public class Nurse
    {
        [Key]
        public int NURSE_ID { get; set; }
        
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
    }
}


