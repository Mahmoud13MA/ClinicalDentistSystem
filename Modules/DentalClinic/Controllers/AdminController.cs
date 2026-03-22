
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Modules.DentalClinic.DTOs;
using Microsoft.AspNetCore.Authorization;
using clinical.APIs.Shared.Services;
using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(AppDbContext context, IPasswordHashService passwordHashService, IJwtService jwtService, IConfiguration configuration, IEmailValidationService emailValidationService ,IProfileManagementService profileManagement) : ControllerBase
    {


        [HttpPost("Register")]

        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterRequest request) {



            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validRegistrationKey = configuration["RegistrationSettings:AdminRegistrationKey"];

            if (request.AdminRegistrationKey != validRegistrationKey)
            {
                return Unauthorized(new { error = "Invalid registration key." });

            }
            var exists = await emailValidationService.IsEmailUsedAsync(request.Email);

            if (exists) return BadRequest(new { error = "Email already registered by a user." });



            var admin = new Admin
            { 
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHashService.HashPassword(request.Password),

            };
            context.Admins.Add(admin);
            await context.SaveChangesAsync();
            var token = jwtService.GenerateToken(admin.Admin_ID, admin.Email, admin.Name, "Admin");
            return Ok(new { message = "Admin registered successfully.", adminId = admin.Admin_ID, token });


        }


        [HttpPost("Login")]

        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            var admin = await context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email);

            if (admin == null || !passwordHashService.VerifyPassword(request.Password, admin.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid email or password." });

            }

            var token = jwtService.GenerateToken(admin.Admin_ID, admin.Email, admin.Name, "Admin");

            return Ok(new { message = "Login successful", token });


        }



        [Authorize(Policy ="Admin")]
        [HttpPut("doctors/{id:int}/credentials")]

        public async Task<IActionResult>DoctorUpdateCredentialsRequest([FromBody] UpdateCredentialsRequest request,int id)
        {


            if (!ModelState.IsValid) return BadRequest(ModelState);
            var doctor = await context.Doctors.FindAsync(id);
            if (doctor == null) return BadRequest(new { error = "Doctor Not Found" });

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailUsed = await emailValidationService.IsEmailUsedAsync(request.Email, doctorId: id);

                if (emailUsed) return BadRequest(new { error = "Email already used by another user" });

                doctor.Email = request.Email.Trim().ToLowerInvariant();
            }
                if (!string.IsNullOrEmpty(request.Password))
                {

                    doctor.PasswordHash=passwordHashService.HashPassword(request.Password);

                }
            await context.SaveChangesAsync();

            return Ok(new { message = "Doctor Credentials Updated " });

            }


        [Authorize(Policy = "Admin")]
        [HttpPut("nurses/{id:int}/credntials")]
        public async Task<IActionResult> NurseUpdateCredentialsRequest([FromBody]UpdateCredentialsRequest request,int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var nurse = await context.Nurses.FindAsync(id);
            if (nurse == null) return BadRequest(new { error = "Nurse Not Found " });

            if (!string.IsNullOrEmpty(request.Email)) {
                var emailUsed = await emailValidationService.IsEmailUsedAsync(request.Email, nurseId: id);

                if (emailUsed) return BadRequest(new { error = "Email already used by another user" });

                nurse.Email = request.Email.Trim().ToLowerInvariant();
            }


            if (!string.IsNullOrEmpty(request.Password)) {

                nurse.PasswordHash = passwordHashService.HashPassword(request.Password);
            
            }
            await context.SaveChangesAsync();
            return Ok(new { message = "Nurse Credentials Updated" });


        }


        [Authorize(Policy = "Admin")]
        [HttpDelete("doctors /{id:int}/credentials")]

        public async Task<IActionResult> RemoveDoctorCrednetials(int id)
        {
            var doctor = await context.Doctors.FindAsync(id);

            if (doctor == null) return NotFound(new { error = "Doctor not found" });


            doctor.Email=string.Empty;
            doctor.PasswordHash=string.Empty;
            await context.SaveChangesAsync();

            return Ok(new {massage = "Doctor Cerdnetails removed . Doctor record kept"});


        }


        [Authorize(Policy = "Admin")]
        [HttpDelete("nurses /{id:int}/credentials")]

        public async Task<IActionResult> RemoveNurseCrednetials(int id)
        {
            var nurse = await context.Nurses.FindAsync(id);

            if (nurse == null) return NotFound(new { error = "Nurse not found" });


            nurse.Email = string.Empty;
            nurse.PasswordHash = string.Empty;
            await context.SaveChangesAsync();

            return Ok(new { massage = "nurse Cerdnetails removed . nurse record kept" });
        }



        [Authorize(Policy = "Admin")]
        [HttpPut("doctors/{id:int}/updateinfo")]
        public async Task<IActionResult> UpdateDoctorInfo(int id, [FromBody] UpdateStaffInfoRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await profileManagement.UpdateDoctorInfoAsync(id, request);
            if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { message = "Doctor profile updated successfully." });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("nurses/{id:int}/updateinfo")]
        public async Task<IActionResult> UpdateNurseInfo(int id, [FromBody] UpdateStaffInfoRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await profileManagement.UpdateNurseInfoAsync(id, request);
            if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { message = "Nurse profile updated successfully." });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("patients/{id:int}/updateinfo")]
        public async Task<IActionResult> UpdatePatientInfo(int id, [FromBody] UpdatePatientInfoRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await profileManagement.UpdatePatientInfoAsync(id, request);
            if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { message = "Patient profile updated successfully." });
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("patients/{id:int}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var result = await profileManagement.DeletePatientAsync(id);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "Patient not found.")
                    return NotFound(new { error = result.ErrorMessage });

                return Conflict(new { error = result.ErrorMessage });
            }

            return Ok(new { message = "Patient deleted successfully." });
        }
    }













}




