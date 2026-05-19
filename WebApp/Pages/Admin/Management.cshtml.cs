using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientJournal.Contracts.DTO;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.DTO;

namespace WebApp.Pages.Admin;

[Authorize(Roles = "Admin,Receptionist")]
public class ManagementModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;

    public ManagementModel(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public string ActiveView { get; set; } = "patients";

    public bool IsAdmin { get; set; }

    public List<PatientDTO> Patients { get; set; } = new();

    public List<StaffMemberDTO> StaffMembers { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public string? TemporaryPassword { get; set; }

    public async Task OnGetAsync(string? view = null)
    {
        IsAdmin = User.IsInRole("Admin");

        ActiveView = IsAdmin && string.Equals(view, "staff", StringComparison.OrdinalIgnoreCase)
            ? "staff"
            : "patients";

        var client = await CreateAuthorizedApiClientAsync();

        if (client == null)
        {
            return;
        }

        if (ActiveView == "staff" && IsAdmin)
        {
            await LoadStaffMembersAsync(client);
        }
        else
        {
            await LoadPatientsAsync(client);
        }
    }

    public async Task<IActionResult> OnPostDeletePatientAsync(int id)
    {
        IsAdmin = User.IsInRole("Admin");
        ActiveView = "patients";

        if (!IsAdmin)
        {
            return Forbid();
        }

        var client = await CreateAuthorizedApiClientAsync();

        if (client == null)
        {
            await TryReloadCurrentViewAsync();
            return Page();
        }

        var response = await client.DeleteAsync($"/api/patient/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();

            ErrorMessage = $"Could not delete patient. Status: {(int)response.StatusCode}. Response: {responseText}";
            await LoadPatientsAsync(client);

            return Page();
        }

        return RedirectToPage("/Admin/Management", new { view = "patients" });
    }

    public async Task<IActionResult> OnPostDeleteStaffAsync(int id)
    {
        IsAdmin = User.IsInRole("Admin");
        ActiveView = "staff";

        if (!IsAdmin)
        {
            return Forbid();
        }

        var client = await CreateAuthorizedApiClientAsync();

        if (client == null)
        {
            await TryReloadCurrentViewAsync();
            return Page();
        }

        var response = await client.DeleteAsync($"/api/staffmember/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();

            ErrorMessage = $"Could not delete staff member. Status: {(int)response.StatusCode}. Response: {responseText}";
            await LoadStaffMembersAsync(client);

            return Page();
        }

        return RedirectToPage("/Admin/Management", new { view = "staff" });
    }

    public async Task<IActionResult> OnPostResetStaffPasswordAsync(int id)
    {
        IsAdmin = User.IsInRole("Admin");
        ActiveView = "staff";

        if (!IsAdmin)
        {
            return Forbid();
        }

        var client = await CreateAuthorizedApiClientAsync();

        if (client == null)
        {
            await TryReloadCurrentViewAsync();
            return Page();
        }

        var response = await client.PostAsync($"/api/staffmember/{id}/reset-password", null);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();

            ErrorMessage = $"Could not reset password. Status: {(int)response.StatusCode}. Response: {responseText}";
            await LoadStaffMembersAsync(client);

            return Page();
        }

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.TryGetProperty("temporaryPassword", out var passwordElement))
        {
            TemporaryPassword = passwordElement.GetString();
        }

        SuccessMessage = "Password was reset.";
        await LoadStaffMembersAsync(client);

        return Page();
    }

    private async Task<HttpClient?> CreateAuthorizedApiClientAsync()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            ErrorMessage = "No login token was found. Please log in again.";
            return null;
        }

        var client = httpClientFactory.CreateClient("PatientJournalApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }

    private async Task TryReloadCurrentViewAsync()
    {
        var client = await CreateAuthorizedApiClientAsync();

        if (client == null)
        {
            return;
        }

        if (ActiveView == "staff" && IsAdmin)
        {
            await LoadStaffMembersAsync(client);
        }
        else
        {
            await LoadPatientsAsync(client);
        }
    }

    private async Task LoadPatientsAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/patient");

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();

            ErrorMessage = $"Could not load patients. Status: {(int)response.StatusCode}. Response: {responseText}";
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        Patients = JsonSerializer.Deserialize<List<PatientDTO>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<PatientDTO>();
    }

    private async Task LoadStaffMembersAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/staffmember");

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();

            ErrorMessage = $"Could not load staff members. Status: {(int)response.StatusCode}. Response: {responseText}";
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        StaffMembers = JsonSerializer.Deserialize<List<StaffMemberDTO>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<StaffMemberDTO>();
    }
}