using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PatientJournal.Core.Interfaces;

namespace PatientJournal.Infrastructure.Services;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient httpClient;

    private const string KeycloakBaseUrl = "http://localhost:8080";
    private const string Realm = "patient-journal";

    public KeycloakAdminService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<KeycloakUserInfo>> GetUsersAsync()
    {
        var adminToken = await GetAdminTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not get Keycloak users. Status: {(int)response.StatusCode}. Response: {content}");
        }

        using var json = JsonDocument.Parse(content);

        var users = new List<KeycloakUserInfo>();

        foreach (var userElement in json.RootElement.EnumerateArray())
        {
            users.Add(new KeycloakUserInfo
            {
                Id = GetString(userElement, "id"),
                Username = GetString(userElement, "username"),
                Email = GetString(userElement, "email"),
                FirstName = GetString(userElement, "firstName"),
                LastName = GetString(userElement, "lastName"),
                Enabled = GetBool(userElement, "enabled")
            });
        }

        return users;
    }

    public async Task<KeycloakUserInfo?> GetUserAsync(string userId)
    {
        var adminToken = await GetAdminTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not get Keycloak user. Status: {(int)response.StatusCode}. Response: {content}");
        }

        using var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        return new KeycloakUserInfo
        {
            Id = GetString(root, "id"),
            Username = GetString(root, "username"),
            Email = GetString(root, "email"),
            FirstName = GetString(root, "firstName"),
            LastName = GetString(root, "lastName"),
            Enabled = GetBool(root, "enabled")
        };
    }

    public async Task<string> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string temporaryPassword,
        string roleName)
    {
        var adminToken = await GetAdminTokenAsync();

        var userId = await CreateKeycloakUserAsync(
            adminToken,
            username,
            email,
            firstName,
            lastName);

        await SetTemporaryPasswordInternalAsync(adminToken, userId, temporaryPassword);

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            await AssignRealmRoleInternalAsync(adminToken, userId, roleName);
        }

        return userId;
    }

    public async Task UpdateUserAsync(
        string userId,
        string username,
        string email,
        string firstName,
        string lastName,
        bool enabled)
    {
        var adminToken = await GetAdminTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var body = new
        {
            username,
            email,
            firstName,
            lastName,
            enabled,
            emailVerified = true
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not update Keycloak user. Status: {(int)response.StatusCode}. Response: {content}");
        }
    }

    public async Task DeleteUserAsync(string userId)
    {
        var adminToken = await GetAdminTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not delete Keycloak user. Status: {(int)response.StatusCode}. Response: {content}");
        }
    }

    public async Task SetTemporaryPasswordAsync(
        string userId,
        string temporaryPassword)
    {
        var adminToken = await GetAdminTokenAsync();

        await SetTemporaryPasswordInternalAsync(adminToken, userId, temporaryPassword);
    }

    public async Task TriggerPasswordResetAsync(string userId)
    {
        var adminToken = await GetAdminTokenAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}/execute-actions-email");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var requiredActions = new[]
        {
            "UPDATE_PASSWORD"
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(requiredActions),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not trigger password reset email. Status: {(int)response.StatusCode}. Response: {content}");
        }
    }

    public async Task AssignRealmRoleAsync(
        string userId,
        string roleName)
    {
        var adminToken = await GetAdminTokenAsync();

        await AssignRealmRoleInternalAsync(adminToken, userId, roleName);
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{KeycloakBaseUrl}/realms/master/protocol/openid-connect/token");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = "admin-cli",
            ["username"] = "admin",
            ["password"] = "admin",
            ["grant_type"] = "password"
        });

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not get Keycloak admin token. Status: {(int)response.StatusCode}. Response: {content}");
        }

        using var json = JsonDocument.Parse(content);

        if (!json.RootElement.TryGetProperty("access_token", out var tokenElement))
        {
            throw new InvalidOperationException("Keycloak response did not contain an access_token.");
        }

        return tokenElement.GetString()
            ?? throw new InvalidOperationException("Keycloak access_token was empty.");
    }

    private async Task<string> CreateKeycloakUserAsync(
        string adminToken,
        string username,
        string email,
        string firstName,
        string lastName)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var body = new
        {
            username,
            email,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            requiredActions = new[] { "UPDATE_PASSWORD" }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not create Keycloak user. Status: {(int)response.StatusCode}. Response: {content}");
        }

        var location = response.Headers.Location?.ToString();

        if (string.IsNullOrWhiteSpace(location))
        {
            throw new InvalidOperationException("Keycloak did not return a Location header for the created user.");
        }

        return location.Split('/').Last();
    }

    private async Task SetTemporaryPasswordInternalAsync(
        string adminToken,
        string userId,
        string temporaryPassword)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}/reset-password");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var body = new
        {
            type = "password",
            value = temporaryPassword,
            temporary = true
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not set temporary password. Status: {(int)response.StatusCode}. Response: {content}");
        }
    }

    private async Task AssignRealmRoleInternalAsync(
        string adminToken,
        string userId,
        string roleName)
    {
        var role = await GetRealmRoleAsync(adminToken, roleName);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}/role-mappings/realm");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var body = new[]
        {
            new
            {
                id = role.Id,
                name = role.Name
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not assign Keycloak role '{roleName}'. Status: {(int)response.StatusCode}. Response: {content}");
        }
    }

    private async Task<KeycloakRole> GetRealmRoleAsync(string adminToken, string roleName)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/roles/{roleName}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not get Keycloak role '{roleName}'. Status: {(int)response.StatusCode}. Response: {content}");
        }

        using var json = JsonDocument.Parse(content);

        var id = json.RootElement.GetProperty("id").GetString();
        var name = json.RootElement.GetProperty("name").GetString();

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException($"Keycloak role '{roleName}' did not contain id/name.");
        }

        return new KeycloakRole(id, name);
    }

    public async Task RemoveRealmRoleAsync(string userId, string roleName)
    {
        var adminToken = await GetAdminTokenAsync();

        await RemoveRealmRoleInternalAsync(adminToken, userId, roleName);
    }

    public async Task SetRealmRolesAsync(string userId, List<string> roleNames)
    {
        var adminToken = await GetAdminTokenAsync();

        var applicationRoles = new List<string>
    {
        "Admin",
        "Doctor",
        "Nurse",
        "Receptionist",
        "Patient"
    };

        foreach (var roleName in applicationRoles)
        {
            await RemoveRealmRoleInternalAsync(adminToken, userId, roleName);
        }

        foreach (var roleName in roleNames.Distinct())
        {
            await AssignRealmRoleInternalAsync(adminToken, userId, roleName);
        }
    }

    private async Task RemoveRealmRoleInternalAsync(
    string adminToken,
    string userId,
    string roleName)
    {
        var role = await GetRealmRoleAsync(adminToken, roleName);

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"{KeycloakBaseUrl}/admin/realms/{Realm}/users/{userId}/role-mappings/realm");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var body = new[]
        {
        new
        {
            id = role.Id,
            name = role.Name
        }
    };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not remove Keycloak role '{roleName}'. Status: {(int)response.StatusCode}. Response: {content}");
        }
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.GetString() ?? string.Empty;
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind == JsonValueKind.True;
    }

    private sealed record KeycloakRole(string Id, string Name);
}