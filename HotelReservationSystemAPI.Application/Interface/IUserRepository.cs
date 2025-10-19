
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;

namespace HotelReservationAPI.Application.Interface
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<UserRegisterDto?> GetByEmailAsync(string email);
        Task<UserRegisterDto?> GetByIdAsync(Guid id);
        Task<List<UserRegisterDto>> ListAsync();
        Task SoftDeleteAsync(Guid userId);  // New: Soft delete method
        Task RecoverUserAsync(Guid userId);  // New: Recovery method
        Task SaveEventAsync(object domainEvent);
        Task UpdateAsync(User user);
    }
}
