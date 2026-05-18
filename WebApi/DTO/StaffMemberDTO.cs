namespace WebApi.DTO;

public class StaffMemberDTO
{
    // Id from Keycloak / external identity provider
    public string ExternalUserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    // Example: Doctor, Nurse, Admin
    public string Role { get; set; } = string.Empty;

    public List<JournalEntryDTO> JournalEntries { get; set; } = new();
}