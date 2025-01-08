#nullable disable
using Microsoft.EntityFrameworkCore;

namespace WebDataXplorer.Server.Models;

public partial class SqldbWebDataXplorerContext : DbContext
{
    public SqldbWebDataXplorerContext(DbContextOptions<SqldbWebDataXplorerContext> options) : base(options) { }

    public virtual DbSet<InventoryItem> InventoryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.InventoryItemId).HasName("PK_InventoryItem");
            entity.ToTable("InventoryItem");
            entity.Property(e => e.InventoryItemId)
                .HasMaxLength(50)
                .HasColumnName("inventory_item_id");
            entity.Property(e => e.ItemName)
                .HasMaxLength(255)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemDescription)
                .HasColumnName("item_description");
            entity.Property(e => e.ItemImageUrl)
                .HasColumnName("item_image_url");
            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp");
            entity.Property(e => e.DateAdded)
                .HasColumnName("date_added");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.StockQuantity)
                .HasColumnName("stock_quantity");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("unit_price");
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}