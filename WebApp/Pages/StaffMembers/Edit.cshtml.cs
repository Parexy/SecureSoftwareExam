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
public class EditModel : PageModel
{
    private readonly IHttpClientFactory httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public UpdateStaffMemberDTO StaffMember { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
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

        var response = await client.GetAsync($"/api/staffmember/{id}");

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Could not load staff member. Status: {(int)response.StatusCode}";
            return Page();
        }

        var json = await response.Content.ReadAsStringAsync();

        var staffMember = JsonSerializer.Deserialize<StaffMemberDTO>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (staffMember == null)
        {
            ErrorMessage = "Could not read staff member data.";
            return Page();
        }

        StaffMember = new UpdateStaffMemberDTO
        {
            Name = staffMember.Name,
            Address = staffMember.Address,
            PhoneNumber = staffMember.PhoneNumber,
            Email = staffMember.Email,
            DateOfBirth = staffMember.DateOfBirth,
            Role = staffMember.Role,
            Enabled = true
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
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

        var response = await client.PutAsync($"/api/staffmember/{id}", content);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            ErrorMessage = $"Could not update staff member. Status: {(int)response.StatusCode}. Response: {responseText}";
            return Page();
        }

        return RedirectToPage("/Admin/Management", new { view = "staff" });
    }
}