using clinical.APIs.Data;
using clinical.APIs.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Using connection string: {connectionString}");

// Add DbContext with retry on failure
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
    
    // Enable detailed logging for development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("NurseOnly", policy => policy.RequireRole("Nurse"));
    options.AddPolicy("DoctorOrNurse", policy => policy.RequireRole("Doctor", "Nurse"));
});

builder.Services.AddControllers();

// Configure Swagger/OpenAPI with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clinical Dentist System API",
        Version = "v1",
        Description = "API for Clinical Dentist Management System with JWT Authentication"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your JWT token. Example: 'Bearer eyJhbGc...'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        Console.WriteLine("Attempting to connect to database...");
        
        // Create database and apply all migrations
        context.Database.Migrate();
        
        Console.WriteLine("✓ Database created/updated successfully!");
        Console.WriteLine($"✓ Database: {context.Database.GetDbConnection().Database}");
        Console.WriteLine($"✓ Server: {context.Database.GetDbConnection().DataSource}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Database initialization error: {ex.Message}");
        Console.WriteLine("\nTroubleshooting steps:");
        Console.WriteLine("  1. Ensure SQL Server LocalDB is installed");
        Console.WriteLine("  2. Run 'sqllocaldb start mssqllocaldb' in command prompt");
        Console.WriteLine("  3. Or install LocalDB from: https://aka.ms/sqlexpress");
        Console.WriteLine($"\nFull error details:\n{ex}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
