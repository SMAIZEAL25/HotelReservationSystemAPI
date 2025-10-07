



namespace HotelReservationSystemAPI.Domain.Entities
{
    public partial class User
    {
        public class UserCreationResult
        {
            public bool IsSuccess { get; private set; }
            public string Message { get; private set; } = string.Empty;
            public User? User { get; private set; }
            public UserRegisteredEvent? DomainEvent { get; private set; }

            private UserCreationResult() { }

            public static UserCreationResult Success(User user, UserRegisteredEvent domainEvent)
            {
                return new UserCreationResult
                {
                    IsSuccess = true,
                    Message = "User created successfully",
                    User = user,
                    DomainEvent = domainEvent
                };
            }

            public static UserCreationResult Failure(string message)
            {
                return new UserCreationResult
                {
                    IsSuccess = false,
                    Message = message
                };
            }
        }
    }

}



