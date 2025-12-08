using clinical.APIs.Models;
using clinical.APIs.DTOs;

namespace clinical.APIs.Services
{
    public class AppointmentMappingService : IAppointmentMappingService
    {
        public AppointmentResponse MapToResponse(Appointment appointment)
        {
            if (appointment == null)
                return null;

            return new AppointmentResponse
            {
                Appointment_ID = appointment.Appointment_ID,
                Date = appointment.Date,
                Time = appointment.Time,
                Ref_Num = appointment.Ref_Num,
                Type = appointment.Type,
                Patient_ID = appointment.Patient_ID,
                Patient = appointment.Patient != null ? new PatientBasicInfo
                {
                    Patient_ID = appointment.Patient.Patient_ID,
                    First = appointment.Patient.First,
                    Middle = appointment.Patient.Middle,
                    Last = appointment.Patient.Last,
                    Gender = appointment.Patient.Gender,
                    DOB = appointment.Patient.DOB
                } : null,
                Doctor_ID = appointment.Doctor_ID,
                Doctor = appointment.Doctor != null ? new DoctorBasicInfo
                {
                    ID = appointment.Doctor.ID,
                    Name = appointment.Doctor.Name,
                    Phone = appointment.Doctor.Phone,
                    Email = appointment.Doctor.Email
                } : null,
                Nurse_ID = appointment.Nurse_ID,
                Nurse = appointment.Nurse != null ? new NurseBasicInfo
                {
                    NURSE_ID = appointment.Nurse.NURSE_ID,
                    Name = appointment.Nurse.Name,
                    Phone = appointment.Nurse.Phone,
                    Email = appointment.Nurse.Email
                } : null
            };
        }

        public List<AppointmentResponse> MapToResponseList(List<Appointment> appointments)
        {
            if (appointments == null)
                return new List<AppointmentResponse>();

            return appointments.Select(a => MapToResponse(a)).ToList();
        }
    }
}
