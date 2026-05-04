using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        [ForeignKey("LabTechnician")]
        public int LabTechnicianID { get; set; }
        public LabTechnician LabTechnician { get; set; } = null!;

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}