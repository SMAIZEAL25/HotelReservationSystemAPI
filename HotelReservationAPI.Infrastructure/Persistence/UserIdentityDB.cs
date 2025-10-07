using HotelReservationSystemAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace UserIdentity.Infrastructure.Persistence
{
    public class UserIdentityDB : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<User> Users { get; set; } // Explicitly exposed for queries
        public DbSet<Role> Roles { get; set; }

        public UserIdentityDB(DbContextOptions<UserIdentityDB> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

                // Optional: if you model Role as a Value Object inside User
                // (if not, skip this block)
                entity.Property(u => u.Role)
                      .HasConversion<int>() // Store Enum as integer
                      .IsRequired();
            });

            // ==============================
            // ROLE ENTITY CONFIGURATION
            // ==============================
            builder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                // Index for fast lookup
                entity.HasIndex(r => r.Name).IsUnique();
                entity.HasIndex(r => r.NormalizedName).IsUnique();

                // Field constraints
                entity.Property(r => r.Name)
                      .HasMaxLength(100)
                      .IsRequired();
                entity.Property(r => r.Description)
                      .HasMaxLength(512);
                entity.Property(r => r.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(r => r.UpdatedAt)
                      .IsRequired(false);

                // Seed roles with fixed GUIDs for consistency across migrations
                // Using anonymous types to handle private setters
                entity.HasData(
                    new
                    {
                        Id = new Guid("844eb56d-ea3a-4f71-abd5-3f648ed9d61b"), // Fixed GUID for Guest
                        Name = "Guest",
                        NormalizedName = "GUEST",
                        Description = "Default user role",
                        CreatedAt = DateTime.UtcNow,
                        ConcurrencyStamp = "fixed-concurrency-stamp-guest" // Fixed value for reproducibility
                    },
                    new
                    {
                        Id = new Guid("a1ea0823-516d-4441-9a40-16e8b7649171"), // Fixed GUID for HotelAdmin
                        Name = "HotelAdmin",
                        NormalizedName = "HOTELADMIN",
                        Description = "Administrator for hotel branch",
                        CreatedAt = DateTime.UtcNow,
                        ConcurrencyStamp = "fixed-concurrency-stamp-hoteladmin" // Fixed value for reproducibility
                    },
                    new
                    {
                        Id = new Guid("34f40151-388b-434f-8b34-910ef9c6098b"), // Fixed GUID for SuperAdmin
                        Name = "SuperAdmin",
                        NormalizedName = "SUPERADMIN",
                        Description = "Global administrator with all permissions",
                        CreatedAt = DateTime.UtcNow,
                        ConcurrencyStamp = "fixed-concurrency-stamp-superadmin" // Fixed value for reproducibility
                    }
                );
            });
        }
    }
}