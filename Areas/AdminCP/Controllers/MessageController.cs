using App.Data;
using App.Models;
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
            var query = from m in
                        (
                            from mess in _context.Messages.AsEnumerable()
                            where mess.Sender != "admin"
                            orderby mess.CreatedAt descending
                            group mess by mess.Sender into grMess
                            select grMess.First()
                        )
                        join user in _context.Users on m.Sender equals user.Id
                        select new
                        {
                            userId = user.Id,
                            userName = user.FullName,
                            userAvatarUrl = $"/files/UserAvatar/{user.UserAvatar ?? "no-image.png"}",
                            lastMessage = m.Content,
                            date = m.CreatedAt,
                            seen = m.Seen
                        };

            var userInMessage = (from mess in _context.Messages
                                 join user in _context.Users on mess.Sender equals user.Id
                                 select user).ToList().Distinct();

            var userList = new List<AdminGetUserListModel>();

            foreach (var item in userInMessage)
            {
                var messages = await _context.Messages.AsNoTracking()
                                    .Where(x => x.Sender == item.Id || x.Receiver == item.Id)
                                    .OrderByDescending(x => x.CreatedAt)
                                    .FirstOrDefaultAsync();

                userList.Add(new AdminGetUserListModel
                {
                    UserId = item.Id,
                    UserName = string.IsNullOrEmpty(item.FullName) ? item.Email! : item.FullName,
                    UserAvatarUrl = $"/files/UserAvatar/{item.UserAvatar ?? "no-image.png"}",
                    LastMessage = messages!.Content,
                    Date = messages!.CreatedAt,
                    Seen = messages!.Seen,
                    IsLastMessageFromAdmin = messages.Sender == "admin"
                });
            }

            return Ok(userList);
        }

        public class AdminGetUserListModel
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
        public IActionResult AdminGetUser(string userId)
        {
            var query = from m in
                        (
                            from mess in _context.Messages.AsEnumerable()
                            where mess.Sender == userId
                            orderby mess.CreatedAt descending
                            group mess by mess.Sender into grMess
                            select grMess.First()
                        )
                        join user in _context.Users on m.Sender equals user.Id
                        select new
                        {
                            userId = user.Id,
                            userName = user.FullName,
                            userAvatarUrl = $"/files/UserAvatar/{user.UserAvatar ?? "no-image.png"}",
                            lastMessage = m.Content,
                            date = m.CreatedAt,
                            seen = m.Seen
                        };

            return Ok(query.FirstOrDefault());
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet("/admin-get-user-message/{userId}")]
        public async Task<IActionResult> AdminGetUserMessage(string userId)
        {
            var query = from mess in _context.Messages
                        orderby mess.CreatedAt ascending
                        where mess.Sender == userId || mess.Receiver == userId
                        select mess;

            return Ok(await query.ToListAsync());
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpGet("/admin-seen-message/{userId}")]
        public async Task<IActionResult> AdminSeenMessage(string userId)
        {
            var adminId = (await _userManager.GetUserAsync(User))?.Id;

            var message = await _context.Messages.Where(x => x.Sender == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

            if (message != null && adminId != null)
            {
                message.AdminId = adminId;
                message.Seen = true;
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest();

        }

        [HttpGet("/client-seen-message")]
        public async Task<IActionResult> ClientSeenMessageAsync()
        {
            var userId = (await _userManager.GetUserAsync(User))?.Id;

            var message = await _context.Messages.Where(x => x.Receiver == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

            if (message != null && userId != null)
            {
                message.Seen = true;
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest();
        }

        [HttpGet("/client-get-message")]
        public async Task<IActionResult> ClientGetMessage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var messages = await _context.Messages.Where(x => x.Sender == user.Id || x.Receiver == user.Id).OrderBy(x => x.CreatedAt).ToListAsync();

            return Ok(messages);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}