using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.DTO;

namespace WebApp.Pages.StaffMembers;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public CreateStaffMemberDTO StaffMember { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public string? TemporaryPassword { get; set; }

    public void OnGet()
    {
        StaffMember.DateOfBirth = DateTime.Today.AddYears(-30);
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

        var json = JsonSerializer.Serialize(StaffMember);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/staffmember", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            ErrorMessage = $"Could not create staff member. Status: {(int)response.StatusCode}. Response: {responseText}";
            return Page();
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(responseJson);

        if (document.RootElement.TryGetProperty("temporaryPassword", out var passwordElement))
        {
            TemporaryPassword = passwordElement.GetString();
        }

        return Page();
    }
}