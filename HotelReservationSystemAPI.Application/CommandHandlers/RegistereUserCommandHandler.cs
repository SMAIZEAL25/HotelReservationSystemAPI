using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace HotelReservationSystemAPI.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, APIResponse<UserDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IEventStore _eventStore;
    private readonly IEventBus _eventBus;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _cache;

    // Password hasher as func for domain factory
    private readonly Func<string, string> _hashFunction = password => password; // Placeholder

    public RegisterUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUserRepository userRepository,
        ILogger<RegisterUserCommandHandler> logger,
        IEmailService emailService,
        IEventStore eventStore,
        IEventBus eventBus,
        IMediator mediator,
        IDistributedCache cache)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _logger = logger;
        _emailService = emailService;
        _eventStore = eventStore;
        _eventBus = eventBus;
        _mediator = mediator;
        _cache = cache;
    }

    public async Task<APIResponse<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to register new user with email {Email}", request.Email);

        // 1. Check if user exists (Idempotency)
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Email {Email} is already registered.", request.Email);
            return APIResponse<UserDto>.Fail(HttpStatusCode.Conflict, "Email is already registered.");
        }

        // 2. Create via domain factory (ensures VO validation & invariants)
        var role = Enum.Parse<UserRole>(request.Role, true);
        var creationResult = User.Create(request.FullName, request.Email, request.Password, role, _hashFunction);
        if (!creationResult.IsSuccess)
        {
            _logger.LogError("Domain validation failed for {Email}: {Error}", request.Email, creationResult.Error ?? "Unknown error");
            return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, creationResult.Error ?? "Validation failed");
        }

        var userCreationData = creationResult.Value;
        if (userCreationData == null || userCreationData.User == null)
        {
            _logger.LogError("User creation data is null for {Email}", request.Email);
            return APIResponse<UserDto>.Fail(HttpStatusCode.InternalServerError, "User creation failed due to unexpected error.");
        }

        var user = userCreationData.User;

        // 3. Ensure role exists (via CQRS command)
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            _logger.LogInformation($"Role '{request.Role}' not found. Creating via command...");
            var roleCommand = new CreateRoleCommand(request.Role);
            var roleResponse = await _mediator.Send(roleCommand, cancellationToken);
            if (!roleResponse.IsSuccess)
            {
                _logger.LogError("Failed to create role {Role}: {Error}", request.Role, roleResponse.Message);
                return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, $"Failed to create role: {roleResponse.Message}");
            }
        }

        // 4. Persist with Identity (hashing already in factory)
        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            _logger.LogError("Identity creation failed for {Email}: {Errors}", request.Email, errors);
            return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, $"User creation failed: {errors}");
        }

        // 5. Assign role
        var addRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user); // Rollback
            var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
            _logger.LogError("Role assignment failed for {Email}: {Errors}", request.Email, errors);
            return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, $"Role assignment failed: {errors}");
        }

        // 6. Email confirmation (part of flow – sends link, but confirmation is separate endpoint)
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"https://yourapi.com/api/users/confirm-email?email={user.Email}&token={token}";
        await _emailService.SendConfirmationEmailAsync(user.Email ?? "", "Confirm Your Email", $"Please confirm your account: {confirmationLink}");  // Fixed: ?? ""

        _logger.LogInformation("Email confirmation link sent to {Email}", user.Email ?? "unknown");

        // 7. Events (store → MediatR → bus)
        var domainEvent = creationResult.Value!.DomainEvent;
        await _eventStore.SaveEventAsync(domainEvent);
        await _mediator.Publish(domainEvent, cancellationToken);
        _eventBus.Publish(domainEvent);

        // 8. Cache (email & name only)
        var cacheKey = $"user:{user.Id}";
        var userCacheData = new { Email = user.Email, FullName = user.FullName };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userCacheData),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

        _logger.LogInformation("User {Email} registered successfully with role {Role}", user.Email, user.Role);

        return APIResponse<UserDto>.Success(UserDto.FromUser(user), "User registered successfully. Please confirm your email.");
    }
}