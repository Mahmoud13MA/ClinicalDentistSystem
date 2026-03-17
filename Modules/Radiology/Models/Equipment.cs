using System.ComponentModel.DataAnnotations;

namespace Radiology.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentID { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        public ICollection<ImagingAppointment> ImagingAppointments { get; set; } = new List<ImagingAppointment>();
    }
}
