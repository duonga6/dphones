using App.Areas.Products.Models.Review;
using App.Models;
using App.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public ReviewController(UserManager<AppUser> userManager, ILogger<ProductController> logger, AppDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [HttpPost("/review")]
        public async Task<IActionResult> Create([FromBody] ReviewCreate model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return BadRequest();

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ProductId == model.ProductId && r.UserId == user.Id);
            if (review != null) return BadRequest(new
            {
                status = 0,
                message = "Bạn đã đánh giá sản phẩm này rồi"
            });

            var newReview = new Review()
            {
                Content = model.Content,
                ProductId = model.ProductId,
                Rate = model.Rate,
                UserId = user.Id,
                DateCreated = DateTime.Now
            };

            await _context.Reviews.AddAsync(newReview);
            await _context.SaveChangesAsync();

            return Ok(newReview);
        }

        [AllowAnonymous]
        [HttpGet("/review/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var reviews = await _context.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .OrderByDescending(r => r.DateCreated)
            .ToListAsync();

            if (reviews == null || reviews.Count == 0) return Ok();

            var reviewResponse = reviews.Select(r => new ReviewGetAllByProduct.ReviewResponse
            {
                UserId = r.UserId,
                Content = r.Content,
                Rate = r.Rate,
                UserName = r.User.FullName,
                Image = r.User.UserAvatar ?? "no-image.png",
                DateCreated = r.DateCreated.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            var avgRate = reviews.Average(r => r.Rate);

            var result = new ReviewGetAllByProduct
            {
                AverageRate = Math.Round(avgRate, 1),
                Reviews = reviewResponse
            };

            return Ok(result);
        }
    }
}