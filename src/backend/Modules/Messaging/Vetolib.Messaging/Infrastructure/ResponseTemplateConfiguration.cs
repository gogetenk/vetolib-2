using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Infrastructure;

internal class ResponseTemplateConfiguration : IEntityTypeConfiguration<ResponseTemplate>
{
    public void Configure(EntityTypeBuilder<ResponseTemplate> builder)
    {
        builder.ToTable("response_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ClinicId)
            .IsRequired();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.ContentEn)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.ContentAr)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(t => t.Category)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();
    }
}
