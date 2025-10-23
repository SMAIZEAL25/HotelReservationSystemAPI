using Serilog;

namespace HotelReservationSystemAPI.Extensions
{
    public static class ConfigurationPipeline
    {
        public static WebApplication ConfigurePipeline(this WebApplication app, IWebHostEnvironment environment)
        {
            // Development-specific middleware
            if (environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Common pipeline
            app.UseSerilogRequestLogging();
            app.UseRateLimiter();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
