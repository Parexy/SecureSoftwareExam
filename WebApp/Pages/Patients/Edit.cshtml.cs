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
public class EditModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public UpdatePatientDTO Patient { get; set; } = new();

    public int PatientId { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        PatientId = id;

        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            ErrorMessage = "No login token was found. Please log in again.";
            return Page();
        }

        var client = httpClientFactory.CreateClient("PatientJournalApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"/api/patient/{id}");

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Could not load patient. Status: {(int)response.StatusCode}";
            return Page();
        }

        var json = await response.Content.ReadAsStringAsync();

        var patient = JsonSerializer.Deserialize<PatientDTO>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (patient == null)
        {
            ErrorMessage = "Could not read patient data.";
            return Page();
        }

        Patient = new UpdatePatientDTO
        {
            CPRNumber = patient.CPRNumber,
            Name = patient.Name,
            Address = patient.Address,
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            ExternalUserId = patient.ExternalUserId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        PatientId = id;

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

        var response = await client.PutAsync($"/api/patient/{id}", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            ErrorMessage = $"Could not update patient. Status: {(int)response.StatusCode}. Response: {responseText}";
            return Page();
        }

        return RedirectToPage("/Admin/Management", new { view = "patients" });
    }
}