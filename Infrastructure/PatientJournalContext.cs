using Microsoft.EntityFrameworkCore;
using PatientJournal.Core.Entities;

namespace PatientJournal.Infrastructure;

public class PatientJournalContext : DbContext
{
    public PatientJournalContext(DbContextOptions<PatientJournalContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CPRNumber)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired();

            entity.HasIndex(x => x.ExternalUserId)
                .IsUnique();

            entity.Property(x => x.ExternalUserId)
                .IsRequired();

            entity.HasMany(x => x.JournalEntries)
                .WithOne(x => x.Patient)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.PatientDocuments)
                .WithOne(x => x.Patient)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.AuditLogs)
                .WithOne(x => x.Patient)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StaffMember>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ExternalUserId)
                .IsUnique();

            entity.Property(x => x.ExternalUserId)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired();

            entity.HasMany(x => x.JournalEntries)
                .WithOne(x => x.StaffMember)
                .HasForeignKey(x => x.StaffMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired();

            entity.Property(x => x.Notes)
                .IsRequired();

            entity.HasMany(x => x.PatientDocuments)
                .WithOne(x => x.JournalEntry)
                .HasForeignKey(x => x.JournalEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.AuditLogs)
                .WithOne(x => x.JournalEntry)
                .HasForeignKey(x => x.JournalEntryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PatientDocument>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName)
                .IsRequired();

            entity.Property(x => x.FileType)
                .IsRequired();

            entity.Property(x => x.EncryptedContent)
                .IsRequired();

            entity.HasMany(x => x.AuditLogs)
                .WithOne(x => x.PatientDocument)
                .HasForeignKey(x => x.PatientDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExternalUserId)
                .IsRequired();

            entity.Property(x => x.UserRole)
                .IsRequired();

            entity.Property(x => x.Action)
                .IsRequired();
        });
    }
}