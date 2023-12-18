using App.Data;
using App.Models;
using App.Models.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.AdminCP.Controllers
{
    [Area("AdminCP")]
    [Authorize]
    [Route("/[controller]/[action]")]
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(ILogger<MessageController> logger, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet("/admin-get-user-list")]
        public async Task<IActionResult> AdminGetUserListAsync()
        {
            string adminId = await GetLoggedUserId();

            var userHasMessage = await _context.Users
            .AsNoTracking()
            .Where(x => x.SentMessage.Any(x => x.ReceiverId == null || x.ReceiverId == adminId))
            .ToListAsync();

            var userList = new List<AdminGetUserModel>();

            foreach (var item in userHasMessage)
            {
                var messages = await _context.Messages.AsNoTracking()
                                    .Where(x => x.SenderId == item.Id || x.ReceiverId == item.Id)
                                    .OrderByDescending(x => x.CreatedAt)
                                    .FirstOrDefaultAsync();

                userList.Add(new AdminGetUserModel
                {
                    UserId = item.Id,
                    UserName = string.IsNullOrEmpty(item.FullName) ? item.Email! : item.FullName,
                    UserAvatarUrl = $"/files/UserAvatar/{item.UserAvatar ?? "no-image.png"}",
                    LastMessage = messages!.Content,
                    Date = messages!.CreatedAt,
                    Seen = messages!.Seen,
                    IsLastMessageFromAdmin = messages.SenderId == adminId
                });
            }

            return Ok(userList);
        }

        public class AdminGetUserModel
        {
            public string UserId { set; get; } = string.Empty;
            public string UserName { set; get; } = string.Empty;
            public string UserAvatarUrl { set; get; } = string.Empty;
            public string LastMessage { set; get; } = string.Empty;
            public bool IsLastMessageFromAdmin { set; get; }
            public DateTime Date { set; get; }
            public bool Seen { set; get; }
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet("/admin-get-user/{userId}")]
        public async Task<IActionResult> AdminGetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest();

            var lastMessage = await _context.Messages.Where(x => x.SenderId == user.Id).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
            if (lastMessage == null) return BadRequest();

            var modelRespone = new AdminGetUserModel
            {
                UserId = user.Id,
                UserName = user.FullName,
                UserAvatarUrl = $"/files/UserAvatar/{user.UserAvatar ?? "no-image.png"}",
                LastMessage = lastMessage.Content,
                Date = lastMessage.CreatedAt,
                Seen = lastMessage.Seen
            };

            return Ok(modelRespone);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet("/admin-get-user-message/{userId}")]
        public async Task<IActionResult> AdminGetUserMessage(string userId)
        {
            string adminId = await GetLoggedUserId();

            var messages = await _context.Messages
                .AsNoTracking()
                .Where(x => (x.SenderId == userId && (x.ReceiverId == adminId || x.ReceiverId == null)) || (x.SenderId == adminId && x.ReceiverId == userId))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet("/admin-seen-message/{userId}")]
        public async Task<IActionResult> AdminSeenMessage(string userId)
        {
            var adminId = await GetLoggedUserId();

            var message = await _context.Messages
            .Where(x => x.SenderId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

            if (message != null)
            {
                if (message.ReceiverId != null && message.ReceiverId != adminId)
                {
                    return BadRequest();
                }

                message.Seen = true;
                message.ReceiverId ??= adminId;

                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest();

        }

        [HttpGet("/client-seen-message")]
        public async Task<IActionResult> ClientSeenMessageAsync()
        {
            var userId = await GetLoggedUserId();

            var message = await _context.Messages
            .Where(x => x.ReceiverId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

            if (message != null)
            {
                message.Seen = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("/client-get-message")]
        public async Task<IActionResult> ClientGetMessage()
        {
            var userId = await GetLoggedUserId();

            var messages = await _context.Messages
                .AsNoTracking()
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        private async Task<string> GetLoggedUserId()
        {
            return (await _userManager.GetUserAsync(User))?.Id ?? "";
        }
    }
}