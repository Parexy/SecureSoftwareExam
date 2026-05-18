using PatientJournal.Core.Entities;
using PatientJournal.Core.Interfaces;

namespace PatientJournal.Infrastructure;

public class DbInitializer : IDbInitializer
{
    private readonly PatientJournalContext context;

    public DbInitializer(PatientJournalContext context)
    {
        this.context = context;
    }

    public async Task InitializeAsync()
    {
        await context.Database.EnsureCreatedAsync();

        if (context.Patients.Any())
        {
            return;
        }

        var staffMembers = new List<StaffMember>
        {
            new StaffMember
            {
                Id = 1,
                ExternalUserId = "keycloak-user-doctor-1",
                Name = "Dr. Test Doctor",
                Address = "Hospitalvej 1",
                PhoneNumber = "12345678",
                Email = "doctor@test.dk",
                DateOfBirth = new DateTime(1980, 1, 1),
                Role = "Doctor"
            },
            new StaffMember
            {
                Id = 2,
                ExternalUserId = "keycloak-user-nurse-1",
                Name = "Test Nurse",
                Address = "Hospitalvej 2",
                PhoneNumber = "87654321",
                Email = "nurse@test.dk",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Nurse"
            }
        };

        var patients = new List<Patient>
        {
            new Patient
            {
                Id = 1,
                CPRNumber = "0101011234",
                Name = "Test Patient",
                Address = "Patientvej 1",
                PhoneNumber = "11223344",
                Email = "patient@test.dk",
                DateOfBirth = new DateTime(2001, 1, 1),
                Gender = "Male"
            }
        };

        var journalEntries = new List<JournalEntry>
        {
            new JournalEntry
            {
                Id = 1,
                PatientId = 1,
                StaffMemberId = 1,
                Title = "Initial consultation",
                Notes = "Patient had an initial consultation.",
                CreatedAt = DateTime.UtcNow
            }
        };

        var documents = new List<PatientDocument>
        {
            new PatientDocument
            {
                Id = 1,
                PatientId = 1,
                JournalEntryId = 1,
                FileName = "test-document.pdf",
                FileType = "application/pdf",
                FileSize = 128,
                EncryptedContent = new byte[] { 1, 2, 3, 4, 5 },
                UploadedAt = DateTime.UtcNow
            }
        };

        var auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                Id = 1,
                ExternalUserId = "keycloak-user-doctor-1",
                UserRole = "Doctor",
                Action = "CreateJournalEntry",
                PatientId = 1,
                JournalEntryId = 1,
                PatientDocumentId = null,
                Timestamp = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                Success = true
            }
        };

        await context.StaffMembers.AddRangeAsync(staffMembers);
        await context.Patients.AddRangeAsync(patients);
        await context.JournalEntries.AddRangeAsync(journalEntries);
        await context.PatientDocuments.AddRangeAsync(documents);
        await context.AuditLogs.AddRangeAsync(auditLogs);

        await context.SaveChangesAsync();
    }
}