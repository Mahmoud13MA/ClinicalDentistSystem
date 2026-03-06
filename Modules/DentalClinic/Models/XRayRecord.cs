using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class XRayRecord
    {
        [Key]
        public int XRay_ID { get; set; }

        [Required]
        public string Type { get; set; }           
        
        public string? Findings { get; set; }         
        public string? ImagePath { get; set; }        
        public byte[]? ImageData { get; set; }         
        public DateTime TakenAt { get; set; }
        public string? TakenBy { get; set; }           
        public string? Notes { get; set; }

        // Foreign Key
        [ForeignKey(nameof(EHR))]
        public int EHR_ID { get; set; }
        public EHR? EHR { get; set; }
    }
}
