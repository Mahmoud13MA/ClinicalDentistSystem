
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.ComponentModel.DataAnnotations;
using clinical.APIs.Modules.DentalClinic.DTOs;


namespace clinical.APIs.Modules.DentalClinic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;


        public AdminController(AppDbContext context, IPasswordHashService passwordHashService, IJwtService jwtService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;

        }

        [HttpPost("Register")]

        public async Task<IActionResult> RegisterAdmin ([FromBody] AdminRegisterRequest request) {



            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var exists = await _context.Admins.AnyAsync(a => a.Email == request.Email);

            if (exists) return BadRequest(new { error = "Email already registered." });

            var admin = new Admin
            {
                Email = request.Email,
                PasswordHash = _passwordHashService.HashPassword(request.Password),


            };
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Admin registered successfully.", adminId = admin.Admin_ID });


        }
    }


}
