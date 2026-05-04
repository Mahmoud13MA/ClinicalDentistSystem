using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class LabTechnician
    {
        [Key]
        public int LabTechnicianID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}