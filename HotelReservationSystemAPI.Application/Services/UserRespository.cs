using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly UserIdentityDB _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(useridentitydb context, ILogger<UserRepository> logger)
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
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role
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
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}

