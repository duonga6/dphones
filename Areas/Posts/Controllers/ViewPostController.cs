using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Areas.Posts.Controllers
{
    [Area("Posts")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;
        private readonly int ITEM_PER_PAGE = 10;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/news")]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, [FromQuery(Name = "s")] string searchString)
        {
            var posts = _context.Posts.AsNoTracking().OrderByDescending(x => x.CreatedAt).AsQueryable();

            if (searchString != null)
            {
                posts = posts.Where(p => p.Title.Contains(searchString) || p.Content.Contains(searchString));
            }

            int totalPosts = await posts.CountAsync();
            ViewBag.TotalPost = totalPosts;
            int totalPage = (int)Math.Ceiling((decimal)totalPosts / ITEM_PER_PAGE);

            if (totalPage < 1) totalPage = 1;
            if (currentPage < 1) currentPage = 1;
            if (currentPage > totalPage) currentPage = totalPage;

            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPage = totalPage;

            var postInPage = posts.Skip((currentPage - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE);

            // Sản phẩm nổi bật
            var outstandingProducts = _context.Products.AsNoTracking()
                                                        .Include(p => p.ProductDiscounts)
                                                            .ThenInclude(p => p.Discount)
                                                        .Include(p => p.Colors.OrderBy(x => x.Name))
                                                            .ThenInclude(c => c.Capacities.OrderBy(x => x.SellPrice))
                                                        .Include(p => p.Reviews)
                                                        .OrderByDescending(p => p.Colors.SelectMany(c => c.Capacities).Sum(cap => cap.Sold))
                                                        .AsSingleQuery()
                                                        .Take(6);

            ViewBag.OutStandingProducts = outstandingProducts.ToList();


            return View(await postInPage.ToListAsync());
        }

        [Route("/news/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var post = await _context.Posts
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Slug == slug);
            if (post == null) return NotFound();

            // Bài viết khác
            var otherPosts = await _context.Posts.AsNoTracking()
                                            .OrderBy(x => Guid.NewGuid())
                                            .Take(4)
                                            .ToListAsync();

            ViewBag.OtherPosts = otherPosts;

            // Sản phẩm nổi bật
            var outstandingProducts = _context.Products.AsNoTracking()
                                                        .Include(p => p.ProductDiscounts)
                                                            .ThenInclude(p => p.Discount)
                                                        .Include(p => p.Colors.OrderBy(x => x.Name))
                                                            .ThenInclude(c => c.Capacities.OrderBy(x => x.SellPrice))
                                                        .Include(p => p.Reviews)
                                                        .OrderByDescending(p => p.Colors.SelectMany(c => c.Capacities).Sum(cap => cap.Sold))
                                                        .AsSingleQuery()
                                                        .Take(6);

            ViewBag.OutStandingProducts = outstandingProducts.ToList();

            // return Ok(outstandingProducts.ToList());
            return View(post);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}