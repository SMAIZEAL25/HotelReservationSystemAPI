namespace HotelReservationSystemAPI.Domain.Events
{

    public class UserRegisteredEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid AggregateId { get; }  // Added: UserId as aggregate ID
        public string Email { get; }

        public UserRegisteredEvent(Guid userId, string email)
        {
            AggregateId = userId;
            Email = email; //?? throw new ArgumentNullException(nameof(email));
        }
    }
}

    




