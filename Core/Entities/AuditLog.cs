namespace PatientJournal.Core.Entities;

public class AuditLog
{
    public int Id { get; set; }

    // External user id from Keycloak
    public string ExternalUserId { get; set; } = string.Empty;

    public string UserRole { get; set; } = string.Empty;

    // Example: ViewPatient, CreateJournalEntry, UploadDocument, DownloadDocument, DeleteDocument
    public string Action { get; set; } = string.Empty;

    public int? PatientId { get; set; }

    public Patient? Patient { get; set; }

    public int? JournalEntryId { get; set; }

    public JournalEntry? JournalEntry { get; set; }

    public int? PatientDocumentId { get; set; }

    public PatientDocument? PatientDocument { get; set; }

    public DateTime Timestamp { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public bool Success { get; set; }
}