using NexusPOS.Application.Hubs;
using NexusPOS.API.Helpers;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.ConfigureDbContext(configuration);
builder.Services.ConfigureUnitOfWork();
builder.Services.ConfigureMediator();
builder.Services.ConfiguredOtherServices();
builder.Services.ConfigureSignalR();
builder.Services.ConfigureControllers();
builder.Services.ConfigureOpenApi();
builder.Services.ConfigureCors();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapHub<OrderHub>("/hubs/orders");
app.MapControllers();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();
