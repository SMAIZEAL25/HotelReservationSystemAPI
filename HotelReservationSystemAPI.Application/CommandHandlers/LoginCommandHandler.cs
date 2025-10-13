using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HotelReservationSystemAPI.Application.CommandHandlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, APIResponse<LoginResponseDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;  // Added for domain persistence (e.g., refresh token update)
    private readonly ITokenService _tokenService;
    private readonly RedisTokenBucketRateLimiter _rateLimiter;
    private readonly IEventStore _eventStore;  // For optional persistence
    private readonly IEventBus _eventBus;  // Custom bus
    private readonly IMediator _mediator;
    private readonly IDistributedCache _cache;  // For user details caching
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<User> userManager,
        IUserRepository userRepository,  // Injected
        ITokenService tokenService,
        RedisTokenBucketRateLimiter rateLimiter,
        IEventStore eventStore,
        IEventBus eventBus,
        IMediator mediator,
        IDistributedCache cache,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _rateLimiter = rateLimiter;
        _eventStore = eventStore;
        _eventBus = eventBus;
        _mediator = mediator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<APIResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var dto = request.LoginDto;
        _logger.LogInformation("Login attempt for {Email}", dto.Email);

        // 1. Check if user exists
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found for {Email}", dto.Email);
            return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.NotFound, "User not found");
        }

        // 2. Apply rate limiting (based on user ID + IP)
        var ip = "127.0.0.1"; // TODO: Extract from IHttpContextAccessor
        bool allowed = await _rateLimiter.AllowRequestAsync(user.Id.ToString(), ip);
        if (!allowed)
        {
            _logger.LogWarning("Rate limit exceeded for {Email} from IP {IP}", dto.Email, ip);
            return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.TooManyRequests, "Too many login attempts. Try again shortly.");
        }

        // 3. Validate password using Identity's hasher
        bool validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!validPassword)
        {
            _logger.LogWarning("Invalid password for {Email}", dto.Email);
            return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.Unauthorized, "Invalid credentials");
        }

        // 4. Check email confirmed
        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Email not confirmed for {Email}", dto.Email);
            return APIResponse<LoginResponseDto>.Fail(HttpStatusCode.Forbidden, "Please confirm your email before logging in.");
        }

        // 5. Retrieve role from Identity
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? user.Role.ToString();

        // 6. Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // 7. Update refresh token in domain entity via repo (align with snippet)
        user.RefreshToken = refreshToken; // Assumes User has this prop
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);  // Use repo for domain persistence

        // 8. Events (align with snippet: MediatR + Custom Bus + Optional Store)
        var loginEvent = new UserLoggedInEvent(user.Id, user.Email);
        await _eventStore.SaveEventAsync(loginEvent);  // Optional persistence
        await _mediator.Publish(loginEvent, cancellationToken);  // MediatR handles INotification
        _eventBus.Publish(loginEvent);  // Custom bus if needed

        // 9. Cache user details (email & name only)
        var cacheKey = $"user:{user.Id}";
        var userCacheData = new { Email = user.Email, FullName = user.FullName };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userCacheData),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

        _logger.LogInformation("Login successful for {UserId} ({Email})", user.Id, user.Email);

        var response = new LoginResponseDto
        {
            FullName = user.FullName,
            Email = user.Email,
            Role = role,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return APIResponse<LoginResponseDto>.Success(response, "Login successful");
    }
}