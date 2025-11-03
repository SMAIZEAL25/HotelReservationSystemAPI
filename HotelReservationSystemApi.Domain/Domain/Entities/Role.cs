using HotelReservationSystemAPI.Domain.Domain.Entities;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.ValueObject;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Xml.Linq;

public class Role : IdentityRole<Guid>
{
    public string Description { get; protected set; } = string.Empty;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    // Navigation: Role → RolePermissions (one-to-many)
    public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    // Read-only projection of permissions
    [NotMapped]
    public List<string> Permissions => RolePermissions.Select(rp => rp.Permission).ToList();

    // EF constructor (must be protected or public)
    private Role() { }

    protected Role(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        CreatedAt = DateTime.UtcNow; 
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Factory method to create a new role entity and domain event.
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

    
    /// Adds a permission (creates RolePermission entity).
    
    public OperationResult AddPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return OperationResult.Failure("Permission cannot be empty");

        if (Permissions.Contains(permission))
            return OperationResult.Failure("Permission already exists");

        var rolePermissionResult = RolePermission.Create(Id, permission);
        if (!rolePermissionResult.IsSuccess)
            return OperationResult.Failure(rolePermissionResult.Error);

        RolePermissions.Add(rolePermissionResult.Value);
        return OperationResult.Success("Permission added successfully");
    }

   
    /// Removes a permission from the role.
  
    public OperationResult RemovePermission(string permission)
    {
        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.Permission == permission);
        if (rolePermission == null)
            return OperationResult.Failure("Permission not found");

        RolePermissions.Remove(rolePermission);
        return OperationResult.Success("Permission removed successfully");
    }

    
    /// Updates the role’s name and description with validation.
    
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


/// Wrapper for returning both role and its domain event.

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

