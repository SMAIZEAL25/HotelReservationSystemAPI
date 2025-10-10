using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;

namespace HotelReservationAPI.Domain.Interface
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<UserDto?> GetByEmailAsync(string email);
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<List<UserDto>> ListAsync();
        Task SaveEventAsync(object domainEvent);
        Task UpdateAsync(User user);
    }
}
