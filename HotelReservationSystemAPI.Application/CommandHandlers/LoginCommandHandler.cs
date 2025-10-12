using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Application.Events;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;

public class LoginCommandHandler : IRequestHandler<LoginCommand, APIResponse<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly RedisTokenBucketRateLimiter _rateLimiter;
    private readonly IMediator _mediator;
    private readonly IEventBus _eventBus;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        RedisTokenBucketRateLimiter rateLimiter,
        IMediator mediator,
        IEventBus eventBus,
        UserManager<IdentityUser> userManager,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _rateLimiter = rateLimiter;
        _mediator = mediator;
        _eventBus = eventBus;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<APIResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var dto = request.LoginDto;
        _logger.LogInformation("Login attempt for {Email}", dto.Email);

        // 1️⃣ Check if user exists in Identity
        var identityUser = await _userManager.FindByEmailAsync(dto.Email);
        if (identityUser == null)
            return APIResponse<AuthResponseDto>.Fail(HttpStatusCode.NotFound, "User not found");

        // 2️⃣ Apply rate limiting (based on userId + IP)
        var ip = "127.0.0.1"; // Ideally from HttpContextAccessor
        bool allowed = await _rateLimiter.AllowRequestAsync(identityUser.Id, ip);
        if (!allowed)
            return APIResponse<AuthResponseDto>.Fail(HttpStatusCode.TooManyRequests, "Too many login attempts. Try again shortly.");

        // 3️⃣ Validate password using Identity's hasher
        bool validPassword = await _userManager.CheckPasswordAsync(identityUser, dto.Password);
        if (!validPassword)
            return APIResponse<AuthResponseDto>.Fail(HttpStatusCode.Unauthorized, "Invalid credentials");

        // 4️⃣ Retrieve role from Identity
        var roles = await _userManager.GetRolesAsync(identityUser);
        var role = roles.FirstOrDefault() ?? "Guest";

        // 5️⃣ Retrieve domain user (if needed for extra fields)
        var domainUser = await _userRepository.GetByEmailAsync(dto.Email);

        // 6️⃣ Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(new User
        {
            Id = domainUser?.Id ?? Guid.Parse(identityUser.Id),
            Email = identityUser.Email!,
            Fullname = domainUser?.Fullname ?? identityUser.UserName ?? "User",
            Role = role
        });

        var refreshToken = _tokenService.GenerateRefreshToken();

        // 7️⃣ Update refresh token in repository
        if (domainUser != null)
        {
            domainUser.RefreshToken = refreshToken;
            domainUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(domainUser);
        }

        // 8️⃣ Publish login event
        var loginEvent = new UserLoggedInEvent(Guid.Parse(identityUser.Id), identityUser.Email!);
        await _mediator.Publish(loginEvent, cancellationToken);
        _eventBus.Publish(loginEvent);

        // 9️⃣ Return response
        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            FullName = domainUser?.Fullname ?? identityUser.UserName!,
            Email = identityUser.Email!,
            Role = role
        };

        return APIResponse<AuthResponseDto>.Success(response, "Login successful");
    }
}