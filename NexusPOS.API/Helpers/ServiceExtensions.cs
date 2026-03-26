using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexusPOS.Application.Commands.CreateOrder;
using NexusPOS.Application.Hubs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Services;
using NexusPOS.Application.Validators;
using NexusPOS.Infrastructure.Data;
using NexusPOS.Infrastructure.Services;
using NexusPOS.Infrastructure.UnitOfWork;

namespace NexusPOS.API.Helpers;

public static class ServiceExtensions
{
    public static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    public static void ConfigureUnitOfWork(IServiceCollection services)
        => services.AddScoped<IUnitOfWork, UnitOfWork>();

    public static void ConfigureMediator(IServiceCollection services) => services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

    public static void ConfigureJwt(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtService, JwtService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
    }

    public static void ConfigureOtherServices(IServiceCollection services)
    {
        services.AddScoped<IOrderRoutingService, OrderRoutingService>();
        services.AddScoped<INotificationService, SignalRNotificationService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }

    public static void ConfigureSignalR(IServiceCollection services) => services.AddSignalR()
        .AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = null;
        })
        .AddHubOptions<OrderHub>(options =>
        {
            options.EnableDetailedErrors = true;
        });

    public static void ConfigureControllers(IServiceCollection services) => services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    public static void ConfigureOpenApi(IServiceCollection services) => services.AddOpenApi();


    public static void ConfigureCors(IServiceCollection services, IConfiguration configuration) 
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:5175", "http://localhost:8081" };
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public static void ConfigureFluentValidation(IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
    }
}