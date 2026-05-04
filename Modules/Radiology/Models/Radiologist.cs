using System.ComponentModel.DataAnnotations;

namespace Radiology.Models
{
    public class Radiologist
    {
        [Key]
        public int RadiologistID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public required string Phone { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<ImagingAppointment> ImagingAppointments { get; set; } = new List<ImagingAppointment>();

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
