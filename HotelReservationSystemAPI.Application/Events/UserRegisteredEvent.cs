namespace HotelReservationSystemAPI.Application.Events
{

    public record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}

    




