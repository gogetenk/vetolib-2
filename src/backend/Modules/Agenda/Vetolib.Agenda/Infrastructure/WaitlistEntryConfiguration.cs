using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Infrastructure;

internal class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ToTable("waitlist_entries", "agenda");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClinicId)
            .IsRequired();

        builder.Property(e => e.PatientId)
            .IsRequired();

        builder.Property(e => e.OwnerName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.OwnerPhone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.OwnerEmail)
            .HasMaxLength(256);

        builder.Property(e => e.PreferredDate)
            .IsRequired();

        builder.Property(e => e.PreferredTimeSlot)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.VetPreference);

        builder.Property(e => e.Reason)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.NotifiedAt);
        builder.Property(e => e.ExpiresAt);

        // Index for waitlist matching queries: find pending entries by clinic + date
        builder.HasIndex(e => new { e.ClinicId, e.Status, e.PreferredDate });
    }
}
