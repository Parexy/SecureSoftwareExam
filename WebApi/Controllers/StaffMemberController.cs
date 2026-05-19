using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientJournal.Core.Entities;
using PatientJournal.Core.Interfaces;
using WebApi.DTO;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Roles = "Admin")]
public class StaffMemberController : ControllerBase
{
    private readonly IRepository<StaffMember> staffMemberRepository;
    private readonly IKeycloakAdminService keycloakAdminService;

    public StaffMemberController(
        IRepository<StaffMember> staffMemberRepository,
        IKeycloakAdminService keycloakAdminService)
    {
        this.staffMemberRepository = staffMemberRepository;
        this.keycloakAdminService = keycloakAdminService;
    }

    // GET: api/staffmember
    [HttpGet]
    public async Task<IActionResult> GetStaffMembers()
    {
        var staffMembers = await staffMemberRepository.GetAllAsync();

        var result = staffMembers
            .Select(MapToDTO)
            .ToList();

        return Ok(result);
    }

    // GET: api/staffmember/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStaffMember(int id)
    {
        var staffMember = await staffMemberRepository.GetAsync(id);

        if (staffMember == null)
        {
            return NotFound();
        }

        return Ok(MapToDTO(staffMember));
    }

    // POST: api/staffmember
    [HttpPost]
    public async Task<IActionResult> CreateStaffMember([FromBody] CreateStaffMemberDTO dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        if (!IsValidStaffRole(dto.Role))
        {
            return BadRequest("Invalid staff role. Valid roles are: Admin, Doctor, Nurse, Receptionist.");
        }

        var temporaryPassword = GenerateTemporaryPassword();

        var firstName = GetFirstName(dto.Name);
        var lastName = GetLastName(dto.Name);

        var keycloakUserId = await keycloakAdminService.CreateUserAsync(
            dto.Username,
            dto.Email,
            firstName,
            lastName,
            temporaryPassword,
            dto.Role);

        var staffMember = new StaffMember
        {
            ExternalUserId = keycloakUserId,
            Name = dto.Name,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            Role = dto.Role
        };

        await staffMemberRepository.AddAsync(staffMember);

        return CreatedAtAction(
            nameof(GetStaffMember),
            new { id = staffMember.Id },
            new
            {
                StaffMember = MapToDTO(staffMember),
                TemporaryPassword = temporaryPassword
            });
    }

    // PUT: api/staffmember/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStaffMember(int id, [FromBody] UpdateStaffMemberDTO dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        if (!IsValidStaffRole(dto.Role))
        {
            return BadRequest("Invalid staff role. Valid roles are: Admin, Doctor, Nurse, Receptionist.");
        }

        var staffMember = await staffMemberRepository.GetAsync(id);

        if (staffMember == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(staffMember.ExternalUserId))
        {
            return BadRequest("The staff member is not linked to a Keycloak user.");
        }

        var firstName = GetFirstName(dto.Name);
        var lastName = GetLastName(dto.Name);

        await keycloakAdminService.UpdateUserAsync(
            staffMember.ExternalUserId,
            dto.Username,
            dto.Email,
            firstName,
            lastName,
            dto.Enabled);

        await keycloakAdminService.SetRealmRolesAsync(
            staffMember.ExternalUserId,
            new List<string> { dto.Role });

        staffMember.Name = dto.Name;
        staffMember.Address = dto.Address;
        staffMember.PhoneNumber = dto.PhoneNumber;
        staffMember.Email = dto.Email;
        staffMember.DateOfBirth = dto.DateOfBirth;
        staffMember.Role = dto.Role;

        await staffMemberRepository.EditAsync(staffMember);

        return NoContent();
    }

    // DELETE: api/staffmember/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStaffMember(int id)
    {
        var staffMember = await staffMemberRepository.GetAsync(id);

        if (staffMember == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(staffMember.ExternalUserId))
        {
            await keycloakAdminService.DeleteUserAsync(staffMember.ExternalUserId);
        }

        await staffMemberRepository.DeleteAsync(id);

        return NoContent();
    }

    // POST: api/staffmember/5/reset-password
    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var staffMember = await staffMemberRepository.GetAsync(id);

        if (staffMember == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(staffMember.ExternalUserId))
        {
            return BadRequest("The staff member is not linked to a Keycloak user.");
        }

        var temporaryPassword = GenerateTemporaryPassword();

        await keycloakAdminService.SetTemporaryPasswordAsync(
            staffMember.ExternalUserId,
            temporaryPassword);

        return Ok(new
        {
            TemporaryPassword = temporaryPassword
        });
    }

    private static StaffMemberDTO MapToDTO(StaffMember staffMember)
    {
        return new StaffMemberDTO
        {
            Id = staffMember.Id,
            ExternalUserId = staffMember.ExternalUserId,
            Name = staffMember.Name,
            Address = staffMember.Address,
            PhoneNumber = staffMember.PhoneNumber,
            Email = staffMember.Email,
            DateOfBirth = staffMember.DateOfBirth,
            Role = staffMember.Role
        };
    }

    private static bool IsValidStaffRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        var validRoles = new[]
        {
            "Admin",
            "Doctor",
            "Nurse",
            "Receptionist"
        };

        return validRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return string.Empty;
        }

        var nameParts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        return nameParts.Length > 0 ? nameParts[0] : fullName;
    }

    private static string GetLastName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return string.Empty;
        }

        var nameParts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        return nameParts.Length > 1 ? nameParts[1] : string.Empty;
    }

    private static string GenerateTemporaryPassword()
    {
        return $"Temp-{Guid.NewGuid():N}"[..14] + "!";
    }
}