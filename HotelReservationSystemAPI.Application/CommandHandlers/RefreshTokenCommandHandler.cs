
using global::HotelReservationSystemAPI.Application.Commands;
using global::HotelReservationSystemAPI.Application.CommonResponse;
using global::HotelReservationSystemAPI.Application.DTO_s;
using global::HotelReservationSystemAPI.Application.Interface;
using global::HotelReservationSystemAPI.Domain.Entities;
using global::HotelReservationSystemAPI.Domain.Events;
using HotelReservationAPI.Application.Interface;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Text.Json;


namespace HotelReservationSystemAPI.Application.CommandHandlers
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, APIResponse<LoginResponseDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEventStore _eventStore;
        private readonly IEventBus _eventBus;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RefreshTokenHandler> _logger;

        public RefreshTokenHandler(
            UserManager<User> userManager,
            IUserRepository userRepository,
            ITokenService tokenService,
            IEventStore eventStore,
            IEventBus eventBus,
            IMediator mediator,
            IDistributedCache cache,
            ILogger<RefreshTokenHandler> logger)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _eventStore = eventStore;
            _eventBus = eventBus;
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<APIResponse<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refreshing token for request");

            // 1️⃣ Validate refresh token
            var userValidationResult = await ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (!userValidationResult.IsSuccess)
                return APIResponse<LoginResponseDto>.Fail(userValidationResult.StatusCode, userValidationResult.Message);

            var user = userValidationResult.Data;

            // 2️⃣ Generate new tokens
            var tokensGenerationResult = await GenerateTokensAsync(user, cancellationToken);
            if (!tokensGenerationResult.IsSuccess)
                return APIResponse<LoginResponseDto>.Fail(tokensGenerationResult.StatusCode, tokensGenerationResult.Message);

            var (accessToken, newRefreshToken) = tokensGenerationResult.Data;

            // 3️⃣ Update refresh token
            var updateRefreshResult = await UpdateRefreshTokenAsync(user, newRefreshToken, cancellationToken);
            if (!updateRefreshResult.IsSuccess)
                return APIResponse<LoginResponseDto>.Fail(updateRefreshResult.StatusCode, updateRefreshResult.Message);

            // 4️⃣ Publish events
            await PublishRefreshEventsAsync(user, cancellationToken);

            // 5️⃣ Cache user details
            await CacheUserDetailsAsync(user, cancellationToken);

            _logger.LogInformation("Token refreshed for {UserId}", user.Id);

            var response = new LoginResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };

            return APIResponse<LoginResponseDto>.Success(response, "Token refreshed successfully");
        }


        // Step 1: Validate refresh token (returns APIResponse<User>)
        private async Task<APIResponse<User>> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return APIResponse<User>.Fail(HttpStatusCode.Unauthorized, "Invalid or expired refresh token");
            }
            return APIResponse<User>.Success(user);
        }

        // Step 2: Generate new tokens (returns APIResponse<(string AccessToken, string RefreshToken)>)
        private async Task<APIResponse<(string AccessToken, string RefreshToken)>> GenerateTokensAsync(User user, CancellationToken cancellationToken)
        {
            var accessToken = await _tokenService.GenerateAccessToken(user); 
            var refreshToken = _tokenService.GenerateRefreshToken();

            return APIResponse<(string, string)>.Success((accessToken, refreshToken));
        }


        // Step 3: Update refresh token (returns APIResponse<LoginResponseDto>)
        private async Task<APIResponse<LoginResponseDto>> UpdateRefreshTokenAsync(User user, string refreshToken, CancellationToken cancellationToken)
        {
            var expiry = TimeSpan.FromDays(7);
            var updateResult = user.UpdateRefreshToken(refreshToken, expiry);
            if (!updateResult.IsSuccess)
            {
                _logger.LogError("Domain refresh token update failed for {UserId}: {Error}", user.Id, updateResult.Error);
                return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.InternalServerError, "Failed to update refresh token");
            }

            await _userRepository.UpdateAsync(user);
            return APIResponse<LoginResponseDto>.Success(data:null);  // Success with null DTO
        }

        // Step 4: Publish events
        private async Task PublishRefreshEventsAsync(User user, CancellationToken cancellationToken)
        {
            var refreshEvent = new UserTokenRefreshedEvent(user.Id, user.Email);
            await _eventStore.SaveEventAsync(refreshEvent);
            await _mediator.Publish(refreshEvent, cancellationToken);
            _eventBus.Publish(refreshEvent);
        }

        // Step 5: Cache user details
        private async Task CacheUserDetailsAsync(User user, CancellationToken cancellationToken)
        {
            var cacheKey = $"user:{user.Id}";
            var userCacheData = new { Email = user.Email, FullName = user.FullName };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userCacheData),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });
        }
    }
}
