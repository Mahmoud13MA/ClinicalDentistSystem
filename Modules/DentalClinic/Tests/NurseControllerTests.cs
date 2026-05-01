
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Controllers;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.DentalClinic.Services;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace clinical.APIs.Modules.DentalClinic.Tests
{
    public class NurseControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly Mock<INurseMappingService> _mappingMock;
        private readonly Mock<IProfileManagementService> _profileMock;

        public NurseControllerTests()
        {
            // Use a unique in-memory DB for each test class instance to avoid cross-test pollution
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _mappingMock = new Mock<INurseMappingService>();
            _profileMock = new Mock<IProfileManagementService>();
        }

        private AppDbContext CreateContext() => new AppDbContext(_options);

        private NurseController CreateController(AppDbContext ctx) =>
            new NurseController(ctx, _mappingMock.Object, _profileMock.Object);

        [Fact]
        public async Task GetNurseById_ReturnsNotFound_WhenNoNurse()
        {
            using var ctx = CreateContext();
            var controller = CreateController(ctx);

            // Mocking the Controller context to bypass User.FindFirst logic for simple testing, 
            // but ideally we should set up ClaimsPrincipal. For now, let's just make it compile.
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claimsPrincipal }
            };

            var result = await controller.GetNurseById(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}