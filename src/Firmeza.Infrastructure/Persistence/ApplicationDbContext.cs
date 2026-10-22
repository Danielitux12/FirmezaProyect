using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

/// <summary>
/// Database context for the application using Entity Framework Core and ASP.NET Core Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Client)
                  .WithMany()
                  .HasForeignKey(u => u.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Client configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Email).HasMaxLength(150);
            entity.Property(c => c.Phone).HasMaxLength(50);
            entity.Property(c => c.Address).HasMaxLength(250);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        // Sale configuration
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Total).HasPrecision(18, 2);

            entity.HasOne(s => s.Client)
                  .WithMany()
                  .HasForeignKey(s => s.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(s => s.SaleDetails)
                  .WithOne(d => d.Sale)
                  .HasForeignKey(d => d.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SaleDetail configuration
        modelBuilder.Entity<SaleDetail>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.UnitPrice).HasPrecision(18, 2);
            entity.Property(d => d.Subtotal).HasPrecision(18, 2);

            entity.HasOne(d => d.Product)
                  .WithMany()
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
