using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<ProductStock> ProductStocks => Set<ProductStock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity => entity.ToTable("users"));
            modelBuilder.Entity<IdentityRole<int>>(entity => entity.ToTable("roles"));
            modelBuilder.Entity<IdentityUserRole<int>>(entity => entity.ToTable("user_roles"));
            modelBuilder.Entity<IdentityUserClaim<int>>(entity => entity.ToTable("user_claims"));
            modelBuilder.Entity<IdentityUserLogin<int>>(entity => entity.ToTable("user_logins"));
            modelBuilder.Entity<IdentityUserToken<int>>(entity => entity.ToTable("user_tokens"));
            modelBuilder.Entity<IdentityRoleClaim<int>>(entity => entity.ToTable("role_claims"));

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.Property(c => c.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Sku).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Price).IsRequired().HasColumnType("numeric(18,2)");
                entity.Property(p => p.Active).IsRequired();
                entity.Property(p => p.CreatedAt).IsRequired();

                entity.HasIndex(p => p.Sku).IsUnique();

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.ToTable("stock_movements");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Type).IsRequired();
                entity.Property(m => m.Quantity).IsRequired();
                entity.Property(m => m.Reason).HasMaxLength(300);
                entity.Property(m => m.MovementDate).IsRequired();

                entity.HasOne(m => m.Product)
                    .WithMany(p => p.StockMovements)
                    .HasForeignKey(m => m.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Branch)
                    .WithMany(b => b.StockMovements)
                    .HasForeignKey(m => m.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("suppliers");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
                entity.Property(s => s.Document).HasMaxLength(20);
                entity.Property(s => s.Email).HasMaxLength(150);
                entity.Property(s => s.Phone).HasMaxLength(20);
                entity.Property(s => s.Address).HasMaxLength(300);
                entity.Property(s => s.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<Branch>(entity =>
            {
                entity.ToTable("branches");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(150);
                entity.Property(b => b.Address).HasMaxLength(300);
                entity.Property(b => b.Active).IsRequired();
                entity.Property(b => b.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<ProductStock>(entity =>
            {
                entity.ToTable("product_stocks");
                entity.HasKey(ps => ps.Id);
                entity.Property(ps => ps.Quantity).IsRequired();
                entity.Property(ps => ps.MinimumQuantity).IsRequired();

                entity.HasIndex(ps => new { ps.ProductId, ps.BranchId }).IsUnique();

                entity.HasOne(ps => ps.Product)
                    .WithMany(p => p.ProductStocks)
                    .HasForeignKey(ps => ps.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ps => ps.Branch)
                    .WithMany(b => b.ProductStocks)
                    .HasForeignKey(ps => ps.BranchId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
