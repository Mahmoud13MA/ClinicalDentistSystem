using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Models;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public class EHRMappingService : IEHRMappingService
    {
        public EHRResponse MapToResponse(EHR ehr)
        {
            if (ehr == null)
                return null;

            return new EHRResponse
            {
                EHR_ID = ehr.EHR_ID,
                // Medical Information
                Allergies = ehr.Allergies,
                MedicalAlerts = ehr.MedicalAlerts,
                // Dental Information
                Diagnosis = ehr.Diagnosis,
                XRayFindings = ehr.XRayFindings,
                PeriodontalStatus = ehr.PeriodontalStatus,
                ClinicalNotes = ehr.ClinicalNotes,
                Recommendations = ehr.Recommendations,
                History = ehr.History,
                Treatments = ehr.Treatments,
                // Metadata
                UpdatedAt = ehr.UpdatedAt,
                UpdatedBy = ehr.UpdatedBy,
                Patient_ID = ehr.Patient_ID,
                AppointmentId = ehr.AppointmentId,
                Patient = ehr.Patient != null ? new PatientBasicInfo
                {
                    Patient_ID = ehr.Patient.Patient_ID,
                    First = ehr.Patient.First,
                    Middle = ehr.Patient.Middle,
                    Last = ehr.Patient.Last,
                    Gender = ehr.Patient.Gender,
                    DOB = ehr.Patient.DOB
                } : null,
                Appointment = ehr.Appointment != null ? new AppointmentBasicInfo
                {
                    Appointment_ID = ehr.Appointment.Appointment_ID,
                    Date = ehr.Appointment.Date,
                    Time = ehr.Appointment.Time,
                    Ref_Num = ehr.Appointment.Ref_Num,
                    Type = ehr.Appointment.Type
                } : null,
                // Normalized collections
                Medications = ehr.Medications?.Select(m => new MedicationRecordResponse
                {
                    Medication_ID = m.Medication_ID,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Route = m.Route,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Notes = m.Notes
                }).ToList(),
                Procedures = ehr.Procedures?.Select(p => new ProcedureRecordResponse
                {
                    Procedure_ID = p.Procedure_ID,
                    Code = p.Code,
                    Description = p.Description,
                    PerformedAt = p.PerformedAt,
                    ToothNumber = p.ToothNumber,
                    Status = p.Status,
                    Notes = p.Notes
                }).ToList(),
                Teeth = ehr.Teeth?.Select(t => new ToothRecordResponse
                {
                    ToothRecord_ID = t.ToothRecord_ID,
                    ToothNumber = t.ToothNumber,
                    Condition = t.Condition,
                    TreatmentPlanned = t.TreatmentPlanned,
                    TreatmentCompleted = t.TreatmentCompleted,
                    Surfaces = t.Surfaces,
                    Notes = t.Notes,
                    LastUpdated = t.LastUpdated
                }).ToList(),
                XRays = ehr.XRays?.Select(x => new XRayRecordResponse
                {
                    XRay_ID = x.XRay_ID,
                    Type = x.Type,
                    Findings = x.Findings,
                    ImagePath = x.ImagePath,
                    HasImage = x.ImageData != null && x.ImageData.Length > 0,
                    TakenAt = x.TakenAt,
                    TakenBy = x.TakenBy,
                    Notes = x.Notes
                }).ToList(),
                ChangeLogs = ehr.ChangeLogs?.OrderByDescending(cl => cl.ChangedAt).Select(cl => new EHRChangeLogResponse
                {
                    ChangeLog_ID = cl.ChangeLog_ID,
                    FieldName = cl.FieldName,
                    OldValue = cl.OldValue,
                    NewValue = cl.NewValue,
                    ChangeType = cl.ChangeType,
                    ChangedAt = cl.ChangedAt,
                    ChangedByDoctorId = cl.ChangedByDoctorId,
                    ChangedByDoctorName = cl.ChangedByDoctorName,
                    AppointmentId = cl.AppointmentId,
                    EHR_ID = cl.EHR_ID
                }).ToList()
            };
        }

        public List<EHRResponse> MapToResponseList(List<EHR> ehrs)
        {
            if (ehrs == null)
                return new List<EHRResponse>();

            return ehrs.Select(e => MapToResponse(e)).ToList();
        }
    }
}
