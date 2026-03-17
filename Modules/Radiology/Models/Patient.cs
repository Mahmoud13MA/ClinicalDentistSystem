using System.ComponentModel.DataAnnotations;

namespace Radiology.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        public ICollection<ImagingAppointment> ImagingAppointments { get; set; } = new List<ImagingAppointment>();

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
