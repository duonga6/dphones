using App.Data;
using App.Models;
using App.Models.Chats;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace SignalRChat.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ConnectionManagerService _connectionManager;
        private readonly ILogger _logger;
        private readonly AppDbContext _context;

        public ChatHub(ConnectionManagerService connectionManager, ILogger<ChatHub> logger, AppDbContext context)
        {
            _connectionManager = connectionManager;
            _logger = logger;
            _context = context;
        }

        private string? UserId
        {
            get { return Context.UserIdentifier; }
        }

        private bool IsAdmin
        {
            get { return Context.User != null && Context.User.IsInRole(RoleName.Administrator); }
        }

        private string ConnectionId
        {
            get { return Context.ConnectionId; }
        }

        public override async Task OnConnectedAsync()
        {
            if (UserId != null)
            {
                _connectionManager.AddConnection(UserId, ConnectionId, IsAdmin);

                var listConnectionId = "\n\tConnectionId list:\n\t\t" + string.Join("\n\t\t", _connectionManager.GetConnectionId(UserId));

                _logger.LogInformation($"UserID: {UserId} connected with connectionId: {ConnectionId}.{listConnectionId}");
                _logger.LogInformation($"Admin logged: {_connectionManager.AdminLogged()}");

            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            if (UserId != null)
            {
                _connectionManager.RemoveConnection(UserId, ConnectionId);

                var listConnectionId = "\n\tConnectionId list:\n\t\t" + string.Join("\n\t\t", _connectionManager.GetConnectionId(UserId));

                _logger.LogInformation($"UserID: {UserId} disconnected on connectionID: {ConnectionId}. {listConnectionId}");
                _logger.LogInformation($"Admin logged: {_connectionManager.AdminLogged()}");
            }
            await base.OnDisconnectedAsync(ex);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task ClientSendToAdmin(string content)
        {
            var messageWithAdmin = await _context.Messages
                                            .AsNoTracking()
                                            .Where(x => x.SenderId == UserId && x.ReceiverId != null)
                                            .FirstOrDefaultAsync();

            var message = new Message
            {
                Content = content,
                CreatedAt = DateTime.Now,
                ReceiverId = messageWithAdmin?.ReceiverId,
                SenderId = UserId!
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var adminConnectionId = _connectionManager.GetConnectionIdAdmin();
            foreach (var item in adminConnectionId)
            {
                await Clients.Client(item).SendAsync("NewClientMessage", message);
            }

            var clientConnectionId = _connectionManager.GetConnectionId(UserId!);
            foreach (var item in clientConnectionId)
            {
                await Clients.Client(item).SendAsync("ClientSendMessageSuccess", message);
            }
        }

        public async Task AdminSendToClient(string clientId, string content)
        {
            var message = new Message
            {
                Content = content,
                CreatedAt = DateTime.Now,
                ReceiverId = clientId,
                SenderId = UserId!,
                FromAdmin = true
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var clientConnectionId = _connectionManager.GetConnectionId(clientId);
            foreach (var item in clientConnectionId)
            {
                await Clients.Client(item).SendAsync("NewAdminMessage", message);
            }
        }
    }
}