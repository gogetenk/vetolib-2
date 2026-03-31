using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Infrastructure;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.FullName)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty);

        builder.Property(u => u.VetLicenseNumber)
            .HasMaxLength(100);

        builder.Property(u => u.ClinicId)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.IsLocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.EmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailVerificationToken)
            .HasMaxLength(256);

        builder.Property(u => u.EmailVerificationExpiry);

        // Email must be globally unique (not per-clinic) — a user can't register with the same email across clinics
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Token index for fast lookup during verification
        builder.HasIndex(u => u.EmailVerificationToken)
            .HasFilter("\"EmailVerificationToken\" IS NOT NULL");

        builder.Property(u => u.ReferredByUserId);
    }
}
