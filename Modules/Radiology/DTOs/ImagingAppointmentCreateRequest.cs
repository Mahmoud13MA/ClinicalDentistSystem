using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class ImagingAppointmentCreateRequest
    {
        [Required(ErrorMessage = "Date and time are required")]
        public DateTime Datetime { get; set; }
        
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientID { get; set; }
        
        [Required(ErrorMessage = "Radiologist ID is required")]
        public int RadiologistID { get; set; }
        
        [Required(ErrorMessage = "Equipment ID is required")]
        public int EquipmentID { get; set; }
    }
}
