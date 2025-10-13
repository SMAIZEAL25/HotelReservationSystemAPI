
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.ValueObject;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HotelReservationSystemAPI.Infrastructure.Implementation
{
    public class AuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            UserManager<User> userManager,
            ITokenService tokenService,
            ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<APIResponse<LoginResponseDto>> LoginAsync(UserLoginDto loginDto)
        {
            _logger.LogInformation("Attempting login for {Email}", loginDto.Email);

            // ✅ Validate Email via Value Object
            var emailResult = EmailVO.Create(loginDto.Email);
            if (!emailResult.IsSuccess)
                return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.BadRequest, emailResult.Error ?? "Invalid email format");

            var user = await _userManager.FindByEmailAsync(emailResult.Value.Value);  // Fixed: Value.Value (Result.Value is EmailVO, EmailVO.Value is string)
            if (user == null)
            {
                _logger.LogWarning("User not found for {Email}", loginDto.Email);
                return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.Unauthorized, "Invalid credentials");
            }

            // ✅ Verify password using ASP.NET Identity
            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password for {Email}", loginDto.Email);
                return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.Unauthorized, "Invalid credentials");
            }

            // ✅ Generate JWT Tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new LoginResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),  // Ensure string conversion
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            _logger.LogInformation("Login successful for {UserId}", user.Id);
            return APIResponse<LoginResponseDto>.Success(response, "Login successful");
        }
    }
}
