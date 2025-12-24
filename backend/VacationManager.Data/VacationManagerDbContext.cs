using Microsoft.EntityFrameworkCore;
using VacationManager.Core.Entities;

namespace VacationManager.Data;

public class VacationManagerDbContext : DbContext
{
    public VacationManagerDbContext(DbContextOptions<VacationManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Vacation> Vacations { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Integration> Integrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntraId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Department).HasMaxLength(256);
            
            entity.HasIndex(e => e.EntraId).IsUnique();
            entity.HasIndex(e => e.Email);
            
            entity.HasOne(e => e.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Vacation Configuration
        modelBuilder.Entity<Vacation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Vacations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ApprovedByUser)
                .WithMany(u => u.ApprovedVacations)
                .HasForeignKey(e => e.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });
        });

        // Team Configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        // Integration Configuration
        modelBuilder.Entity<Integration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Config).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.Type }).IsUnique();
        });
    }
}
