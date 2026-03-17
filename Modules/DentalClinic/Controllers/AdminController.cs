
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using clinical.APIs.Modules.DentalClinic.DTOs;
using Microsoft.AspNetCore.Authorization;


namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;


        public AdminController(AppDbContext context, IPasswordHashService passwordHashService, IJwtService jwtService, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;

        }

        [HttpPost("Register")]

        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterRequest request) {



            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validRegistrationKey = _configuration["RegistrationSettings:AdminRegistrationKey"];

            if (request.AdminRegistrationKey != validRegistrationKey)
            {
                return Unauthorized(new { error = "Invalid registration key." });

            }
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var exists = await _context.Admins.AnyAsync(a => a.Email == normalizedEmail);

            if (exists) return BadRequest(new { error = "Email already registered." });



            var admin = new Admin
            { Name = request.Name,
                Email = request.Email,
                PasswordHash = _passwordHashService.HashPassword(request.Password),



            };
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            var token = _jwtService.GenerateToken(admin.Admin_ID, admin.Email, admin.Name, "Admin");
            return Ok(new { message = "Admin registered successfully.", adminId = admin.Admin_ID, token });


        }


        [HttpPost("Login")]

        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email);

            if (admin == null || !_passwordHashService.VerifyPassword(request.Password, admin.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid email or password." });

            }

            var token = _jwtService.GenerateToken(admin.Admin_ID, admin.Email, admin.Name, "Admin");

            return Ok(new { message = "Login successful", token });


        }




        [Authorize(Policy = "Admin")]
        [HttpDelete("doctors /{id:int}/credentials")]

        public async Task<IActionResult> RemoveDoctorCrednetials(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null) return NotFound(new { error = "Doctor not found" });


            doctor.Email=string.Empty;
            doctor.PasswordHash=string.Empty;
            await _context.SaveChangesAsync();

            return Ok(new {massage = "Doctor Cerdnetails removed . Doctor record kept"});


        }





        [Authorize(Policy = "Admin")]
        [HttpDelete("nurses /{id:int}/credentials")]

        public async Task<IActionResult> RemoveNurseCrednetials(int id)
        {
            var nurse = await _context.Doctors.FindAsync(id);

            if (nurse == null) return NotFound(new { error = "Nurse not found" });


            nurse.Email = string.Empty;
            nurse.PasswordHash = string.Empty;
            await _context.SaveChangesAsync();

            return Ok(new { massage = "nurse Cerdnetails removed . nurse record kept" });



        }








    }








}
