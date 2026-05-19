using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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

    public async Task OnGetAsync(string? view = null)
    {
        IsAdmin = User.IsInRole("Admin");

        if (IsAdmin && string.Equals(view, "staff", StringComparison.OrdinalIgnoreCase))
        {
            ActiveView = "staff";
        }
        else
        {
            ActiveView = "patients";
        }

        var accessToken = await HttpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            ErrorMessage = "No login token was found. Please log in again.";
            return;
        }

        var client = httpClientFactory.CreateClient("PatientJournalApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

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
            ErrorMessage = $"Could not load patients. Status: {(int)response.StatusCode}";
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
            ErrorMessage = $"Could not load staff members. Status: {(int)response.StatusCode}";
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        StaffMembers = JsonSerializer.Deserialize<List<StaffMemberDTO>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<StaffMemberDTO>();
    }
}