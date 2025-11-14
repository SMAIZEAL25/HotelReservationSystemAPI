using HotelReservationSystemAPI.Domain.ValueObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Domain.Entities
{
    public class RolePermission : Entity
    {
        public Guid RoleId { get; private set; } 
        public string Permission { get; private set; } = string.Empty; 
        
        public Role Role { get; set; } = null!;  

        // EF private constructor
        private RolePermission() { }

        private RolePermission(Guid roleId, string permission)
        {
            Id = Guid.NewGuid();
            RoleId = roleId;
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }

        public static Result<RolePermission> Create(Guid roleId, string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return Result<RolePermission>.Failure("Permission cannot be empty");

            return Result<RolePermission>.Success(new RolePermission(roleId, permission));
        }
    }
}
