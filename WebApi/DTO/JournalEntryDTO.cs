namespace WebApi.DTO;

public class JournalEntryDTO
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    public PatientDTO? Patient { get; set; }

    public int StaffMemberId { get; set; }

    public StaffMemberDTO? StaffMember { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<PatientDocumentDTO> PatientDocuments { get; set; } = new();
}