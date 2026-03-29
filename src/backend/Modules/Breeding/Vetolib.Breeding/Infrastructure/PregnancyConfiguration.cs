using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Breeding.Domain;

namespace Vetolib.Breeding.Infrastructure;

internal class PregnancyConfiguration : IEntityTypeConfiguration<Pregnancy>
{
    public void Configure(EntityTypeBuilder<Pregnancy> builder)
    {
        builder.ToTable("pregnancies", "breeding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClinicId).IsRequired();
        builder.Property(x => x.PatientId).IsRequired();
        builder.Property(x => x.FatherPatientId);
        builder.Property(x => x.MatingDate).IsRequired();
        builder.Property(x => x.MatingMethod).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.ExpectedDueDate).IsRequired();
        builder.Property(x => x.ActualDeliveryDate);
        builder.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.OffspringCount);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasMany(x => x.ScheduledChecks)
            .WithOne()
            .HasForeignKey(c => c.PregnancyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ClinicId, x.PatientId, x.Status })
            .HasDatabaseName("ix_pregnancies_clinic_patient_status");
    }
}
