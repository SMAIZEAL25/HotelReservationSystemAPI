using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;

namespace HotelReservationAPI.Domain.Interface
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<UserRegisterDto?> GetByEmailAsync(string email);
        Task<UserRegisterDto?> GetByIdAsync(Guid id);
        Task<List<UserRegisterDto>> ListAsync();
        Task SaveEventAsync(object domainEvent);
        Task UpdateAsync(User user);
    }
}
