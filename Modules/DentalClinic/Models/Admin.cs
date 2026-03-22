using System.ComponentModel.DataAnnotations;


namespace clinical.APIs.Modules.DentalClinic.Models
{
    public class Admin
    {
        [Key]
        public int Admin_ID { get; set; }
        public string Name {  get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }






    }
}
