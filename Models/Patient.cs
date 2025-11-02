using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models
{
    public class Patient
    {
        [Key]
        public int Patient_ID { get; set; }
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }

        
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<EHR> EHRs { get; set; }
    }
}


