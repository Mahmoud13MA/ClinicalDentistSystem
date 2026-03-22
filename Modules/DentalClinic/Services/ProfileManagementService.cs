
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public class ProfileManagementService(AppDbContext context) : IProfileManagementService
    {
        

      public async Task<(bool IsSuccess, string ErrorMessage)> UpdateDoctorInfoAsync(int id, UpdateStaffInfoRequest request)
      {
          var doctor = await context.Doctors.FirstOrDefaultAsync(d=>d.ID == id);

          if (doctor == null)  return (false, "Doctor not found");

          bool hasRecords = await context.Appointments.AnyAsync(a => a.Doctor_ID == id);

            if(hasRecords && !string.IsNullOrEmpty(request.Name))
                 return (false, "Doctor has active records. Cannot modify Name. Only Phone number updates allowed.");
            

            if(!hasRecords  && !string.IsNullOrEmpty(request.Name))
                doctor.Name = request.Name;

            if(!string.IsNullOrEmpty(request.Phone))
                doctor.Phone = request.Phone;

            await context.SaveChangesAsync();

            return (true,string.Empty);


        }

       public async Task<(bool IsSuccess, string ErrorMessage)> UpdateNurseInfoAsync(int id, UpdateStaffInfoRequest request)
        {

            var nurse = await context.Nurses.FirstOrDefaultAsync(n => n.NURSE_ID == id);

            if (nurse == null) return (false, "Nurse not found");

            bool hasRecords = await context.Appointments.AnyAsync(a => a.Nurse_ID == id);

            if (hasRecords && !string.IsNullOrEmpty(request.Name)) return (false, "Nurse has active records. Cannot modify Name. Only Phone number updates allowed.");

            if(!hasRecords && !string.IsNullOrEmpty(request.Name)) nurse.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Phone))
                nurse.Phone = request.Phone;

            await context.SaveChangesAsync();

            return (true, string.Empty);


        }

       public async Task<(bool IsSuccess, string ErrorMessage)> UpdatePatientInfoAsync(int id, UpdatePatientInfoRequest request)
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p=>p.Patient_ID == id);

            if(patient == null) return (false, "Patient not found");

            bool hasRecords = await context.Appointments.AnyAsync(a => a.Patient_ID == id) || await context.EHRs.AnyAsync(e => e.Patient_ID == id);

        

            bool checkRequest = !string.IsNullOrEmpty(request.First)  || !string.IsNullOrEmpty(request.Middle) || !string.IsNullOrEmpty(request.Last)
                || !string.IsNullOrEmpty(request.Gender)  || request.DOB.HasValue ;

            if (hasRecords && checkRequest)
            {
                return (false, "Patient has medical records. To preserve historical accuracy, only Phone Number can be updated.");
            }



            if (!hasRecords)
            {
                if (!string.IsNullOrEmpty(request.First)) patient.First = request.First;
                if (!string.IsNullOrEmpty(request.Middle)) patient.Middle = request.Middle;
                if (!string.IsNullOrEmpty(request.Last)) patient.Last = request.Last;
                if (!string.IsNullOrEmpty(request.Gender)) patient.Gender = request.Gender;
                if (request.DOB.HasValue) patient.DOB = request.DOB.Value;
            }



            if (!string.IsNullOrEmpty(request.Phone)) patient.Phone = request.Phone;


            await context.SaveChangesAsync();
            return (true,string.Empty);

        }





       public async Task<(bool IsSuccess, string ErrorMessage)> DeletePatientAsync(int id)
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.Patient_ID == id);

            if (patient == null) return (false, "Patient not found");

            bool hasRecords = await context.Appointments.AnyAsync(a => a.Patient_ID == id) || await context.EHRs.AnyAsync(e => e.Patient_ID == id);
            if (hasRecords) return (false, "Cannot delete patient because they have active medical records or appointments.");

            context.Patients.Remove(patient);
            await context.SaveChangesAsync();
            return (true, string.Empty);










        }



    }








}
