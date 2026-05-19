using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.DTO;

namespace WebApp.Pages.Patients;

[Authorize(Roles = "Admin,Receptionist")]
public class CreateModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public CreatePatientDTO Patient { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        Patient.DateOfBirth = DateTime.Today.AddYears(-18);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            ErrorMessage = "No login token was found. Please log in again.";
            return Page();
        }

        var client = httpClientFactory.CreateClient("PatientJournalApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var json = JsonSerializer.Serialize(Patient);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/patient", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            ErrorMessage = $"Could not create patient. Status: {(int)response.StatusCode}. Response: {responseText}";
            return Page();
        }

        return RedirectToPage("/Admin/Management", new { view = "patients" });
    }
}