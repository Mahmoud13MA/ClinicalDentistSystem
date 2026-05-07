using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }

        public int PatientId { get; set; }
        public int DentistId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Shade { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Material { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ProductType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CaseType { get; set; } = string.Empty;

        [Range(11, 48)]
        public int ToothNumber { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [ForeignKey("LabTechnician")]
        public int LabTechnicianID { get; set; }
        public LabTechnician LabTechnician { get; set; } = null!;

        [ForeignKey("Order")]
        public int OrderID { get; set; }
        public Order Order { get; set; } = null!;
    }
}