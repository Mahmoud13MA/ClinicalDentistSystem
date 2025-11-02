
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clinical.APIs.Models
{
    public class Appointment
    {
        [Key] 
        public int Appointment_ID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Ref_Num { get; set; }
        public string Type { get; set; }

       
        public int Patient_ID { get; set; }
        public Patient Patient { get; set; }

        public int Doctor_ID { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("Nurse")]
        public int Nurse_ID { get; set; }
        public Nurse Nurse { get; set; }

        // One-to-One relationship with EHR
        public EHR EHR { get; set; }
    }
}
