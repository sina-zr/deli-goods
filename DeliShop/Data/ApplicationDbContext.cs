using Microsoft.EntityFrameworkCore;
using DeliShop.Models;

namespace DeliShop.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
            entity.Property(e => e.Stock)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });
    }
}

