using App.Data;
using App.Models;
using App.Models.Chats;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

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

        private string? userId
        {
            get { return Context.UserIdentifier; }
        }

        private bool isAdmin
        {
            get { return Context.User != null && Context.User.IsInRole(RoleName.Administrator); }
        }

        private string connectionId
        {
            get { return Context.ConnectionId; }
        }

        public override async Task OnConnectedAsync()
        {
            if (userId != null)
            {
                _connectionManager.AddConnection(userId, connectionId, isAdmin);

                var listConnectionId = "\n\tConnectionId list:\n\t\t" + string.Join("\n\t\t", _connectionManager.GetConnectionId(userId));

                _logger.LogInformation($"UserID: {userId} connected with connectionId: {connectionId}.{listConnectionId}");
                _logger.LogInformation($"Admin logged: {_connectionManager.AdminLogged()}");

            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            if (userId != null)
            {
                _connectionManager.RemoveConnection(userId, connectionId);

                var listConnectionId = "\n\tConnectionId list:\n\t\t" + string.Join("\n\t\t", _connectionManager.GetConnectionId(userId));

                _logger.LogInformation($"UserID: {userId} disconnected on connectionID: {connectionId}. {listConnectionId}");
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
            var message = new Message
            {
                Content = content,
                CreatedAt = DateTime.Now,
                Receiver = "admin",
                Sender = userId!,
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var adminConnectionId = _connectionManager.GetConnectionIdAdmin();
            foreach (var item in adminConnectionId)
            {
                await Clients.Client(item).SendAsync("NewClientMessage", message);
            }

            var clientConnectionId = _connectionManager.GetConnectionId(userId!);
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
                Receiver = clientId,
                Sender = "admin",
                AdminId = userId
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