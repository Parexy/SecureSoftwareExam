using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddSession();

builder.Services.AddHttpClient("PatientJournalApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7145");
});

// Keycloak browser login
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();