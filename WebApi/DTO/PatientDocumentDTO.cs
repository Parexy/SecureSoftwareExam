namespace WebApi.DTO;

public class PatientDocumentDTO
{
    public int PatientId { get; set; }

    public PatientDTO? Patient { get; set; }

    public int JournalEntryId { get; set; }

    public JournalEntryDTO? JournalEntry { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    // Encrypted file content
    public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();

    public DateTime UploadedAt { get; set; }

}