

using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.DentalClinic.DTOs
{
    public class UpdateStaffInfoRequest
    {

        [StringLength(100)]
        public string? Name {  get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone {  get; set; }



    }
}
