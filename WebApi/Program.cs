using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PatientJournal.Core.Interfaces;
using PatientJournal.Infrastructure;
using PatientJournal.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<PatientJournalContext>(options =>
{
    options.UseInMemoryDatabase("PatientJournalDb");
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services
    .AddAuthentication(options =>
    {
        // Razor Pages / website login uses cookies.
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // If a browser user is not logged in, redirect to Keycloak.
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/Account/AccessDenied";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = "http://localhost:8080/realms/patient-journal";
        options.ClientId = "patient-journal-api";

        options.ResponseType = "code";
        options.RequireHttpsMetadata = false;

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "http://localhost:8080/realms/patient-journal";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
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

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Patient Journal API",
        Version = "v1"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();