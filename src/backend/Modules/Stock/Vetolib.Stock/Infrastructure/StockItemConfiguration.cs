using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vetolib.Stock.Domain;

namespace Vetolib.Stock.Infrastructure;

internal class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items", "stock");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.MinThreshold).IsRequired();
        builder.Property(x => x.ExpiryDate);
        builder.Property(x => x.DrugCatalogEntryId);
        builder.HasIndex(x => x.DrugCatalogEntryId).HasDatabaseName("ix_stock_items_drug_catalog_entry_id");

        builder.HasMany<StockMovement>()
            .WithOne()
            .HasForeignKey(m => m.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance: composite indexes for common query patterns
        builder.HasIndex(x => new { x.ClinicId, x.Quantity });
        builder.HasIndex(x => new { x.ClinicId, x.ExpiryDate });
    }
}
