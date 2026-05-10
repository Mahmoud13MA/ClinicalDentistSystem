using clinical.APIs.Modules.DentalClinic;
using clinical.APIs.Modules.DentalClinic.Services;
using clinical.APIs.Modules.ProsthodonticLab.Services;
using clinical.APIs.Modules.Radiology.Services;
using ClinicalDentistSystem.Shared.Contracts.Radiology;
using ClinicalDentistSystem.Shared.Contracts.Lab;
using ClinicalDentistSystem.Shared.Services;
using clinical.APIs.Modules.Radiology.MappingProfiles;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Filters;
using clinical.APIs.Shared.Middleware;
using clinical.APIs.Shared.Security;
using clinical.APIs.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var isRailway = Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") != null;
var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";


// ── Port / Kestrel ──────────────────────────────────────────────────────────
if (isRailway || isContainer)
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
else
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        var httpPort = FindAvailablePort(5107);
        var httpsPort = FindAvailablePort(7044);

        serverOptions.Listen(IPAddress.Loopback, httpPort);
        serverOptions.Listen(IPAddress.Loopback, httpsPort, listenOptions =>
        {
            listenOptions.UseHttps();
        });

        if (httpPort != 5107 || httpsPort != 7044)
            Console.WriteLine($"⚠ Using alternate ports - HTTP: {httpPort}, HTTPS: {httpsPort}");
    });
}

static int FindAvailablePort(int startPort)
{
    for (int port = startPort; port < startPort + 100; port++)
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            return port;
        }
        catch (SocketException)
        {
            continue;
        }
    }
    throw new InvalidOperationException($"No available ports found starting from {startPort}");
}

// ── Database ────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ── SQLite Fallback Queue ───────────────────────────────────────────────────
var sqlitePath = isRailway
    ? "Data Source=/tmp/local_fallback_queue.db"
    : "Data Source=local_fallback_queue.db";

builder.Services.AddDbContext<LocalQueueDbContext>(opts =>
    opts.UseSqlite(sqlitePath));

// ── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
        policy.WithOrigins(
            "http://localhost:3000",
            "https://localhost:3000",
            "https://your-app.railway.app") // ← replace with your Railway URL
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// ── Core Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<clinical.APIs.Shared.Services.IEmailValidationService, clinical.APIs.Shared.Services.EmailValidationService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<IFhirValidationService, FhirValidationService>();
builder.Services.AddHttpClient("LocalSyncClient");
builder.Services.AddHostedService<clinical.APIs.Shared.Services.BackgroundSyncService>();
builder.Services.AddAutoMapper(
    typeof(RadiologyMappingProfile),
    typeof(clinical.APIs.Modules.ProsthodonticLab.MappingProfiles.ProsthodonticLabMappingProfile));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddScoped<IRadiologyModule, RadiologyModuleService>();
builder.Services.AddScoped<ILabModule, LabModuleService>();
builder.Services.AddScoped<ILabFhirMappingService, LabFhirMappingService>();
builder.Services.AddSingleton<ClinicalDentistSystem.Shared.Serialization.FhirSerializationUtility>();

// ── Module Services ─────────────────────────────────────────────────────────
builder.Services.AddDentalClinicModule();

// ── JWT Authentication ───────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
    throw new InvalidOperationException("JwtSettings configuration is incomplete. SecretKey, Issuer, and Audience are required.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    options.MapInboundClaims = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole("Doctor", "Admin"));
    options.AddPolicy("NurseOnly", policy => policy.RequireRole("Nurse"));
    options.AddPolicy("DoctorOrNurse", policy => policy.RequireRole("Doctor", "Nurse"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Radiologist", policy => policy.RequireRole("Radiologist"));
    options.AddPolicy("RadiologistOrAdmin", policy => policy.RequireRole("Radiologist", "Admin"));
    options.AddPolicy("LabTechnician", policy => policy.RequireRole("LabTechnician"));
});

// ── MVC + Filters ────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<IdempotencyFilter>();
});

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName ?? type.Name);
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clinical Dentist System API",
        Version = "v1",
        Description = "API for Clinical Dentist Management System"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Ollama ───────────────────────────────────────────────────────────────────
var ollama = app.Services.GetRequiredService<OllamaManager>();
await ollama.StartWithFallbackAsync();

// ── Database Migration ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    var queueContext = scope.ServiceProvider.GetRequiredService<LocalQueueDbContext>();
    await queueContext.Database.EnsureCreatedAsync();
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<DatabaseOutageMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// HTTPS redirection — local only, Railway handles SSL at proxy level
if (!isRailway)
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowNextJs");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Map("/error", () => Results.Problem("An unexpected server error occurred."));

app.Lifetime.ApplicationStopping.Register(ollama.Stop);

app.Run();