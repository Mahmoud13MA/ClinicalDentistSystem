using System.ComponentModel.DataAnnotations;


namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Admin
    {
        [Key]
        public int Admin_ID { get; set; }
        public string Name {  get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;






    }
}
