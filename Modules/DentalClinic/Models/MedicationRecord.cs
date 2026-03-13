using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class MedicationRecord
    {
        [Key]
        public int Medication_ID { get; set; }

        [Required]
        public string Name { get; set; }               // Medication name
        
        public string? Dosage { get; set; }            // e.g., "500mg"
        public string? Frequency { get; set; }         // e.g., "Twice daily"
        public string? Route { get; set; }             // e.g., "Oral", "Topical"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }

        // Foreign Key
        [ForeignKey(nameof(EHR))]
        public int EHR_ID { get; set; }
        public EHR? EHR { get; set; }
    }
}
