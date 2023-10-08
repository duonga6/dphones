using System.Text.Json.Serialization;
using App.Areas.Products.Models;
using App.Areas.Products.Services;
using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace App.Areas.Products.Controllers
{
    [Area("Products")]
    public class ViewProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;

        private readonly CartService _cart;

        public ViewProductController(ILogger<ProductController> logger, AppDbContext context, CartService cart)
        {
            _logger = logger;
            _context = context;
            _cart = cart;
        }


        [Route("/{slug}")]
        public IActionResult Details(string slug)
        {
            var product = _context.Products.Where(p => p.Slug == slug)
                                            .Include(p => p.Photos)
                                            .Include(p => p.Colors)
                                            .ThenInclude(c => c.Capacities)
                                            .AsSplitQuery()
                                            .FirstOrDefault();

            if (product == null) return NotFound();

            product.Colors = product.Colors.OrderBy(c => c.Name).ToList();
            product.Colors.ForEach(c =>
            {
                c.Capacities = c.Capacities.OrderBy(ca => ca.Rom).ToList();
            });

            return View(product);
        }

        [HttpGet]
        [Route("/get-color")]
        public IActionResult GetColor(int productId)
        {
            var color = _context.Colors.Where(c => c.ProductId == productId).OrderBy(c => c.Name).ToList();

            return Ok(JsonConvert.SerializeObject(color));
        }

        [HttpGet]
        [Route("/get-capacity")]
        public IActionResult GetCapacity(int colorId)
        {
            var capacity = _context.Capacities.Where(c => c.ColorId == colorId).OrderBy(c => c.Rom).ToList();
            return Ok(JsonConvert.SerializeObject(capacity));
        }


        [Route("/cart")]
        public IActionResult Cart()
        {
            return View();
        }

        [HttpPost]
        [Route("/cart/add-to-cart")]
        public IActionResult AddToCart(int productId, int colorId, int capaId)
        {
            var product = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == productId);
            var color = _context.Colors.AsNoTracking().FirstOrDefault(c => c.Id == colorId && c.ProductId == productId);
            var capa = _context.Capacities.AsNoTracking().FirstOrDefault(c => c.Id == capaId && c.ColorId == colorId);

            if (product == null || color == null || capa == null)
            {
                return Json(new
                {
                    status = 0,
                    message = "Thêm thất bại. Không tìm thấy sản phẩm yêu cầu!"
                });
            }

            product.Description = null;

            var cartItem = new CartItem()
            {
                Id = Guid.NewGuid().ToString(),
                Product = product,
                Capacity = capa,
                Color = color,
                Quantity = 1
            };

            var cartList = _cart.GetCart();
            cartList ??= new List<CartItem>();
            var productInCart = cartList.FirstOrDefault(p => p.Product.Id == productId && p.Color.Id == colorId && p.Capacity.Id == capaId);
            if (productInCart == null)
            {
                cartList.Add(cartItem);
            }
            else
            {
                if (productInCart.Quantity >= productInCart.Capacity.Quantity)
                {
                    return Json(new
                    {
                        status = 0,
                        message = "Sản phẩm bạn chọn đã hết!"
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }

            _cart.SaveCart(cartList);

            return Json(new
            {
                status = 1,
                message = "Đã thêm vào giỏ hàng!",
                qtt = cartList.Count()
            });
        }

        [HttpPut]
        [Route("/cart/plus")]
        public IActionResult PlusQuantity(string Id)
        {
            var cartList = _cart.GetCart();
            var cartItem = cartList?.FirstOrDefault(c => c.Id == Id);
            if (cartItem != null)
            {
                if (cartItem.Quantity >= cartItem.Capacity.Quantity)
                {
                    return Ok(new
                    {
                        status = 0,
                        message = "Sản phẩm này đã hết!",
                        qtt = cartItem.Quantity
                    });
                }
                else
                {
                    cartItem.Quantity++;
                    _cart.SaveCart(cartList);
                    return Ok(new
                    {
                        status = 1,
                        message = "Đã tăng số lượng!",
                        qtt = cartItem.Quantity
                    });
                }
            }
            else
            {
                return NotFound(new
                {
                    status = 0,
                    message = "Không tìm thấy sản phẩm này!"
                });
            }
        }

        [HttpPut]
        [Route("/cart/minus")]
        public IActionResult MinusQuantity(string Id)
        {
            var cartList = _cart.GetCart();
            var cartItem = cartList?.FirstOrDefault(c => c.Id == Id);
            if (cartItem != null)
            {
                if (cartItem.Quantity <= 1)
                {
                    return Ok(new
                    {
                        status = 0,
                        message = "Số lượng phải >= 1",
                        qtt = cartItem.Quantity
                    });
                }
                else
                {
                    cartItem.Quantity--;
                    _cart.SaveCart(cartList);
                    return Ok(new
                    {
                        status = 1,
                        message = "Đã giảm số lượng",
                        qtt = cartItem.Quantity
                    });
                }
            }
            else
            {
                return NotFound(new
                {
                    status = 0,
                    message = "Không tìm thấy sản phẩm này"
                });
            }
        }

        [HttpDelete]
        [Route("/cart/delete-item")]
        public IActionResult DeleteCartItem(string Id)
        {
            var cartList = _cart.GetCart();
            var cartItem = cartList?.FirstOrDefault(c => c.Id == Id);

            if (cartItem == null)
                return Json(new
                {
                    status = 0,
                    message = "Lỗi. Không tìm thấy sản phẩm này trong giỏ hàng!"
                });
            else
                cartList?.Remove(cartItem);

            _cart.SaveCart(cartList);

            return Ok(new
            {
                status = 1,
                message = "Đã xóa sản phẩm!",
                qtt = cartList?.Count()
            });
        }

        [HttpGet]
        [Route("/cart/get-cart")]
        public IActionResult GetCartList()
        {
            var cartList = _cart.GetCart();
            return Ok(JsonConvert.SerializeObject(cartList));
        }
    }
}