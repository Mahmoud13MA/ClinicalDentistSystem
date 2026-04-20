using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class SupplyCreateRequest
    {
        [Required(ErrorMessage = "Supply_Name is required")]
        public string Supply_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or greater")]
        public int Quantity { get; set; }

        public string? Description { get; set; }
    }
}