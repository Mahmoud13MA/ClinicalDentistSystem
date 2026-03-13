using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class EHR
    {
        [Key]
        public int EHR_ID { get; set; }

        // ---- Medical Information ----
        public string? Allergies { get; set; }
        public string? MedicalAlerts { get; set; }     // Diabetes, Hypertension

        // ---- Dental Information ----
        public string? Diagnosis { get; set; }
        public string? XRayFindings { get; set; }      // General X-ray findings (legacy)
        public string? PeriodontalStatus { get; set; }
        public string? ClinicalNotes { get; set; }
        public string? Recommendations { get; set; }

        // ---- Legacy Fields (kept for backwards compatibility) ----
        public string? History { get; set; }
        public string? Treatments { get; set; }

        // ---- Metadata ----
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }         // Doctor name who last updated

        // ---- Foreign Keys ----
        [ForeignKey(nameof(Patient))]
        public int Patient_ID { get; set; }
        public Patient? Patient { get; set; }

        [ForeignKey(nameof(Appointment))]
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        // ---- Navigation Collections ----
        public List<EHRChangeLog>? ChangeLogs { get; set; }
        public List<MedicationRecord>? Medications { get; set; }
        public List<ProcedureRecord>? Procedures { get; set; }
        public List<ToothRecord>? Teeth { get; set; }
        public List<XRayRecord>? XRays { get; set; }
    }
}
