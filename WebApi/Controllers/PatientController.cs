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
public class PatientController : ControllerBase
{
    private readonly IRepository<Patient> patientsRepository;
    private readonly IKeycloakAdminService keycloakAdminService;

    public PatientController(
        IRepository<Patient> patientsRepository,
        IKeycloakAdminService keycloakAdminService)
    {
        this.patientsRepository = patientsRepository;
        this.keycloakAdminService = keycloakAdminService;
    }

    // GET: api/patient
    [HttpGet]
    [Authorize(Policy = "CanViewPatients")]
    public async Task<IActionResult> GetPatients()
    {
        var patients = await patientsRepository.GetAllAsync();

        var result = patients
            .Select(MapToDTO)
            .ToList();

        return Ok(result);
    }

    // GET: api/patient/5
    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewPatients")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var patient = await patientsRepository.GetAsync(id);

        if (patient == null)
        {
            return NotFound();
        }

        var result = MapToDTO(patient);

        return Ok(result);
    }

    // GET: api/patient/me
    [HttpGet("me")]
    [Authorize(Policy = "CanViewOwnPatientProfile")]
    public async Task<IActionResult> GetMyPatientProfile()
    {
        var externalUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            return Unauthorized();
        }

        var patients = await patientsRepository.GetAllAsync();

        var patient = patients.FirstOrDefault(x => x.ExternalUserId == externalUserId);

        if (patient == null)
        {
            return NotFound();
        }

        var result = MapToDTO(patient);

        return Ok(result);
    }

    // POST: api/patient
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> CreatePatient([FromBody] PatientDTO patientDto)
    {
        if (patientDto == null)
        {
            return BadRequest();
        }

        var patient = new Patient
        {
            ExternalUserId = patientDto.ExternalUserId,
            CPRNumber = patientDto.CPRNumber,
            Name = patientDto.Name,
            Address = patientDto.Address,
            PhoneNumber = patientDto.PhoneNumber,
            Email = patientDto.Email,
            DateOfBirth = patientDto.DateOfBirth,
            Gender = patientDto.Gender
        };

        await patientsRepository.AddAsync(patient);

        var result = MapToDTO(patient);

        return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, result);
    }

    // PUT: api/patient/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientDTO patientDto)
    {
        if (patientDto == null)
        {
            return BadRequest();
        }

        var patient = await patientsRepository.GetAsync(id);

        if (patient == null)
        {
            return NotFound();
        }

        patient.ExternalUserId = patientDto.ExternalUserId;
        patient.CPRNumber = patientDto.CPRNumber;
        patient.Name = patientDto.Name;
        patient.Address = patientDto.Address;
        patient.PhoneNumber = patientDto.PhoneNumber;
        patient.Email = patientDto.Email;
        patient.DateOfBirth = patientDto.DateOfBirth;
        patient.Gender = patientDto.Gender;

        await patientsRepository.EditAsync(patient);

        return NoContent();
    }

    // DELETE: api/patient/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await patientsRepository.GetAsync(id);

        if (patient == null)
        {
            return NotFound();
        }

        await patientsRepository.DeleteAsync(id);

        return NoContent();
    }

    // POST: api/patient/5/reset-password
    [HttpPost("{id:int}/reset-password")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var patient = await patientsRepository.GetAsync(id);

        if (patient == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(patient.ExternalUserId))
        {
            return BadRequest("The patient is not linked to a Keycloak user.");
        }

        var temporaryPassword = GenerateTemporaryPassword();

        await keycloakAdminService.SetTemporaryPasswordAsync(
            patient.ExternalUserId,
            temporaryPassword);

        return Ok(new
        {
            TemporaryPassword = temporaryPassword
        });
    }

    private static PatientDTO MapToDTO(Patient patient)
    {
        return new PatientDTO
        {
            Id = patient.Id,
            ExternalUserId = patient.ExternalUserId,
            CPRNumber = patient.CPRNumber,
            Name = patient.Name,
            Address = patient.Address,
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender
        };
    }

    private static string GenerateTemporaryPassword()
    {
        return $"Temp-{Guid.NewGuid():N}"[..14] + "!";
    }
}