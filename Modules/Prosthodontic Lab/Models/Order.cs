using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int PatientId { get; set; }
        public int DentistId { get; set; }

        [ForeignKey("LabTechnician")]
        public int LabTechnicianID { get; set; }
        public LabTechnician LabTechnician { get; set; } = null!;

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime RequiredDate { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ShippingMethod { get; set; } = string.Empty;

        [MaxLength(200)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}