namespace PatientJournal.Core.Entities;

public class JournalEntry
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public Patient? Patient { get; set; }

    public int StaffMemberId { get; set; }

    public StaffMember? StaffMember { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<PatientDocument> PatientDocuments { get; set; } = new();

    public List<AuditLog> AuditLogs { get; set; } = new();
}