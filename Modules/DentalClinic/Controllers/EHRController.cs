using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using clinical.APIs.Shared.Data;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EHRController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEHRMappingService _mappingService;
        private readonly IEHRChangeLogService _changeLogService;

        public EHRController(AppDbContext context, IEHRMappingService mappingService, IEHRChangeLogService changeLogService)
        {
            _context = context;
            _mappingService = mappingService;
            _changeLogService = changeLogService;
        }

        private (int DoctorId, string DoctorName) GetDoctorFromToken()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userName = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
            {
                throw new UnauthorizedAccessException("Unable to retrieve doctor information from token");
            }

            return (int.Parse(userId), userName);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetEHR()
        {
            var ehrs = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .Include(e => e.Medications)
                .Include(e => e.Procedures)
                .Include(e => e.Teeth)
                .Include(e => e.XRays)
                .Include(e => e.ChangeLogs)
                .ToListAsync();

            if (ehrs == null || ehrs.Count == 0)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponseList(ehrs);
            return Ok(response);
        }

        [HttpGet("{EHR_ID}")]
        public async Task<IActionResult> GetEHRById(int EHR_ID)
        {
            var ehr = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .Include(e => e.Medications)
                .Include(e => e.Procedures)
                .Include(e => e.Teeth)
                .Include(e => e.XRays)
                .Include(e => e.ChangeLogs)
                .FirstOrDefaultAsync(e => e.EHR_ID == EHR_ID);

            if (ehr == null)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponse(ehr);
            return Ok(response);
        }

        [HttpGet("patient/{Patient_ID}")]
        public async Task<IActionResult> GetEHRByPatientId(int Patient_ID)
        {
            var ehrs = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .Include(e => e.Medications)
                .Include(e => e.Procedures)
                .Include(e => e.Teeth)
                .Include(e => e.XRays)
                .Include(e => e.ChangeLogs)
                .Where(e => e.Patient_ID == Patient_ID)
                .ToListAsync();

            if (ehrs == null || ehrs.Count == 0)
            {
                return NotFound();
            }

            var response = _mappingService.MapToResponseList(ehrs);
            return Ok(response);
        }

        [HttpGet("{EHR_ID}/history")]
        public async Task<IActionResult> GetEHRChangeHistory(int EHR_ID)
        {
            var ehr = await _context.EHRs.FindAsync(EHR_ID);
            if (ehr == null)
            {
                return NotFound(new { error = "EHR not found.", ehr_id = EHR_ID });
            }

            var changeLogs = await _context.EHRChangeLogs
                .Where(cl => cl.EHR_ID == EHR_ID)
                .OrderByDescending(cl => cl.ChangedAt)
                .Select(cl => new EHRChangeLogResponse
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
                })
                .ToListAsync();

            return Ok(changeLogs);
        }

        [Authorize(Policy = "DoctorOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateEHR([FromBody] EHRCreateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "EHR data is required.", hint = "Make sure you're sending a valid JSON body with EHR information." });
            }

            var (doctorId, doctorName) = GetDoctorFromToken();

            var patient = await _context.Patients.FindAsync(request.Patient_ID);
            if (patient == null)
            {
                return BadRequest(new { error = "Patient not found.", patient_ID = request.Patient_ID });
            }

            var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
            if (appointment == null)
            {
                return BadRequest(new { error = "Appointment not found.", appointment_id = request.AppointmentId });
            }

            var ehr = new EHR
            {
                Allergies = request.Allergies,
                MedicalAlerts = request.MedicalAlerts,
                Diagnosis = request.Diagnosis,
                XRayFindings = request.XRayFindings,
                PeriodontalStatus = request.PeriodontalStatus,
                ClinicalNotes = request.ClinicalNotes,
                Recommendations = request.Recommendations,
                History = request.History,
                Treatments = request.Treatments,
                Patient_ID = request.Patient_ID,
                AppointmentId = request.AppointmentId,
                UpdatedBy = doctorName,
                UpdatedAt = DateTime.Now,
                Medications = request.Medications?.Select(m => new MedicationRecord
                {
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Route = m.Route,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Notes = m.Notes
                }).ToList(),
                Procedures = request.Procedures?.Select(p => new ProcedureRecord
                {
                    Code = p.Code,
                    Description = p.Description,
                    PerformedAt = p.PerformedAt,
                    ToothNumber = p.ToothNumber,
                    Status = p.Status,
                    Notes = p.Notes
                }).ToList(),
                Teeth = request.Teeth?.Select(t => new ToothRecord
                {
                    ToothNumber = t.ToothNumber,
                    Condition = t.Condition,
                    TreatmentPlanned = t.TreatmentPlanned,
                    TreatmentCompleted = t.TreatmentCompleted,
                    Surfaces = t.Surfaces,
                    Notes = t.Notes,
                    LastUpdated = DateTime.Now
                }).ToList(),
                XRays = request.XRays?.Select(x => new XRayRecord
                {
                    Type = x.Type,
                    Findings = x.Findings,
                    ImagePath = x.ImagePath,
                    TakenAt = x.TakenAt,
                    TakenBy = x.TakenBy,
                    Notes = x.Notes
                }).ToList()
            };

            _context.EHRs.Add(ehr);
            await _context.SaveChangesAsync();

            await _changeLogService.LogCreationAsync(ehr, doctorId, doctorName, request.AppointmentId);

            var createdEHR = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .Include(e => e.Medications)
                .Include(e => e.Procedures)
                .Include(e => e.Teeth)
                .Include(e => e.XRays)
                .Include(e => e.ChangeLogs)
                .FirstOrDefaultAsync(e => e.EHR_ID == ehr.EHR_ID);

            var response = _mappingService.MapToResponse(createdEHR);
            return CreatedAtAction(nameof(GetEHRById), new { EHR_ID = ehr.EHR_ID }, response);
        }

        [Authorize(Policy = "DoctorOnly")]
        [HttpPut("{EHR_ID}")]
        public async Task<IActionResult> UpdateEHR(int EHR_ID, [FromBody] EHRUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "EHR data is required." });
            }

            if (EHR_ID != request.EHR_ID)
            {
                return BadRequest(new { error = "EHR ID mismatch.", hint = "The ID in the URL must match the ID in the request body." });
            }

            var (doctorId, doctorName) = GetDoctorFromToken();

            var existingEHR = await _context.EHRs.AsNoTracking().FirstOrDefaultAsync(e => e.EHR_ID == EHR_ID);
            if (existingEHR == null)
            {
                return NotFound(new { error = "EHR not found.", ehr_id = EHR_ID });
            }

            if (existingEHR.Patient_ID != request.Patient_ID)
            {
                var patient = await _context.Patients.FindAsync(request.Patient_ID);
                if (patient == null)
                {
                    return BadRequest(new { error = "Patient not found.", patient_ID = request.Patient_ID });
                }
            }

            if (existingEHR.AppointmentId != request.AppointmentId)
            {
                var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
                if (appointment == null)
                {
                    return BadRequest(new { error = "Appointment not found.", appointment_id = request.AppointmentId });
                }
            }

            var trackedEHR = await _context.EHRs
                .Include(e => e.Medications)
                .Include(e => e.Procedures)
                .Include(e => e.Teeth)
                .Include(e => e.XRays)
                .Include(e => e.ChangeLogs)
                .FirstOrDefaultAsync(e => e.EHR_ID == EHR_ID);

            var oldEHR = new EHR
            {
                EHR_ID = existingEHR.EHR_ID,
                Allergies = existingEHR.Allergies,
                MedicalAlerts = existingEHR.MedicalAlerts,
                Diagnosis = existingEHR.Diagnosis,
                XRayFindings = existingEHR.XRayFindings,
                PeriodontalStatus = existingEHR.PeriodontalStatus,
                ClinicalNotes = existingEHR.ClinicalNotes,
                Recommendations = existingEHR.Recommendations,
                History = existingEHR.History,
                Treatments = existingEHR.Treatments
            };

            var newEHR = new EHR
            {
                EHR_ID = EHR_ID,
                Allergies = request.Allergies,
                MedicalAlerts = request.MedicalAlerts,
                Diagnosis = request.Diagnosis,
                XRayFindings = request.XRayFindings,
                PeriodontalStatus = request.PeriodontalStatus,
                ClinicalNotes = request.ClinicalNotes,
                Recommendations = request.Recommendations,
                History = request.History,
                Treatments = request.Treatments
            };

            await _changeLogService.LogChangesAsync(oldEHR, newEHR, doctorId, doctorName, request.AppointmentId);

            trackedEHR.Allergies = request.Allergies;
            trackedEHR.MedicalAlerts = request.MedicalAlerts;
            trackedEHR.Diagnosis = request.Diagnosis;
            trackedEHR.XRayFindings = request.XRayFindings;
            trackedEHR.PeriodontalStatus = request.PeriodontalStatus;
            trackedEHR.ClinicalNotes = request.ClinicalNotes;
            trackedEHR.Recommendations = request.Recommendations;
            trackedEHR.History = request.History;
            trackedEHR.Treatments = request.Treatments;
            trackedEHR.Patient_ID = request.Patient_ID;
            trackedEHR.AppointmentId = request.AppointmentId;
            trackedEHR.UpdatedAt = DateTime.Now;
            trackedEHR.UpdatedBy = doctorName;

            if (trackedEHR.Medications != null)
                _context.MedicationRecords.RemoveRange(trackedEHR.Medications);
            if (trackedEHR.Procedures != null)
                _context.ProcedureRecords.RemoveRange(trackedEHR.Procedures);
            if (trackedEHR.Teeth != null)
                _context.ToothRecords.RemoveRange(trackedEHR.Teeth);
            if (trackedEHR.XRays != null)
                _context.XRayRecords.RemoveRange(trackedEHR.XRays);

            if (request.Medications != null)
            {
                trackedEHR.Medications = request.Medications.Select(m => new MedicationRecord
                {
                    EHR_ID = EHR_ID,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Route = m.Route,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Notes = m.Notes
                }).ToList();
            }

            if (request.Procedures != null)
            {
                trackedEHR.Procedures = request.Procedures.Select(p => new ProcedureRecord
                {
                    EHR_ID = EHR_ID,
                    Code = p.Code,
                    Description = p.Description,
                    PerformedAt = p.PerformedAt,
                    ToothNumber = p.ToothNumber,
                    Status = p.Status,
                    Notes = p.Notes
                }).ToList();
            }

            if (request.Teeth != null)
            {
                trackedEHR.Teeth = request.Teeth.Select(t => new ToothRecord
                {
                    EHR_ID = EHR_ID,
                    ToothNumber = t.ToothNumber,
                    Condition = t.Condition,
                    TreatmentPlanned = t.TreatmentPlanned,
                    TreatmentCompleted = t.TreatmentCompleted,
                    Surfaces = t.Surfaces,
                    Notes = t.Notes,
                    LastUpdated = DateTime.Now
                }).ToList();
            }

            if (request.XRays != null)
            {
                trackedEHR.XRays = request.XRays.Select(x => new XRayRecord
                {
                    EHR_ID = EHR_ID,
                    Type = x.Type,
                    Findings = x.Findings,
                    ImagePath = x.ImagePath,
                    TakenAt = x.TakenAt,
                    TakenBy = x.TakenBy,
                    Notes = x.Notes
                }).ToList();
            }

            _context.EHRs.Update(trackedEHR);
            await _context.SaveChangesAsync();

            var updatedEHR = await _context.EHRs
                .Include(e => e.Patient)
                .Include(e => e.Appointment)
                .Include(e => e.Medications)
                .Include(e => e.Procedures)
                .Include(e => e.Teeth)
                .Include(e => e.XRays)
                .Include(e => e.ChangeLogs)
                .FirstOrDefaultAsync(e => e.EHR_ID == EHR_ID);

            var response = _mappingService.MapToResponse(updatedEHR);
            return Ok(new { message = "EHR updated successfully.", ehr = response });
        }
    }
}
