using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }

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

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

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