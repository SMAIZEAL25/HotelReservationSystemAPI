using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.ValueObject;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Xml.Linq;

public class Role : IdentityRole<Guid>
{
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public string PermissionsJson { get; private set; } = "[]"; // JSON string for EF

    public List<string> Permissions
    {
        get => JsonSerializer.Deserialize<List<string>>(PermissionsJson) ?? new List<string>();
        private set => PermissionsJson = JsonSerializer.Serialize(value ?? new List<string>());  // Fixed: Null-safe serialization
    }

    // EF private constructor
    private Role() { }

    private Role(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        CreatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
        // Default permissions based on role
        Permissions = name switch
        {
            "Guest" => new List<string> { "read:profile" },
            "HotelAdmin" => new List<string> { "read:profile", "write:booking", "read:users" },
            "SuperAdmin" => new List<string> { "*" },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Factory method to create a new Role entity and domain event.
    /// </summary>
    public static Result<RoleCreationData> Create(string roleName, string description = "")
    {
        var roleValueResult = RoleValue.Create(roleName);
        if (!roleValueResult.IsSuccess)
            return Result<RoleCreationData>.Failure(roleValueResult.Message ?? "Invalid role name");

        var role = new Role(roleName, description);
        var domainEvent = new RoleCreatedEvent(role.Id, role.Name);
        var creationData = new RoleCreationData(role, domainEvent);
        return Result<RoleCreationData>.Success(creationData);
    }

    /// <summary>
    /// Updates the role’s name and description with validation.
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

    /// <summary>
    /// Adds a permission if it doesn’t already exist.
    /// </summary>
    public void AddPermission(string permission)
    {
        if (!Permissions.Contains(permission))
            Permissions.Add(permission);
    }

    /// <summary>
    /// Removes a permission from the role.
    /// </summary>
    public void RemovePermission(string permission)
    {
        Permissions.Remove(permission);
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