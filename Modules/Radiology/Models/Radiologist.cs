using System.ComponentModel.DataAnnotations;

namespace Radiology.Models
{
    public class Radiologist
    {
        [Key]
        public int RadiologistID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public required string Phone { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public ICollection<ImagingAppointment> ImagingAppointments { get; set; } = new List<ImagingAppointment>();

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
