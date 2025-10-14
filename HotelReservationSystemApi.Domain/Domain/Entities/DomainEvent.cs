// DomainEvent.cs (Domain/Entities/DomainEvent.cs – New Entity for Event Sourcing)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservationSystemAPI.Domain.Entities;

[Table("DomainEvents")]
public class DomainEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AggregateId { get; set; }  // Links to UserId/RoleId etc.

    [Required]
    [MaxLength(256)]
    public string EventType { get; set; } = string.Empty;  // e.g., "UserRegisteredEvent"

    [Required]
    public string Data { get; set; } = string.Empty;  // JSON-serialized event payload

    [Required]
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow; // added for temporary ordering 
}