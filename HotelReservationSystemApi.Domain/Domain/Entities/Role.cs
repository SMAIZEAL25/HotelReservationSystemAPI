using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.ValueObject;
using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;

public class Role : IdentityRole<Guid>
{
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // EF Core private constructor
    private Role() { }

    private Role(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        CreatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Creates a new role and returns both the Role entity and a domain event.
    /// </summary>
    public static Result<RoleCreationData> Create(string roleName, string description = "")
    {
        var roleValueResult = RoleValue.Create(roleName);
        if (!roleValueResult.IsSuccess)
            return Result<RoleCreationData>.Failure(roleValueResult.Message ?? "Invalid role name");

        var role = new Role(roleName, description);
        var domainEvent = new RoleCreatedEvent(role.Id, role.Name!);

        var creationData = new RoleCreationData(role, domainEvent);
        return Result<RoleCreationData>.Success(creationData, "Role created successfully");
    }

    /// <summary>
    /// Updates role name and description with validation.
    /// </summary>
    public OperationResult Update(string roleName, string description)
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