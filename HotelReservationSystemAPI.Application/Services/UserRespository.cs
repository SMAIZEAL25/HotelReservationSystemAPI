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
    public class UserRepository : IUserRepository, IEventStore
    {
        private readonly UserIdentityDB _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserIdentityDB context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.EmailValueObject) // Ensure value objects are loaded if needed
                .FirstOrDefaultAsync(u => u.Id == id);
            return user != null ? UserDto.FromUser(user) : null;
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.EmailValueObject)
                .FirstOrDefaultAsync(u => u.EmailValueObject.Value == email);
            return user != null ? UserDto.FromUser(user) : null;
        }

        public async Task<List<UserDto>> ListAsync()
        {
            var users = await _context.Users
                .Include(u => u.EmailValueObject)
                .ToListAsync();
            return users.Select(UserDto.FromUser).ToList();
        }


        // For add use a create user Dto and update user Dto to determ the kind of the records that can be update 
        public async Task AddAsync(User user)
        {
            _logger.LogInformation("Adding user {UserId}", user.Id);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} added successfully", user.Id);
        }

        public async Task UpdateAsync(User user)
        {
            _logger.LogInformation("Updating user {UserId}", user.Id);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} updated successfully", user.Id);
        }

        public async Task SaveEventAsync(object domainEvent)
        {
            _logger.LogInformation("Saving event of type {EventType}", domainEvent.GetType().Name);
            var eventData = JsonSerializer.Serialize(domainEvent);
            // Assuming a DomainEvent entity and DbSet<DomainEvent> DomainEvents in UserIdentityDB
            var dbEvent = new DomainEvent
            {
                Id = Guid.NewGuid(),
                EventType = domainEvent.GetType().Name,
                Data = eventData,
                OccurredAt = DateTime.UtcNow
            };
            await _context.DomainEvents.AddAsync(dbEvent);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Event {EventId} of type {EventType} saved successfully", dbEvent.Id, dbEvent.EventType);
        }
    }
}

