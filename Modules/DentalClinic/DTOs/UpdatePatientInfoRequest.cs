using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class UpdatePatientInfoRequest
    {
        [StringLength(100)] public string? First { get; set; }
        [StringLength(100)] public string? Middle { get; set; }
        [StringLength(100)] public string? Last { get; set; }
        [StringLength(50)] public string? Gender { get; set; }
        public DateTime? DOB { get; set; }

        [Phone]
        [StringLength(20)] public string? Phone { get; set; }



    }
}
