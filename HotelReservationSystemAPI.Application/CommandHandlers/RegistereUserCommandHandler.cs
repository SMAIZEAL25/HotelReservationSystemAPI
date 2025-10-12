
using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Handlers
{
    public class RegistereUserCommandHandler : IRequestHandler<RegisterUserCommand, APIResponse<UserDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegistereUserCommandHandler> _logger;
        private readonly IEmailService _emailService;
        private readonly IEventStore _eventStore;
        private readonly IEventBus _eventBus;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;

        // Password hasher as func for domain factory
        private readonly Func<string, string> _hashFunction = password => /* Use IPasswordHasher or BCrypt */ password; // Placeholder; inject IPasswordHasher

        public RegistereUserCommandHandler(
            UserManager<User> userManager,
            ILogger<RegistereUserCommandHandler> logger,
            IEmailService emailService,
            IEventStore eventStore,
            IEventBus eventBus,
            IMediator mediator,
            IDistributedCache cache)
        {
            _userManager = userManager;
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
                _logger.LogError("Domain validation failed for {Email}: {Error}", request.Email, creationResult.Error);
                return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, creationResult.Error);
            }

            var user = creationResult.Value.User;

            // 3. Persist with Identity (hashing already in factory)
            var identityResult = await _userManager.CreateAsync(user, request.Password); // Password passed again for Identity
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                _logger.LogError("Identity creation failed for {Email}: {Errors}", request.Email, errors);
                return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, $"User creation failed: {errors}");
            }

            // 4. Assign role
            var addRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user); // Rollback
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                _logger.LogError("Role assignment failed for {Email}: {Errors}", request.Email, errors);
                return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, $"Role assignment failed: {errors}");
            }

            // 5. Email confirmation (part of flow)
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            await _userManager.SetEmailConfirmationTokenAsync(user, token);
            var confirmationLink = $"https://yourapi.com/api/users/confirm-email?email={user.Email}&token={token}";
            await _emailService.SendConfirmationEmailAsync(user.Email, user.FullName, confirmationLink);

            _logger.LogInformation("Email confirmation link sent to {Email}", user.Email);

            // 6. Events
            var domainEvent = creationResult.Value.DomainEvent; // From factory
            await _eventStore.SaveEventAsync(domainEvent);
            await _mediator.Publish(domainEvent);
            _eventBus.Publish(domainEvent);

            // 7. Cache (email & name only)
            var cacheKey = $"user:{user.Id}";
            var userCacheData = new { Email = user.Email, FullName = user.FullName };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userCacheData),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

            _logger.LogInformation("User {Email} registered successfully with role {Role}", user.Email, user.Role);

            return APIResponse<UserDto>.Success(UserDto.FromUser(user), "User registered successfully. Please confirm your email.");
        }
    }
}

