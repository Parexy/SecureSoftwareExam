using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PatientJournal.Core.Interfaces;
using PatientJournal.Infrastructure;
using PatientJournal.Infrastructure.Repositories;
using PatientJournal.Infrastructure.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7185",
                "http://localhost:5149")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<PatientJournalContext>(options =>
{
    options.UseInMemoryDatabase("PatientJournalDb");
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/patient-journal";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewPatients", policy =>
        policy.RequireRole("Admin", "Doctor", "Nurse", "Receptionist"));

    options.AddPolicy("CanViewOwnPatientProfile", policy =>
        policy.RequireRole("Patient"));

    options.AddPolicy("CanViewJournalEntries", policy =>
        policy.RequireRole("Admin", "Doctor", "Nurse"));

    options.AddPolicy("CanCreateJournalEntry", policy =>
        policy.RequireRole("Doctor"));

    options.AddPolicy("CanViewDocumentsAsStaff", policy =>
        policy.RequireRole("Admin", "Doctor", "Nurse"));

    options.AddPolicy("CanUploadDocuments", policy =>
        policy.RequireRole("Admin", "Doctor", "Nurse"));

    options.AddPolicy("CanDeleteDocuments", policy =>
        policy.RequireRole("Admin", "Doctor"));

    options.AddPolicy("CanViewOwnDocuments", policy =>
        policy.RequireRole("Patient"));

    options.AddPolicy("CanViewAuditLogs", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();