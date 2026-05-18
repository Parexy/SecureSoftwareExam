using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Needed because AccountController is a controller
builder.Services.AddControllers();

// Only keep session if you actually use it elsewhere
builder.Services.AddSession();

// HttpClient used by Razor Pages to call the API
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
            RoleClaimType = "roles"
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

// Keep this if your project was created with the newer Razor Pages template
app.MapStaticAssets();

// Map AccountController
app.MapControllers();

// Map Razor Pages and static assets
app.MapRazorPages()
   .WithStaticAssets();

app.Run();