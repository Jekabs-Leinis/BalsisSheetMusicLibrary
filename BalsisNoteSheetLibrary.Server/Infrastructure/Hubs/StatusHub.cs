using Microsoft.AspNetCore.SignalR;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Hubs;

public class StatusHub : Hub
{
    public async Task SendStatus(string status, string message, int? current = null,
        int? total = null)
    {
        if (current.HasValue && total.HasValue)
        {
            await Clients.Caller.SendAsync("status", new { status, current, total, message });
        }
        else
        {
            await Clients.Caller.SendAsync("status", new { status, message });
        }
    }
}