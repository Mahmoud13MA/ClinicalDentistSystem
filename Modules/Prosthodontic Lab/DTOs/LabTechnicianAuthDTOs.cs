using System.ComponentModel.DataAnnotations;

namespace clinical.APIs.Modules.ProsthodonticLab.DTOs
{
    public class LabTechnicianRegisterRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string RegistrationKey { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Specialization { get; set; } = string.Empty;
    }

    public class LabTechnicianLoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LabTechnicianLoginResponse
    {
        public int LabTechnicianID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
