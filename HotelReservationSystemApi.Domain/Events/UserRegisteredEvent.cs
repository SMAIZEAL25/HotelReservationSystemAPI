namespace HotelReservationSystemAPI.Domain.Events
{

    public class UserRegisteredEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid UserId { get; }
        public string Email { get; }

        public UserRegisteredEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }
}

    




