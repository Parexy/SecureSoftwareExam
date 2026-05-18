namespace PatientJournal.Core.Entities;

public class Patient
{
    public int Id { get; set; }

    // Keycloak user id
    public string ExternalUserId { get; set; } = string.Empty;

    public string CPRNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public List<JournalEntry> JournalEntries { get; set; } = new();

    public List<PatientDocument> PatientDocuments { get; set; } = new();

    public List<AuditLog> AuditLogs { get; set; } = new();
}