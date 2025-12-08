using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.DTOs
{
    public class AppointmentUpdateRequest
    {
        [Required(ErrorMessage = "Appointment ID is required")]
        public int Appointment_ID { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "Time is required")]
        public TimeSpan Time { get; set; }
        
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }
        
        [Required(ErrorMessage = "Patient ID is required")]
        public int Patient_ID { get; set; }
        
        [Required(ErrorMessage = "Doctor ID is required")]
        public int Doctor_ID { get; set; }
        
        [Required(ErrorMessage = "Nurse ID is required")]
        public int Nurse_ID { get; set; }
    }
}
