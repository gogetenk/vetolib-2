using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class OnboardingStateConfiguration : IEntityTypeConfiguration<OnboardingState>
{
    public void Configure(EntityTypeBuilder<OnboardingState> builder)
    {
        builder.ToTable("onboarding_states", "auth");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.ClinicId)
            .IsRequired();

        builder.Property(o => o.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.WelcomeBannerDismissed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.ChecklistDismissed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.CompletedSteps)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(o => o.StartedAt)
            .IsRequired();

        builder.Property(o => o.CompletedAt);

        builder.HasIndex(o => o.UserId)
            .IsUnique();
    }
}
