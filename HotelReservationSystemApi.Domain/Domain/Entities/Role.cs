using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Domain.ValueObject;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HotelReservationSystemAPI.Domain.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public string Description { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        private Role() { } // EF Core
                           // Factory method (matches User.Create pattern)
        public static Result<RoleCreationData> Create(string roleName, string description = "")
        {
            // Validation using RoleValue
            var roleValueResult = RoleValue.Create(roleName);
            if (!roleValueResult.IsSuccess)
                return Result<RoleCreationData>.Failure(roleValueResult.Message);
            // Create instance
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            var domainEvent = new RoleCreatedEvent(role.Id, roleName, DateTime.UtcNow);
            var creationData = new RoleCreationData(role, domainEvent);
            return Result<RoleCreationData>.Success(creationData, "Role created successfully");
        }
        // Domain behavior — Update role info
        public OperationResult UpdateRole(string roleName, string description)
        {
            // Validation using RoleValue
            var roleValueResult = RoleValue.Create(roleName);
            if (!roleValueResult.IsSuccess)
                return OperationResult.Failure(roleValueResult.Message);
            Name = roleName;
            NormalizedName = roleName.ToUpperInvariant();
            Description = description;
            UpdatedAt = DateTime.UtcNow;
            return OperationResult.Success("Role updated successfully");
        }
        public class RoleCreationData
        {
            public Role Role { get; }
            public RoleCreatedEvent DomainEvent { get; }
            public RoleCreationData(Role role, RoleCreatedEvent domainEvent)
            {
                Role = role;
                DomainEvent = domainEvent;
            }
        }
        public record RoleCreatedEvent(Guid RoleId, string RoleName, DateTime OccurredAt);
    }

}


