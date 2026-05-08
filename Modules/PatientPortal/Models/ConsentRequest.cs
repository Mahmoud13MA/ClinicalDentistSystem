using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using clinical.APIs.Modules.DentalClinic.Models; 

namespace clinical.APIs.Modules.PatientPortal.Models
{
    public class ConsentRequest
    {
        [Key]
        public int Consent_ID { get; set; }

        [Required]
        public string ActionType { get; set; } = string.Empty; 

        [Required]
        public string Description { get; set; } = string.Empty; 

        [Required]
        public string Status { get; set; } = "Pending"; 

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? RespondedAt { get; set; }

        // --- Foreign Keys ---
        [ForeignKey("Patient")]
        public int Patient_ID { get; set; }
        public Patient? Patient { get; set; }

        [ForeignKey("Doctor")]
        public int Doctor_ID { get; set; }
        public Doctor? Doctor { get; set; }
    }
}