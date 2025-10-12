using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Interface
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string to, string subject, string body);
    }
}
