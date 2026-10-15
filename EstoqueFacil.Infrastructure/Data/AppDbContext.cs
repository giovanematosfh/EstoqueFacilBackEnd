using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        private readonly ITenantProvider _tenantProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        // Lido pelos filtros globais a cada consulta (por instância do contexto).
        private int? CurrentCompanyId => _tenantProvider.CompanyId;

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
        public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<Company> Companies => Set<Company>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AssignCompanyToNewEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            AssignCompanyToNewEntities();
            return base.SaveChanges();
        }

        private void AssignCompanyToNewEntities()
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>().Where(e => e.State == EntityState.Added))
            {
                if (entry.Entity.CompanyId != 0)
                {
                    continue;
                }

                entry.Entity.CompanyId = CurrentCompanyId
                    ?? throw new InvalidOperationException($"Não há empresa no contexto para gravar {entry.Entity.GetType().Name}.");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("users");
                entity.HasOne(u => u.Company)
                    .WithMany()
                    .HasForeignKey(u => u.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<IdentityRole<int>>(entity => entity.ToTable("roles"));
            modelBuilder.Entity<IdentityUserRole<int>>(entity => entity.ToTable("user_roles"));
            modelBuilder.Entity<IdentityUserClaim<int>>(entity => entity.ToTable("user_claims"));
            modelBuilder.Entity<IdentityUserLogin<int>>(entity => entity.ToTable("user_logins"));
            modelBuilder.Entity<IdentityUserToken<int>>(entity => entity.ToTable("user_tokens"));
            modelBuilder.Entity<IdentityRoleClaim<int>>(entity => entity.ToTable("role_claims"));

            modelBuilder.Entity<Plan>(entity =>
            {
                entity.ToTable("plans");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Price).IsRequired().HasColumnType("numeric(18,2)");
                entity.Property(p => p.Active).IsRequired();
                entity.Property(p => p.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
                entity.Property(c => c.Document).HasMaxLength(20);
                entity.Property(c => c.Email).HasMaxLength(150);
                entity.Property(c => c.Phone).HasMaxLength(20);
                entity.Property(c => c.Active).IsRequired();
                entity.Property(c => c.CreatedAt).IsRequired();

                entity.HasOne(c => c.Plan)
                    .WithMany(p => p.Companies)
                    .HasForeignKey(c => c.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            ConfigureTenant<Category>(modelBuilder);
            ConfigureTenant<Product>(modelBuilder);
            ConfigureTenant<StockMovement>(modelBuilder);
            ConfigureTenant<Supplier>(modelBuilder);
            ConfigureTenant<Branch>(modelBuilder);
            ConfigureTenant<ProductStock>(modelBuilder);
            ConfigureTenant<PurchaseRequest>(modelBuilder);

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
                entity.Property(p => p.ImageFileName).HasMaxLength(100);
                entity.Property(p => p.Sku).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Price).IsRequired().HasColumnType("numeric(18,2)");
                entity.Property(p => p.Active).IsRequired();
                entity.Property(p => p.CreatedAt).IsRequired();

                entity.HasIndex(p => new { p.CompanyId, p.Sku }).IsUnique();

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
                entity.Property(m => m.RequesterName).HasMaxLength(150);
                entity.Property(m => m.Sector).HasMaxLength(100);

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
                entity.Property(ps => ps.PurchaseRequestNumber).HasMaxLength(50);

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

            modelBuilder.Entity<PurchaseRequest>(entity =>
            {
                entity.ToTable("purchase_requests");
                entity.HasKey(pr => pr.Id);
                entity.Property(pr => pr.Number).IsRequired().HasMaxLength(50);
                entity.Property(pr => pr.Quantity).IsRequired();
                entity.Property(pr => pr.RequestDate).IsRequired();
                entity.Property(pr => pr.Status).IsRequired();
                entity.Property(pr => pr.RequesterName).HasMaxLength(150);
                entity.Property(pr => pr.Notes).HasMaxLength(500);
                entity.Property(pr => pr.CreatedAt).IsRequired();

                entity.HasIndex(pr => new { pr.BranchId, pr.RequestDate });

                entity.HasOne(pr => pr.Product)
                    .WithMany()
                    .HasForeignKey(pr => pr.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pr => pr.Branch)
                    .WithMany()
                    .HasForeignKey(pr => pr.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // Cada empresa só enxerga os próprios registros; CompanyId referencia companies.
        private void ConfigureTenant<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
        {
            modelBuilder.Entity<TEntity>(entity =>
            {
                entity.HasOne<Company>()
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => e.CompanyId == CurrentCompanyId);
            });
        }
    }
}
