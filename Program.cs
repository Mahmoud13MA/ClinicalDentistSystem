using clinical.APIs.Modules.DentalClinic.Services;
using clinical.APIs.Modules.DentalClinic;
using clinical.APIs.Shared.Data;
using clinical.APIs.Shared.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel with fallback ports if primary ports are in use
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
    {
        Console.WriteLine($"⚠ Using alternate ports - HTTP: {httpPort}, HTTPS: {httpsPort}");
    }
});

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
        catch (SocketException) { }
    }
    throw new InvalidOperationException($"No available ports found starting from {startPort}");
}

// Configure services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null));
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// Core services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<clinical.APIs.Shared.Services.IEmailValidationService, clinical.APIs.Shared.Services.EmailValidationService>();

// Module services
builder.Services.AddDentalClinicModule();



// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
{
    throw new InvalidOperationException("JwtSettings configuration is incomplete. SecretKey, Issuer, and Audience are required.");
}

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
        ValidIssuer = issuer, // Use the validated variable
        ValidAudience = audience, // Use the validated variable
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    options.MapInboundClaims = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("NurseOnly", policy => policy.RequireRole("Nurse"));
    options.AddPolicy("DoctorOrNurse", policy => policy.RequireRole("Doctor", "Nurse"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clinical Dentist System API",
        Version = "v1",
        Description = "API for Clinical Dentist Management System with JWT Authentication and AI-Powered EHR Assistance"
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
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Start Ollama (simplified - just 2 lines!)
var ollama = app.Services.GetRequiredService<OllamaManager>();
await ollama.StartWithFallbackAsync();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

// Configure pipeline
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

app.UseHttpsRedirection();
app.UseCors("AllowNextJs");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Map("/error", () => Results.Problem("An unexpected server error occurred."));

// Cleanup on shutdown
app.Lifetime.ApplicationStopping.Register(ollama.Stop);

app.Run();
