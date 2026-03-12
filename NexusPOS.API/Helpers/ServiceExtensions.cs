using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Commands.CreateOrder;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Services;
using NexusPOS.Infrastructure.Data;
using NexusPOS.Infrastructure.UnitOfWork;

namespace NexusPOS.API.Helpers;

public static class ServiceExtensions
{

    extension(IServiceCollection services)
    {
        public void ConfigureDbContext(IConfiguration configuration) =>
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        public void ConfigureUnitOfWork()
            => services.AddScoped<IUnitOfWork, UnitOfWork>();

        public void ConfigureMediator() => services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

        public void ConfiguredOtherServices()
        {
            services.AddScoped<IOrderRoutingService, OrderRoutingService>();
            services.AddScoped<INotificationService, SignalRNotificationService>();
            services.AddScoped<IPdfService, PdfService>();
        }
        
        public void ConfigureSignalR()=> services.AddSignalR();
        
        public void ConfigureControllers() => services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        public void ConfigureOpenApi() => services.AddOpenApi();
        
        
        public void ConfigureCors() => services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
}