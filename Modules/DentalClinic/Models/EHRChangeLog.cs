using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class EHRChangeLog
    {
        [Key]
        public int ChangeLog_ID { get; set; }

        // What changed
        public string FieldName { get; set; } = string.Empty;        // Name of the field that was changed
        public string? OldValue { get; set; } = string.Empty;          // Previous value
        public string? NewValue { get; set; }           // New value
        public string ChangeType { get; set; } = string.Empty;         // "Created", "Updated", "Deleted"

        // When it changed
        public DateTime ChangedAt { get; set; }

        // Who changed it
        [ForeignKey(nameof(Doctor))]
        public int ChangedByDoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public string ChangedByDoctorName { get; set; } = string.Empty; 

        // In which appointment
        [ForeignKey(nameof(Appointment))]
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        // Foreign Keys
        [ForeignKey(nameof(EHR))]
        public int EHR_ID { get; set; }
        public EHR? EHR { get; set; }
    }
}
