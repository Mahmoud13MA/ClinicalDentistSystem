using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace clinical.APIs.Modules.DentalClinic.Services
{
    public class EHRChangeLogService : IEHRChangeLogService
    {
        private readonly AppDbContext _context;

        public EHRChangeLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogCreationAsync(EHR ehr, int doctorId, string doctorName, int appointmentId)
        {
            var changeLogs = new List<EHRChangeLog>();
            var now = DateTime.Now;

            // Log all non-null fields as "Created"
            var fieldsToLog = new Dictionary<string, string?>
            {
                { nameof(ehr.Allergies), ehr.Allergies },
                { nameof(ehr.MedicalAlerts), ehr.MedicalAlerts },
                { nameof(ehr.Diagnosis), ehr.Diagnosis },
                { nameof(ehr.XRayFindings), ehr.XRayFindings },
                { nameof(ehr.PeriodontalStatus), ehr.PeriodontalStatus },
                { nameof(ehr.ClinicalNotes), ehr.ClinicalNotes },
                { nameof(ehr.Recommendations), ehr.Recommendations },
                { nameof(ehr.History), ehr.History },
                { nameof(ehr.Treatments), ehr.Treatments }
            };

            foreach (var field in fieldsToLog)
            {
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    changeLogs.Add(new EHRChangeLog
                    {
                        EHR_ID = ehr.EHR_ID,
                        FieldName = field.Key,
                        OldValue = null,
                        NewValue = field.Value,
                        ChangeType = "Created",
                        ChangedAt = now,
                        ChangedByDoctorId = doctorId,
                        ChangedByDoctorName = doctorName,
                        AppointmentId = appointmentId
                    });
                }
            }

            if (changeLogs.Any())
            {
                await _context.EHRChangeLogs.AddRangeAsync(changeLogs);
                await _context.SaveChangesAsync();
            }
        }

        public async Task LogChangesAsync(EHR oldEhr, EHR newEhr, int doctorId, string doctorName, int appointmentId)
        {
            var changeLogs = new List<EHRChangeLog>();
            var now = DateTime.Now;

            // Compare and log changes for each field
            LogFieldChange(changeLogs, nameof(oldEhr.Allergies), oldEhr.Allergies, newEhr.Allergies, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.MedicalAlerts), oldEhr.MedicalAlerts, newEhr.MedicalAlerts, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.Diagnosis), oldEhr.Diagnosis, newEhr.Diagnosis, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.XRayFindings), oldEhr.XRayFindings, newEhr.XRayFindings, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.PeriodontalStatus), oldEhr.PeriodontalStatus, newEhr.PeriodontalStatus, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.ClinicalNotes), oldEhr.ClinicalNotes, newEhr.ClinicalNotes, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.Recommendations), oldEhr.Recommendations, newEhr.Recommendations, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.History), oldEhr.History, newEhr.History, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);
            LogFieldChange(changeLogs, nameof(oldEhr.Treatments), oldEhr.Treatments, newEhr.Treatments, now, doctorId, doctorName, appointmentId, newEhr.EHR_ID);

            if (changeLogs.Any())
            {
                await _context.EHRChangeLogs.AddRangeAsync(changeLogs);
                await _context.SaveChangesAsync();
            }
        }

        private void LogFieldChange(List<EHRChangeLog> changeLogs, string fieldName, string? oldValue, string? newValue,
            DateTime changedAt, int doctorId, string doctorName, int appointmentId, int ehrId)
        {
            // Only log if values are different
            if (oldValue != newValue)
            {
                changeLogs.Add(new EHRChangeLog
                {
                    EHR_ID = ehrId,
                    FieldName = fieldName,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ChangeType = "Updated",
                    ChangedAt = changedAt,
                    ChangedByDoctorId = doctorId,
                    ChangedByDoctorName = doctorName,
                    AppointmentId = appointmentId
                });
            }
        }
    }
}
