using clinical.APIs.Data;
using clinical.APIs.Services;
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
builder.Services.AddScoped<IEHRChangeLogService, EHRChangeLogService>();

// Mapping services
builder.Services.AddScoped<IAppointmentMappingService, AppointmentMappingService>();
builder.Services.AddScoped<IStockTransactionMappingService, StockTransactionMappingService>();
builder.Services.AddScoped<IEHRMappingService, EHRMappingService>();
builder.Services.AddScoped<IPatientMappingService, PatientMappingService>();
builder.Services.AddScoped<IDoctorMappingService, DoctorMappingService>();
builder.Services.AddScoped<INurseMappingService, NurseMappingService>();

// AI services
builder.Services.AddHttpClient<ILlamaService, LlamaService>();
builder.Services.AddScoped<ILlamaService, LlamaService>();
builder.Services.AddSingleton<OllamaManager>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    options.MapInboundClaims = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("NurseOnly", policy => policy.RequireRole("Nurse"));
    options.AddPolicy("DoctorOrNurse", policy => policy.RequireRole("Doctor", "Nurse"));
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
    try
    {
        Console.WriteLine("Initializing database...");
        context.Database.Migrate();
        Console.WriteLine($"✓ Database ready: {context.Database.GetDbConnection().Database}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Database error: {ex.Message}");
        Console.WriteLine("Install SQL Server LocalDB: https://aka.ms/sqlexpress");
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowNextJs");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Cleanup on shutdown
app.Lifetime.ApplicationStopping.Register(ollama.Stop);

app.Run();
