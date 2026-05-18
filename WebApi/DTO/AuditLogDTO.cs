namespace WebApi.DTO;

public class AuditLogDTO
{
    public int Id { get; set; }

    // External user id from Keycloak
    public string ExternalUserId { get; set; } = string.Empty;

    public string UserRole { get; set; } = string.Empty;

    // Example: ViewPatient, CreateJournalEntry, UploadDocument, DownloadDocument, DeleteDocument
    public string Action { get; set; } = string.Empty;

    public int? PatientId { get; set; }

    public PatientDTO? Patient { get; set; }

    public int? JournalEntryId { get; set; }

    public JournalEntryDTO? JournalEntry { get; set; }

    public int? PatientDocumentId { get; set; }

    public PatientDocumentDTO? PatientDocument { get; set; }

    public DateTime Timestamp { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public bool Success { get; set; }
}