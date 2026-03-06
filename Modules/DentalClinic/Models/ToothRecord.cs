using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class ToothRecord
    {
        [Key]
        public int ToothRecord_ID { get; set; }

        [Required]
        [Range(11, 48)]
        public int ToothNumber { get; set; }           
        public string? Condition { get; set; }         // Caries, Missing, Fractured, Healthy
        public string? TreatmentPlanned { get; set; }
        public string? TreatmentCompleted { get; set; }
        public string? Surfaces { get; set; }          // M, D, O, I, B, L (tooth surfaces)
        public string? Notes { get; set; }

        public DateTime LastUpdated { get; set; }

        // Foreign Key
        [ForeignKey(nameof(EHR))]
        public int EHR_ID { get; set; }
        public EHR? EHR { get; set; }
    }
}
