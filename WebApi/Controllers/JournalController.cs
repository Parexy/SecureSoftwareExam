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
public class JournalController : ControllerBase
{
    private readonly IRepository<JournalEntry> journalsRepository;
    private readonly IRepository<Patient> patientsRepository;
    private readonly IRepository<StaffMember> staffMembersRepository;
    public JournalController(IRepository<JournalEntry> journalsRepository, IRepository<Patient> patientsRepository, IRepository<StaffMember> staffMembersRepository)
    {
        this.journalsRepository = journalsRepository;
        this.patientsRepository = patientsRepository;
        this.staffMembersRepository = staffMembersRepository;
    }

    // GET: api/journal
    [HttpGet]
    [Authorize(Policy = "CanViewJournalEntries")]
    public async Task<IActionResult> GetAllJournals()
    {
        var journals = await journalsRepository.GetAllAsync();

        var result = journals.Select(j => new JournalEntryDTO
        {
            Id = j.Id,
            PatientId = j.PatientId,
            StaffMemberId = j.StaffMemberId,
        }).ToList();

        return Ok(result);
    }

    // GET: api/journal/me
    [HttpGet("me")]
    [Authorize(Policy = "CanViewOwnJournalEntries")]
    public async Task<IActionResult> GetMyJournals()
    {
        var externalUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            return Unauthorized();
        }

        var patients = await patientsRepository.GetAllAsync();
        var staffMembers = await staffMembersRepository.GetAllAsync();

        var patient = patients.FirstOrDefault(p => p.ExternalUserId == externalUserId);
        var staff = staffMembers.FirstOrDefault(s => s.ExternalUserId == externalUserId);

        var journals = await journalsRepository.GetAllAsync();

        var result = journals
            .Where(j =>
                (patient != null && j.PatientId == patient.Id) ||
                (staff != null && j.StaffMemberId == staff.Id)
            )
            .Select(j => new JournalEntryDTO
            {
                Id = j.Id,
                CreatedAt = j.CreatedAt,
                Title = j.Title,
                Notes = j.Notes,
            })
            .ToList();

        return Ok(result);
    }

    // GET: api/journal/5
    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewJournalEntries")]
    public async Task<IActionResult> GetJournalById(int id)
    {
        var journal = await journalsRepository.GetAsync(id);

        if (journal == null)
        {
            return NotFound();
        }

        var result = new JournalEntryDTO
        {
            Id = journal.Id,
            PatientId = journal.PatientId,
            StaffMemberId = journal.StaffMemberId
        };

        return Ok(result);
    }

    // POST: api/journal
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public async Task<IActionResult> CreateJournal([FromBody] JournalEntryDTO journalDto)
    {
        if (journalDto == null)
        {
            return BadRequest();
        }

        var journal = new JournalEntry
        {
            PatientId = journalDto.PatientId,
            StaffMemberId = journalDto.StaffMemberId
        };

        await journalsRepository.AddAsync(journal);

        var result = new JournalEntryDTO
        {
            Id = journal.Id,
            PatientId = journal.PatientId,
            StaffMemberId = journal.StaffMemberId
        };

        return CreatedAtAction(nameof(GetJournalById), new { id = journal.Id }, result);
    }

    // PUT: api/journal/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public async Task<IActionResult> UpdateJournal(int id, [FromBody] JournalEntryDTO journalDto)
    {
        var journal = await journalsRepository.GetAsync(id);

        if (journal == null)
        {
            return NotFound();
        }

        journal.PatientId = journalDto.PatientId;
        journal.StaffMemberId = journalDto.StaffMemberId;

        await journalsRepository.EditAsync(journal);

        return NoContent();
    }

    // DELETE: api/journal/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteJournal(int id)
    {
        var journal = await journalsRepository.GetAsync(id);

        if (journal == null)
        {
            return NotFound();
        }

        await journalsRepository.DeleteAsync(id);

        return NoContent();
    }
}