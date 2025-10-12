using HotelReservationSystemAPI.Domain.ValueObject;
using Microsoft.AspNetCore.Identity;

namespace HotelReservationSystemAPI.Domain.Entities
{
    public partial class Role : IdentityRole<Guid>
    {
        public string Description { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        private Role() { }

        public static Result<Role> Create(string roleName, string description = "")
        {
            var roleValueResult = RoleValue.Create(roleName);
            if (!roleValueResult.IsSuccess)
                return Result<Role>.Failure(roleValueResult.Message);

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            return Result<Role>.Success(role, "Role created successfully");
        }

        public OperationResult UpdateRole(string roleName, string description)
        {
            var roleValueResult = RoleValue.Create(roleName);
            if (!roleValueResult.IsSuccess)
                return OperationResult.Failure(roleValueResult.Message);

            Name = roleName;
            NormalizedName = roleName.ToUpperInvariant();
            Description = description;
            UpdatedAt = DateTime.UtcNow;

            return OperationResult.Success("Role updated successfully");
        }
    }
}
