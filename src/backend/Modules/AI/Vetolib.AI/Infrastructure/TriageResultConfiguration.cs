using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.AI.Application.Domain;

namespace Vetolib.AI.Infrastructure;

internal class TriageResultConfiguration : IEntityTypeConfiguration<TriageResult>
{
    public void Configure(EntityTypeBuilder<TriageResult> builder)
    {
        builder.ToTable("triage_results");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ClinicId)
            .IsRequired();

        builder.Property(t => t.Symptoms)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.Species)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Breed)
            .HasMaxLength(200);

        builder.Property(t => t.SuggestedSeverity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.EstimatedDurationMinutes)
            .IsRequired();

        builder.Property(t => t.RecommendedSpecialty)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Reasoning)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.Confidence)
            .IsRequired();

        builder.Property(t => t.OverriddenSeverity)
            .HasConversion<string?>()
            .HasMaxLength(20);

        builder.Property(t => t.ModelUsed)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.PromptTokens)
            .IsRequired();

        builder.Property(t => t.CompletionTokens)
            .IsRequired();

        builder.Property(t => t.LatencyMs)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(t => t.ClinicId);
        builder.HasIndex(t => t.CreatedAt);
    }
}
