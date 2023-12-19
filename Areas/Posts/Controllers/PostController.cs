using System.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App.Areas.Posts.Models;
using App.Data;
using App.Models;
using App.Models.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Utilities;

namespace App.Areas.Posts.Controllers
{
    [Area("Posts")]
    [Route("[controller]/[action]")]
    [Authorize(Roles = RoleName.Administrator)]
    public class PostController : Controller
    {
        private readonly ILogger<PostController> _logger;
        [TempData]
        public string? StatusMessage { set; get; }
        private readonly AppDbContext _context;

        public PostController(ILogger<PostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var post = await _context.Posts.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();
            return View(post);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName(nameof(Create))]
        public async Task<IActionResult> CreateAsync(PostWithImage model)
        {
            var post = new Post
            {
                Content = model.Content,
                CreatedAt = DateTime.Now,
                Title = model.Title,
                UpdatedAt = DateTime.Now,
            };

            if (string.IsNullOrEmpty(model.Slug))
            {
                post.Slug = AppUtilities.GenerateSlug(model.Title);
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var file = model.ImageFile;
                var filename = await SaveImgFile(file);
                post.Image = filename;
            }

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { post.Id });
        }

        [Route("{Id}")]
        public async Task<IActionResult> Details(int Id)
        {
            var post = await _context.Posts.FindAsync(Id);
            if (post == null) return NotFound();

            return View(post);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Edit(int Id)
        {
            var post = await _context.Posts.FindAsync(Id);
            if (post == null) return NotFound();

            var editModel = new PostWithImage
            {
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                Image = post.Image,
                Title = post.Title,
                UpdatedAt = post.UpdatedAt
            };

            return View(editModel);
        }

        [HttpPost("{Id}"), ActionName(nameof(Edit))]
        public async Task<IActionResult> EditAsync(int Id, PostWithImage model)
        {
            var post = await _context.Posts.FindAsync(Id);
            if (post == null) return NotFound();

            post.Content = model.Content;
            post.Title = model.Title;
            post.UpdatedAt = DateTime.Now;
            post.Slug = model.Slug;

            post.Slug ??= AppUtilities.GenerateSlug(post.Title);

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                if (post.Image != null)
                    DeleteImgFile(post.Image);

                var file = model.ImageFile;
                var filename = await SaveImgFile(file);
                post.Image = filename;
            }
            else
            {
                if (string.IsNullOrEmpty(model.Image) && post.Image != null)
                {
                    DeleteImgFile(post.Image);
                    post.Image = null;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { post.Id });
        }

        [Route("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            var post = await _context.Posts.FindAsync(Id);
            if (post == null)
            {
                StatusMessage = "Không tìm thấy post này";
            }
            else
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                StatusMessage = "Xóa thành công";
            }


            return RedirectToAction(nameof(Index));
        }

        private void DeleteImgFile(string filename)
        {
            string filepath = Path.Combine("Uploads", "Posts", filename);
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }
        }

        private async Task<string> SaveImgFile(IFormFile file)
        {
            string currentTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string filename = currentTime + Path.GetExtension(file.FileName);
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Posts", filename);

            using var fileStream = new FileStream(filepath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return filename;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}