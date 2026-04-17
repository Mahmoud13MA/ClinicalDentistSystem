using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.Radiology.DTOs
{
    public class EquipmentUpdateRequest
    {
        [Required(ErrorMessage = "Equipment ID is required")]
        public int EquipmentID { get; set; }

        [Required(ErrorMessage = "Equipment type is required")]
        [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
        public string Model { get; set; }
    }
}
