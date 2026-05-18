namespace PatientJournal.Core.Entities;

public class PatientDocument
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public Patient? Patient { get; set; }

    public int JournalEntryId { get; set; }

    public JournalEntry? JournalEntry { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    // Encrypted file content
    public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();

    public DateTime UploadedAt { get; set; }

    public List<AuditLog> AuditLogs { get; set; } = new();
}