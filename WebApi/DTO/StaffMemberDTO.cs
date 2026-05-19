namespace WebApi.DTO;

public class StaffMemberDTO
{
    public int Id { get; set; }

    public string ExternalUserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Role { get; set; } = string.Empty;
}