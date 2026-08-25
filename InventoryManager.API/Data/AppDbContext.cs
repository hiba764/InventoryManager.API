using InventoryManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<StockMovement> StockMovements { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // التصنيفات
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.HasIndex(c => c.Name)
                .IsUnique();
        });

        // المنتجات
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.HasIndex(p => p.CategoryId);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Products_Price",
                    "[Price] >= 0");

                table.HasCheckConstraint(
                    "CK_Products_Quantity",
                    "[Quantity] >= 0");

                table.HasCheckConstraint(
                    "CK_Products_MinimumStock",
                    "[MinimumStock] >= 0");
            });
        });

        // حركات المخزون
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(sm => sm.Id);

            entity.Property(sm => sm.Quantity)
                .IsRequired();

            entity.Property(sm => sm.MovementType)
                .HasConversion<int>();

            entity.Property(sm => sm.Note)
                .HasMaxLength(500);

            entity.HasIndex(sm => sm.ProductId);

            entity.HasIndex(sm => sm.UserId);

            entity.HasOne(sm => sm.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sm => sm.User)
                .WithMany(u => u.StockMovements)
                .HasForeignKey(sm => sm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_StockMovements_Quantity",
                    "[Quantity] > 0");
            });
        });

        // المستخدمون
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(u => u.Username)
                .IsUnique();
        });
    }
}