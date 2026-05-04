using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Radiology.Models
{
    public class ImagingAppointment
    {
        [Key]
        public int ImagingID { get; set; }
        
        public DateTime Datetime { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;



        [ForeignKey("Patient")]
        public int PatientID { get; set; }
        public Patient? Patient { get; set; }

        [ForeignKey("Radiologist")]
        public int RadiologistID { get; set; }
        public Radiologist? Radiologist { get; set; }

        [ForeignKey("Equipment")]
        public int EquipmentID { get; set; }
        public Equipment? Equipment { get; set; }

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
