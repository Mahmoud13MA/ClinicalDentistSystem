using clinical.APIs.Modules.DentalClinic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Shared.Data;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/clinic/[controller]")]
    public class PatientController(AppDbContext context, IPatientMappingService mappingService, IProfileManagementService profileManagement) : ControllerBase
    {

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await context.Patients.ToListAsync();

            if (patients.Count == 0)
            {
                return NotFound(new { message = "No patients found." });
            }

            var response = mappingService.MapToResponseList(patients);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.Patient_ID == id);

            if (patient == null)
            {
                return NotFound(new { error = "Patient not found.", patient_ID = id });
            }

            var response = mappingService.MapToResponse(patient);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] PatientCreateRequest request)
        {
            var patient = new Patient
            {
                First = request.First,
                Middle = request.Middle,
                Last = request.Last,
                Gender = request.Gender,
                DOB = request.DOB,
                Phone = request.Phone
            };

            context.Patients.Add(patient);
            await context.SaveChangesAsync();

            var response = mappingService.MapToResponse(patient);
            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Patient_ID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientInfoRequest request)
        {
            var result = await profileManagement.UpdatePatientInfoAsync(id, request);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "Patient not found")
                    return NotFound(new { error = result.ErrorMessage, patient_ID = id });

                return BadRequest(new { error = result.ErrorMessage });
            }

            var updatedPatient = await context.Patients.FindAsync(id);
            var response = mappingService.MapToResponse(updatedPatient);

            return Ok(new { message = "Patient updated successfully.", patient = response });
        }
    }
}
