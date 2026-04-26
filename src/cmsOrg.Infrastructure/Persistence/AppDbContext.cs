using cmsOrg.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace cmsOrg.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserOrganisationPermission> UserOrganisationPermissions => Set<UserOrganisationPermission>();
    public DbSet<OrganisationAsset> OrganisationAssets => Set<OrganisationAsset>();
    public DbSet<OrganisationApiKey> OrganisationApiKeys => Set<OrganisationApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organisation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(128);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<UserOrganisationPermission>(e =>
        {
            e.HasKey(x => x.Id);
            // Ensuring one user has only one permission per organisation
            e.HasIndex(x => new { x.UserId, x.OrganisationId }).IsUnique();
            
            e.HasOne(x => x.Organisation)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Permission)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrganisationAsset>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.Type).IsRequired().HasMaxLength(64);
            e.HasOne(x => x.Organisation)
                .WithMany(x => x.Assets)
                .HasForeignKey(x => x.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrganisationApiKey>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).IsRequired().HasMaxLength(512);
            e.HasIndex(x => x.Key).IsUnique();
            e.HasOne(x => x.Organisation)
                .WithMany(x => x.ApiKeys)
                .HasForeignKey(x => x.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
