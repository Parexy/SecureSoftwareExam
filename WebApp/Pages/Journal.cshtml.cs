using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.DTO;

namespace WebApp.Pages;

[Authorize]
public class JournalModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;

    public JournalModel(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public List<JournalEntryDTO> Entries { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            ErrorMessage = "No login token was found. Please log in again.";
            return;
        }

        var client = httpClientFactory.CreateClient("PatientJournalApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/journal/me");

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Could not load journals. Status: {(int)response.StatusCode}";
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        Entries = JsonSerializer.Deserialize<List<JournalEntryDTO>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<JournalEntryDTO>();
    }
}