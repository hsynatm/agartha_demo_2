using AMMS.Core.Interfaces;
using AMMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence;

public class UserManagementDbContext : BaseDbContext
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RoleGroup> RoleGroups => Set<RoleGroup>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<UserRoleGroup> UserRoleGroups => Set<UserRoleGroup>();

    public DbSet<RoleGroupRole> RoleGroupRoles => Set<RoleGroupRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DependencyInjection.SchemaName);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.KeycloakUserId).IsUnique();
            entity.Property(user => user.KeycloakUserId).HasMaxLength(100);
            entity.Property(user => user.Username).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.OrganizationId).HasMaxLength(100).IsRequired();
            entity.Property(user => user.OrganizationName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasIndex(role => role.Code).IsUnique();
            entity.Property(role => role.Code).HasMaxLength(100).IsRequired();
            entity.Property(role => role.Name).HasMaxLength(200).IsRequired();
            entity.Property(role => role.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<RoleGroup>(entity =>
        {
            entity.ToTable("RoleGroups");
            entity.HasIndex(roleGroup => roleGroup.Code).IsUnique();
            entity.Property(roleGroup => roleGroup.Code).HasMaxLength(100).IsRequired();
            entity.Property(roleGroup => roleGroup.Name).HasMaxLength(200).IsRequired();
            entity.Property(roleGroup => roleGroup.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRoleGroup>(entity =>
        {
            entity.ToTable("UserRoleGroups");
            entity.HasKey(userRoleGroup => new { userRoleGroup.UserId, userRoleGroup.RoleGroupId });
            entity.HasOne(userRoleGroup => userRoleGroup.User)
                .WithMany(user => user.UserRoleGroups)
                .HasForeignKey(userRoleGroup => userRoleGroup.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(userRoleGroup => userRoleGroup.RoleGroup)
                .WithMany(roleGroup => roleGroup.UserRoleGroups)
                .HasForeignKey(userRoleGroup => userRoleGroup.RoleGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoleGroupRole>(entity =>
        {
            entity.ToTable("RoleGroupRoles");
            entity.HasKey(roleGroupRole => new { roleGroupRole.RoleGroupId, roleGroupRole.RoleId });
            entity.HasOne(roleGroupRole => roleGroupRole.RoleGroup)
                .WithMany(roleGroup => roleGroup.RoleGroupRoles)
                .HasForeignKey(roleGroupRole => roleGroupRole.RoleGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(roleGroupRole => roleGroupRole.Role)
                .WithMany(role => role.RoleGroupRoles)
                .HasForeignKey(roleGroupRole => roleGroupRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
