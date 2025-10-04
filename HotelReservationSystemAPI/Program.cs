
using HotelReservationAPI.Infrastructure.Persistence;
using HotelReservationAPI.Infrastructure.Repositories.Implementation;
using HotelReservationAPI.Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HotelReservationSystemAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DataBase AUTHDb

            builder.Services.AddDbContext<UserIdentityDB>(options =>
            options.UseSqlServer(configuration.GetConnectionString("HotelReservationAuthDb")));

            // services 
            builder.Services.AddScoped<IUserRepository,UserRepositroy>();
            builder.Services.AddScoped<IEventStore,EventStore>();
            builder.Services.AddScoped<IRoleRepository,RoleRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
