using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using clinical.APIs.Modules.DentalClinic.DTOs;
using clinical.APIs.Modules.DentalClinic.Controllers;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.DentalClinic.Services;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
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
        private readonly Mock<IPasswordHashService> _passwordMock;

        public NurseControllerTests()
        {
            // Use a unique in-memory DB for each test class instance to avoid cross-test pollution
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _mappingMock = new Mock<INurseMappingService>();
            _passwordMock = new Mock<IPasswordHashService>();
        }

        private AppDbContext CreateContext() => new AppDbContext(_options);

        private NurseController CreateController(AppDbContext ctx) =>
            new NurseController(ctx, _mappingMock.Object, _passwordMock.Object);

        [Fact]
        public async Task GetNurses_ReturnsNotFound_WhenNoNurses()
        {
            using var ctx = CreateContext();
            var controller = CreateController(ctx);

            var result = await controller.GetNurses();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetNurses_ReturnsOk_WithNurses()
        {
            using var ctx = CreateContext();
            ctx.Nurses.Add(new Nurse { NURSE_ID = 1, Name = "Alice", Email = "alice@example.com", Phone = "123", PasswordHash = "hashed" });
            await ctx.SaveChangesAsync();

            // Map results to whatever DTO the mapping service would produce; tests only assert Ok result and payload not null
            _mappingMock.Setup(m => m.MapToResponseList(It.IsAny<List<Nurse>>() ))
                        .Returns((List<Nurse> nurses) =>
                            nurses.Select(n => new NurseResponse { NURSE_ID = n.NURSE_ID, Name = n.Name, Email = n.Email, Phone = n.Phone }).ToList()
                        );

            var controller = CreateController(ctx);

            var result = await controller.GetNurses();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task CreateNurse_ReturnsBadRequest_WhenEmailAlreadyRegistered()
        {
            using var ctx = CreateContext();
            ctx.Nurses.Add(new Nurse { NURSE_ID = 1, Name = "Existing", Email = "dupe@example.com", Phone = "000", PasswordHash = "existinghashed" });
            await ctx.SaveChangesAsync();

            var controller = CreateController(ctx);

            var request = new NurseCreateRequest
            {
                Name = "New",
                Email = "dupe@example.com",
                Phone = "111",
                Password = "Password123!"
            };

            var result = await controller.CreateNurse(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateNurse_ReturnsCreated_WhenValid()
        {
            using var ctx = CreateContext();

            _passwordMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed");
            _mappingMock.Setup(m => m.MapToResponse(It.IsAny<Nurse>()))
                        .Returns((Nurse n) => new NurseResponse { NURSE_ID = n.NURSE_ID, Name = n.Name, Email = n.Email, Phone = n.Phone });

            var controller = CreateController(ctx);

            var request = new NurseCreateRequest
            {
                Name = "New Nurse",
                Email = "new@example.com",
                Phone = "555",
                Password = "StrongPass123!"
            };

            var result = await controller.CreateNurse(request);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(NurseController.GetNurseById), created.ActionName);

            // Verify nurse was persisted
            var persisted = await ctx.Nurses.FirstOrDefaultAsync(n => n.Email == request.Email);
            Assert.NotNull(persisted);
            Assert.Equal("New Nurse", persisted.Name);
            Assert.Equal("hashed", persisted.PasswordHash);
        }
    }
}