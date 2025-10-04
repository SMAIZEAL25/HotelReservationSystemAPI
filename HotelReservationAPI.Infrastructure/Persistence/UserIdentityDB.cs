
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationAPI.Infrastructure.Persistence
{
    public class UserIdentityDB : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<User> Users { get; set; } // Explicit for domain User

        public UserIdentityDB(DbContextOptions<UserIdentityDB> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure value object owned types if needed (e.g., Email as owned)
            builder.Entity<User>(entity =>
            {
                entity.OwnsOne(u => u.EmailValueObject, email =>
                {
                    email.WithOwner().HasForeignKey("UserId");
                    email.Property(e => e.Value).HasColumnName("EmailValue").IsRequired();
                });

                entity.OwnsOne(u => u.PasswordValueObject, pw =>
                {
                    pw.WithOwner().HasForeignKey("UserId");
                    pw.Property(p => p.Hash).HasColumnName("PasswordHash").IsRequired();
                });

                entity.OwnsOne(u => u.RoleValueObject, role =>
                {
                    role.WithOwner().HasForeignKey("UserId");
                    role.Property(r => r.Name).HasColumnName("RoleName").IsRequired();
                });

                entity.Property(u => u.FullName).HasMaxLength(256).IsRequired();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            builder.Entity<Role>(entity =>
            {
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
