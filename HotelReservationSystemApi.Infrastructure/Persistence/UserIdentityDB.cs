using HotelReservationSystemAPI.Domain.Domain.Entities;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace HotelReservationSystemAPI.Infrastructure.Persistence
{
    public class UserIdentityDB : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<DomainEvent> DomainEvents { get; set; }

        public UserIdentityDB(DbContextOptions<UserIdentityDB> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // DomainEvent Configuration
            builder.Entity<DomainEvent>(entity =>
            {
                entity.ToTable("DomainEvents");
                entity.HasIndex(e => e.AggregateId);
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.OccurredAt);
                entity.Property(e => e.Data).HasMaxLength(4000).IsRequired();
                entity.Property(e => e.EventType).HasMaxLength(256).IsRequired();
            });

            // Global query filter for soft delete
            builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

            // ==============================
            // USER ENTITY CONFIGURATION
            // ==============================
            builder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                // Indexes for fast lookup
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.UserName).IsUnique();
                entity.HasIndex(u => u.FullName);

                entity.Property(u => u.RefreshToken)
                .HasMaxLength(512)  // Token length
                .IsRequired(false);

                entity.Property(u => u.RefreshTokenExpiry)
                      .IsRequired(false);

                entity.HasIndex(u => u.RefreshTokenExpiry);

                // Basic field constraints
                entity.Property(u => u.FullName)
                      .HasMaxLength(256)
                      .IsRequired();
                entity.Property(u => u.EmailConfirmed)
                      .HasDefaultValue(false);
                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // ======================
                // VALUE OBJECT MAPPINGS
                // ======================
                entity.OwnsOne(u => u.EmailValueObject, email =>
                {
                    email.WithOwner().HasForeignKey("UserId");
                    email.Property(e => e.Value)
                         .HasColumnName("EmailValue")
                         .IsRequired()
                         .HasMaxLength(256);
                    email.HasIndex(e => e.Value)
                         .IsUnique();
                });

                entity.OwnsOne(u => u.PasswordValueObject, pw =>
                {
                    pw.WithOwner().HasForeignKey("UserId");
                    pw.Property(p => p.HashedValue)
                      .HasColumnName("PasswordHash")
                      .IsRequired();
                });

                // Role as enum
                entity.Property(u => u.Role)
                      .HasConversion<int>()
                      .IsRequired();
            });

            // Role Configuration
            builder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                entity.HasIndex(r => r.Name).IsUnique();
                entity.HasIndex(r => r.NormalizedName).IsUnique();

                entity.Property(r => r.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(r => r.Description)
                      .HasMaxLength(512);

                entity.Property(r => r.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(r => r.UpdatedAt)
                      .IsRequired(false);

                entity.HasMany(r => r.RolePermissions)
                      .WithOne(rp => rp.Role)
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================
            // ROLEPERMISSION CONFIG
            // ======================
            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.Property(rp => rp.Permission)
                      .HasMaxLength(256)
                      .IsRequired();
            });

            // ======================
            // SEED DATA
            // ======================
            var now = DateTime.UtcNow;

            // Roles
            builder.Entity<Role>().HasData(
                new Role
                {
                    Id = Guid.Parse("844eb56d-ea3a-4f71-abd5-3f648ed9d61b"),
                    Name = "Guest",
                    NormalizedName = "GUEST",
                    Description = "Default user role",
                    CreatedAt = now,
                    ConcurrencyStamp = "seed-guest"
                },
                new Role
                {
                    Id = Guid.Parse("a1ea0823-516d-4441-9a40-16e8b7649171"),
                    Name = "HotelAdmin",
                    NormalizedName = "HOTELADMIN",
                    Description = "Administrator for hotel branch",
                    CreatedAt = now,
                    ConcurrencyStamp = "seed-admin"
                },
                new Role
                {
                    Id = Guid.Parse("34f40151-388b-434f-8b34-910ef9c6098b"),
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "Global administrator with all permissions",
                    CreatedAt = now,
                    ConcurrencyStamp = "seed-superadmin"
                }
            );

            // Role Permissions
            builder.Entity<RolePermission>().HasData(
                new { Id = Guid.NewGuid(), RoleId = Guid.Parse("844eb56d-ea3a-4f71-abd5-3f648ed9d61b"), Permission = "read:profile" },
                new { Id = Guid.NewGuid(), RoleId = Guid.Parse("a1ea0823-516d-4441-9a40-16e8b7649171"), Permission = "read:profile" },
                new { Id = Guid.NewGuid(), RoleId = Guid.Parse("a1ea0823-516d-4441-9a40-16e8b7649171"), Permission = "write:booking" },
                new { Id = Guid.NewGuid(), RoleId = Guid.Parse("a1ea0823-516d-4441-9a40-16e8b7649171"), Permission = "read:users" },
                new { Id = Guid.NewGuid(), RoleId = Guid.Parse("34f40151-388b-434f-8b34-910ef9c6098b"), Permission = "*" }
            );
        }
    }
}
           