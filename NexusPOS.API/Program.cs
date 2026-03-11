using Microsoft.EntityFrameworkCore;
using MediatR;
using NexusPOS.Application.Commands.CreateOrder;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Services;
using NexusPOS.Domain.Entities;
using NexusPOS.Infrastructure.Data;
using NexusPOS.Infrastructure.Repositories;
using NexusPOS.Infrastructure.UnitOfWork;
using NexusPOS.API.Hubs;
using NexusPOS.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

builder.Services.AddScoped<IOrderRoutingService, OrderRoutingService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        var maxOrder = db.Orders
            .Select(o => o.Id)
            .OrderByDescending(x => x)
            .FirstOrDefault();
        Order.SetSequence(maxOrder);

        var maxItem = db.OrderItems
            .Select(i => i.Id)
            .OrderByDescending(x => x)
            .FirstOrDefault();
        Order.SetItemSequence(maxItem);
    }
    catch
    {
        Order.SetSequence(0);
        Order.SetItemSequence(0);
    }
}

app.UseCors("AllowFrontend");

app.MapHub<OrderHub>("/hubs/orders");
app.MapControllers();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();
