using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Radiology.Models
{
    public class Report
    {
        [Key]
        public int ReportID { get; set; }

        [Required]
        [StringLength(500)]
        public string Findings { get; set; }

        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; }

        

        [ForeignKey("ImagingAppointment")]
        public int ImagingID { get; set; }
        public ImagingAppointment ImagingAppointment { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }
        public Patient Patient { get; set; }

        [ForeignKey("Radiologist")]
        public int RadiologistID { get; set; }
        public Radiologist Radiologist { get; set; }
    }
}
