using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HotelReservationSystemAPI.Infrastructure.Persistence;

namespace HotelReservationSystemAPI.Application.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly UserIdentityDB _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserIdentityDB context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new user record to the database.
        /// </summary>
        public async Task AddAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {Email} successfully added.", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {Email}", user.Email);
                throw;
            }
        }

        /// <summary>
        /// Fetches a user by email and maps it to a DTO.
        /// </summary>
        public async Task<UserRegisterDto?> GetByEmailAsync(string email)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            return user is null ? null : MapToDto(user);
        }

        /// <summary>
        /// Fetches a user by their unique identifier (GUID).
        /// </summary>
        public async Task<UserRegisterDto?> GetByIdAsync(Guid id)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return user is null ? null : MapToDto(user);
        }

        /// <summary>
        /// Lists all users, projected as DTOs for lightweight response.
        /// </summary>
        public async Task<List<UserRegisterDto>> ListAsync()
        {
            return await _context.Users.AsNoTracking()
                .Select(u => new UserRegisterDto
                {
                   
                    FullName = u.FullName,
                    Email = u.Email!,
                    Role = u.Role.ToString()
                })
                .ToListAsync();
        }

        /// <summary>
        /// Persists domain events if stored in same database.
        /// </summary>
        public async Task SaveEventAsync(object domainEvent)
        {
            // Optional: implement only if you persist domain events in DB.
            _logger.LogInformation("Domain event persisted: {@Event}", domainEvent);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Updates existing user data.
        /// </summary>
        public async Task UpdateAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {Email} updated successfully.", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Email}", user.Email);
                throw;
            }
        }

        // ==============================
        // PRIVATE HELPER
        // ==============================
        private static UserRegisterDto MapToDto(User user)
        {
            return new UserRegisterDto
            {
               
                FullName = user.FullName,
                Email = user.Email!,
                Role = user.Role.ToString()
            };
        }

        public async Task SoftDeleteAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for soft delete: {UserId}", userId);
                    return;
                }

                // Domain behavior
                var result = user.SoftDelete();
                if (!result.IsSuccess)
                {
                    _logger.LogError("Domain soft delete failed for {UserId}: {Error}", userId, result.Error);
                    throw new InvalidOperationException(result.Error);
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} soft deleted successfully.", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Recovers a soft-deleted user.
        /// </summary>
        public async Task RecoverUserAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for recovery: {UserId}", userId);
                    return;
                }

                // Domain behavior
                var result = user.Recover();
                if (!result.IsSuccess)
                {
                    _logger.LogError("Domain recovery failed for {UserId}: {Error}", userId, result.Error);
                    throw new InvalidOperationException(result.Error);
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} recovered successfully.", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recovering user {UserId}", userId);
                throw;
            }
        }
    }
}

