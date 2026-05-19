namespace PatientJournal.Core.Interfaces;

public interface IKeycloakAdminService
{
    Task<List<KeycloakUserInfo>> GetUsersAsync();

    Task<KeycloakUserInfo?> GetUserAsync(string userId);

    Task<string> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string temporaryPassword,
        string roleName);

    Task UpdateUserAsync(
        string userId,
        string username,
        string email,
        string firstName,
        string lastName,
        bool enabled);

    Task DeleteUserAsync(string userId);

    Task SetTemporaryPasswordAsync(
        string userId,
        string temporaryPassword);

    Task TriggerPasswordResetAsync(string userId);

    Task AssignRealmRoleAsync(
        string userId,
        string roleName);

    Task RemoveRealmRoleAsync(
        string userId,
        string roleName);

    Task SetRealmRolesAsync(
        string userId,
        List<string> roleNames);
}

public class KeycloakUserInfo
{
    public string Id { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}