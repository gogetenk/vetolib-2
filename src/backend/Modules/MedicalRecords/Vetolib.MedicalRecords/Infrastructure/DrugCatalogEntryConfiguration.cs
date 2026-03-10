using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class DrugCatalogEntryConfiguration : IEntityTypeConfiguration<DrugCatalogEntry>
{
    public void Configure(EntityTypeBuilder<DrugCatalogEntry> builder)
    {
        builder.ToTable("drug_catalog_entries", "medical");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.ClinicId)
            .IsRequired(false);

        builder.Property(d => d.InnName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(d => d.DisplayName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(d => d.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(d => d.InnName);
        builder.HasIndex(d => new { d.ClinicId, d.InnName }).IsUnique();

        builder.HasMany(d => d.SpeciesContraindications)
            .WithOne()
            .HasForeignKey(c => c.DrugCatalogEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Interactions)
            .WithOne()
            .HasForeignKey(i => i.DrugCatalogEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.DosageGuidelines)
            .WithOne()
            .HasForeignKey(g => g.DrugCatalogEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class SpeciesContraindicationConfiguration : IEntityTypeConfiguration<SpeciesContraindication>
{
    public void Configure(EntityTypeBuilder<SpeciesContraindication> builder)
    {
        builder.ToTable("drug_species_contraindications", "medical");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DrugCatalogEntryId)
            .IsRequired();

        builder.Property(c => c.Species)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.AlternativeDrugId)
            .IsRequired(false);
    }
}

internal class DrugInteractionConfiguration : IEntityTypeConfiguration<DrugInteraction>
{
    public void Configure(EntityTypeBuilder<DrugInteraction> builder)
    {
        builder.ToTable("drug_interactions", "medical");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.DrugCatalogEntryId)
            .IsRequired();

        builder.Property(i => i.OtherDrugId)
            .IsRequired();

        builder.Property(i => i.OtherDrugName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(i => i.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(1000);
    }
}

internal class DosageGuidelineConfiguration : IEntityTypeConfiguration<DosageGuideline>
{
    public void Configure(EntityTypeBuilder<DosageGuideline> builder)
    {
        builder.ToTable("drug_dosage_guidelines", "medical");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.DrugCatalogEntryId)
            .IsRequired();

        builder.Property(g => g.Species)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(g => g.MinDosePerKg)
            .IsRequired()
            .HasPrecision(10, 4);

        builder.Property(g => g.MaxDosePerKg)
            .IsRequired()
            .HasPrecision(10, 4);

        builder.Property(g => g.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.Route)
            .IsRequired()
            .HasMaxLength(100);
    }
}
