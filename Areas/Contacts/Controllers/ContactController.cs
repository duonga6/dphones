using App.Models;
using App.Models.Contacts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Contacts.Controllers
{
    [Area("Contacts")]
    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly AppDbContext _context;

        [TempData]
        public string StatusMessage { set; get; } = string.Empty;

        public ContactController(ILogger<ContactController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Danh sách contact
        [Route("/AdminCP/Contact/[action]")]
        public async Task<IActionResult> Index()
        {
            var contact = await _context.Contacts
                                    .AsNoTracking()
                                    .OrderByDescending(x => x.CreatedAt)
                                    .ToListAsync();
            return View(contact);
        }

        // Chi tiết contact
        [Route("/AdminCP/Contact/[action]/{Id}")]
        public async Task<IActionResult> Details(int Id)
        {
            var contact = await _context.Contacts.FindAsync(Id);
            if (contact == null) return NotFound();

            if (!contact.Seen)
            {
                contact.Seen = true;
                await _context.SaveChangesAsync();
            }

            return View(contact);
        }

        // View tạo contact
        [HttpGet("/contact")]
        public IActionResult Create()
        {
            return View();
        }

        // Tạo contact
        [HttpPost("/contact")]
        public async Task<IActionResult> CreateAsync(Contact model)
        {
            try
            {
                await _context.Contacts.AddAsync(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CreateConfirmed));
            }
            catch
            {
                StatusMessage = "Có lỗi khi tạo";
                return View();
            }
        }

        // Kết quả tạo contact
        [Route("/create-contact-successfully")]
        public IActionResult CreateConfirmed()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}