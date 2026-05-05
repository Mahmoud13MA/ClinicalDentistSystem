using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShippingMethod { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [ForeignKey("LabTechnician")]
        public int LabTechnicianID { get; set; }
        public LabTechnician LabTechnician { get; set; } = null!;

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}