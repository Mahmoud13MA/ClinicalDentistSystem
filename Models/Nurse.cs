
using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Models
{
    public class Nurse
    {
        [Key]
        public int NURSE_ID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
    }
}

