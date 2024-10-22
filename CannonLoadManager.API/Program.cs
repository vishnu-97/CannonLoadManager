using CannonLoadManager.CannonManagement.Providers.Helm;
using CannonLoadManager.Contracts.Configurations;
using CannonLoadManager.Contracts.Interfaces;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace CannonLoadManager.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.GetSection(nameof(ConfigurationSettings)).Bind(new ConfigurationSettings());//TODO: Setup validation of config
            // Add services to the container.
            Console.WriteLine($"MaxValue:{ ConfigurationSettings.MaxAllowedLoadTests }");
            builder.Logging.AddApplicationInsights(
                configureTelemetryConfiguration: (config) =>
                    config.ConnectionString = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING"),
                    configureApplicationInsightsLoggerOptions: (options) => { }
            );

            builder.Services.AddSingleton<ICannonManager, CannonManager>();
            builder.Services.AddSingleton<ICannonCommunicator, CannonCommunicator>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.           
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}