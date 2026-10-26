using Microsoft.AspNetCore.Mvc;
using AutoCare.Api.Middleware;
using AutoCare.Api.Data;
using Microsoft.EntityFrameworkCore;
using AutoCare.Api.Repositories;
using AutoCare.Api.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Fail fast when required secrets/configuration are absent. Supply values through
// environment variables or dotnet user-secrets, never committed appsettings files.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection. Configure it with user-secrets or ConnectionStrings__DefaultConnection.");
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key must be configured and contain at least 32 UTF-8 bytes using user-secrets or Jwt__Key.");
if (string.IsNullOrWhiteSpace(jwtSection["Issuer"]) || string.IsNullOrWhiteSpace(jwtSection["Audience"]))
    throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be configured.");

// Add services to the container with consistent model-validation errors.
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(entry => entry.Key, entry => entry.Value!.Errors.Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The supplied value is invalid." : error.ErrorMessage).ToArray());
        return new BadRequestObjectResult(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path
        });
    };
});

// Entity Framework Core + MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(
        connectionString
    ));

// Customer 
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<CustomerService>();

// Vehicle 
builder.Services.AddScoped<VehicleRepository>();
builder.Services.AddScoped<VehicleService>();

// Mechanic 
builder.Services.AddScoped<MechanicRepository>();
builder.Services.AddScoped<MechanicService>();

// ServiceType 
builder.Services.AddScoped<ServiceTypeRepository>();
builder.Services.AddScoped<ServiceTypeService>();

// Part
builder.Services.AddScoped<PartRepository>();
builder.Services.AddScoped<PartService>();

// ServiceJob
builder.Services.AddScoped<ServiceJobRepository>();
builder.Services.AddScoped<ServiceJobService>();

// Invoice
builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<InvoiceService>();

// MaintenanceRecord
builder.Services.AddScoped<MaintenanceRecordRepository>();
builder.Services.AddScoped<MaintenanceRecordService>();

// ServiceNote
builder.Services.AddScoped<ServiceNoteRepository>();
builder.Services.AddScoped<ServiceNoteService>();

// User
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<AuthService>();

// ServiceJobPart
builder.Services.AddScoped<ServiceJobPartRepository>();
builder.Services.AddScoped<ServiceJobPartService>();

// Dashboard & Reports
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<DashboardService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Enter your JWT token. Example: Bearer {token}"
        });

    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});

// JWT setup
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSection["Issuer"],

                ValidAudience =
                    jwtSection["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await AdminSeeder.SeedAsync(
        context,
        app.Configuration);
}

// Configure the HTTP request pipeline.
// Swagger is available only in the Development environment because of the below if condition.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


// .NET 10
// ASP.NET Core 10
// EF Core 10.0.11
// MySql.EntityFrameworkCore 10.0.9
// MySQL 8
// Swagger