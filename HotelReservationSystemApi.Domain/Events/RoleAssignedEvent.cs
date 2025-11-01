using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Events
{
    public class RoleAssignedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid AggregateId { get; }  // UserId as aggregate
        public string RoleName { get; }
        public List<string> Permissions { get; }  // Role permissions

        public RoleAssignedEvent(Guid userId, string roleName, List<string> permissions)
        {
            AggregateId = userId;
            RoleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
            Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        }
    }
}
