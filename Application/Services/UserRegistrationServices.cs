
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using System.Net;

namespace HotelReservationSystemAPI.Application.Services
{
    public class UserRegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IEventStore _eventStore;
        private readonly ILogger<UserRegistrationService> _logger;

        public UserRegistrationService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IEventStore eventStore,
            ILogger<UserRegistrationService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _eventStore = eventStore;
            _logger = logger;
        }

        public async Task<APIResponse<UserResponseDto>> RegisterAsync(
            RegisterUserDto dto,
            Func<string, string> hashFunction)
        {
            _logger.LogInformation("Attempting to register user with email {Email}", dto.Email);

            var existingUser = await _userRepository.GetByEmailAsync(new Email(dto.Email));
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: User with email {Email} already exists", dto.Email);
                return APIResponse<UserResponseDto>.Fail(HttpStatusCode.Conflict, "User already exists");
            }

            var role = await _roleRepository.GetByNameAsync(dto.RoleName);
            if (role == null)
            {
                _logger.LogWarning("Registration failed: Role {Role} does not exist", dto.RoleName);
                return APIResponse<UserResponseDto>.Fail(HttpStatusCode.BadRequest, "Invalid role");
            }

            var emailVo = new Email(dto.Email);
            var passwordVo = Password.Create(dto.Password, hashFunction);

            var user = new User(dto.FullName, emailVo, passwordVo, role);
            await _userRepository.AddAsync(user);

            // Event sourcing
            var @event = new UserRegisteredEvent(user.Id, user.Email.Value, DateTime.UtcNow);
            await _eventStore.SaveEventAsync(@event);

            _logger.LogInformation("User {UserId} registered successfully", user.Id);

            return APIResponse<UserResponseDto>.Success(new UserResponseDto
            {
                
                FullName = user.FullName,
                Email = user.Email.Value,
                Role = user.Role.Name,
                CreatedAt = user.CreatedAt
            }, "User registered successfully");
        }
    }
}
