using Microsoft.AspNetCore.SignalR;

namespace NexusPOS.Application.Hubs;

public class OrderHub : Hub
{
    public async Task JoinGroup(string group)
        => await Groups.AddToGroupAsync(Context.ConnectionId, group);

    public async Task LeaveGroup(string group)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

    public async Task JoinWaiterGroup(string username)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"waiter:{username}");

    public async Task LeaveWaiterGroup(string username)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"waiter:{username}");
}
