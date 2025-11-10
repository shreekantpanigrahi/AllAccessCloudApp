using AllAccessApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AllAccessApp.Infrastructure.Context
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users => Set<User>();
        public DbSet<FileItem> Files => Set<FileItem>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Defining Primary Key
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<FileItem>().HasKey(f => f.Id);

            // Optional: Explicit table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<FileItem>().ToTable("Files");

            // Relationships
            modelBuilder.Entity<FileItem>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values for audit fields
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<User>()
                .Property(u => u.ModifiedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<FileItem>()
                .Property(f => f.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<FileItem>()
                .Property(f => f.ModifiedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            // Optional: Seed nothing by default.
            base.OnModelCreating(modelBuilder);
        }

        // Optional: Override SaveChanges to auto-set audit fields. 
        public override int SaveChanges() 
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(b => b.CreatedOn).CurrentValue = DateTime.UtcNow;
                    entry.Property(b => b.IsDeleted).CurrentValue = false;
                }
                entry.Property(b => b.ModifiedOn).CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
