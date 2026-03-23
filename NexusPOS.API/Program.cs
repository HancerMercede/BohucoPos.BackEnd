using NexusPOS.Application.Hubs;
using NexusPOS.API.Helpers;
using NexusPOS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

ServiceExtensions.ConfigureDbContext(builder.Services, configuration);
ServiceExtensions.ConfigureUnitOfWork(builder.Services);
ServiceExtensions.ConfigureMediator(builder.Services);
ServiceExtensions.ConfigureJwt(builder.Services, configuration);
ServiceExtensions.ConfigureOtherServices(builder.Services);
ServiceExtensions.ConfigureSignalR(builder.Services);
ServiceExtensions.ConfigureControllers(builder.Services);
ServiceExtensions.ConfigureOpenApi(builder.Services);
ServiceExtensions.ConfigureCors(builder.Services);

var app = builder.Build();

app.UseExceptionHandling();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<OrderHub>("/hubs/orders");
app.MapControllers();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();
