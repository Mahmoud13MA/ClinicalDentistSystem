using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class ProcedureRecord
    {
        [Key]
        public int Procedure_ID { get; set; }

        [Required]
        public string Code { get; set; }               // D1110, D2740, etc.
        
        [Required]
        public string Description { get; set; }        // Procedure description
        
        public DateTime PerformedAt { get; set; }
        
        public string? ToothNumber { get; set; }       // Which tooth(s) involved
        public string? Status { get; set; }            // Planned, Completed, Cancelled
        public string? Notes { get; set; }

        // Foreign Key
        [ForeignKey(nameof(EHR))]
        public int EHR_ID { get; set; }
        public EHR? EHR { get; set; }
    }
}
