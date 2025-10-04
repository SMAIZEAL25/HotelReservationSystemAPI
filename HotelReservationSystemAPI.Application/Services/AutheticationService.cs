
using HotelReservationAPI.Infrastructure.Repositories.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HotelReservationSystemAPI.Application.Services
{
    public class AutheticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AutheticationService> _logger;

        public AutheticationService(IUserRepository userRepository, ILogger<AutheticationService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<APIResponse<UserResponseDto>> Authentication(string email, string password, Func<string, string, bool> verifyFunction)
        {
            _logger.LogInformation("Attempting login for {Email}", email);

            var user = await _userRepository.GetByemailAsync(new Email(email));
            if (user == null || !user.password.Verify(password, verifyFunction))
            {
                _logger.LogWarning("Login failed for {Email}", email);
                return APIResponse<UserResponseDto>.Fail(HttpStatusCode.Unauthorized, "Invalid credentials");

            }

            _logger.LogInformation("Login Successful for {userId}", user.Id);

            return APIResponse<UserResponseDto>.Success(new UserResponseDto
            {                
                FullName = user.FullName,
                Email = user.Email.Value,
                Role = user.Role.Name,
                CreatedAt = user.CreatedAt
            }, "Login successful");
        }
    }
}
