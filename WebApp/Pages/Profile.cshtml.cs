using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientJournal.Contracts.DTO;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.DTO;

namespace WebApp.Pages;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;

    public ProfileModel(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public PatientDTO? Patient { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            ErrorMessage = "No login token found. Please log in again.";
            return;
        }

        var client = httpClientFactory.CreateClient("PatientJournalApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/patient/me");

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Could not load profile. Status: {(int)response.StatusCode}";
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        Patient = JsonSerializer.Deserialize<PatientDTO>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}