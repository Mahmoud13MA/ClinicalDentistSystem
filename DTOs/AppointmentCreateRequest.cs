using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.DTOs
{
    public class AppointmentCreateRequest
    {
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public TimeSpan Time { get; set; }
        
        
        [Required]
        public string Type { get; set; }
        
        [Required]
        public int Patient_ID { get; set; }
        
        [Required]
        public int Doctor_ID { get; set; }
        
        [Required]
        public int Nurse_ID { get; set; }
    }
}
