using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace clinical.APIs.Models
{
    public class EHR
    {
        [Key]
        public int EHR_ID { get; set; }

        public string Medications { get; set; }
        public string Allergies { get; set; }
        public string History { get; set; }
        public string Treatments { get; set; }
        public DateTime Last_Updated { get; set; }

        // -----------------------------
        // Foreign Keys and Navigation
        // -----------------------------

        [ForeignKey(nameof(Patient))]
        public int Patient_ID { get; set; }
        public Patient Patient { get; set; }

        [ForeignKey(nameof(Appointment))]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
    }
}
