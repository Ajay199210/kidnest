using KidNest.Services.DTOs.Orders;
using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KidNest.Web.Controllers
{
    //[Authorize(AuthenticationSchemes = "UserScheme")]
    //[Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
    [Authorize(Policy = "UserOnly")] // can replace the above
    public class OrdersController : Controller
    {
        private readonly IOrdersService _ordersService;
        private readonly IUsersService _usersService;
        private readonly ICartService _cartService;

        public OrdersController(IOrdersService ordersService, IUsersService usersService, ICartService cartService)
        {
            _ordersService = ordersService;
            _usersService = usersService;
            _cartService = cartService;
        }

        // GET: OrdersController
        public ActionResult Index()
        {
            return View();
        }

        // GET: OrdersController/History/5
        public async Task<IActionResult> History()
        {
            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(currentUserIdClaim, out var userId))
            {
                return Forbid(); // Access Denied
            }

            var orders = await _ordersService.GetOrdersByUserId(userId);

            var viewModel = orders.Select(o => new OrderIndexViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                //OrderItemsCount = o.OrderItemsDTO.Sum(oi => oi.Quantity),
                Status = o.Status,
                OrderItems = o.OrderItemsDTO.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Discount = oi.Discount,
                    Color = oi.ColorName,
                    ColorHex = oi.ColorHex,
                    Size = oi.SizeCode,
                }).ToList()
            }).ToList();

            return View(viewModel);
        }

        // GET: OrdersController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // POST: OrdersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] OrderCreateViewModel orderCreateVM)
        {
            if (!ModelState.IsValid)
            {
                // Return an error message if validation fails
                return Json(new { success = false, message = "Please fill in all required fields." });
            }

            var orderCreateDTO = new OrderCreateDTO();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = (User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is not null)
                {
                    orderCreateDTO.UserId = int.Parse(userId);
                }

                try
                {
                    // Map the ViewModel to DTO manually
                    orderCreateDTO.OrderItemsDTO = orderCreateVM.OrderItems!.Select(i => new OrderItemDTO
                    {
                        ProductId = i.ProductId,
                        Name = i.ProductName,
                        ProductVariantId = i.VariantId,
                        Price = i.Price,
                        Quantity = i.Quantity,
                    }).ToList();

                    // Call the service to create the order
                    var result = await _ordersService.CreateOrderAsync(orderCreateDTO);

                    if (result.Success)
                    {
                        _cartService.ClearCart();

                        return Json(new { success = true, message = $"Your order has been successfully submitted!" });
                    }

                    // Return failure message if the service fails
                    return Json(new { success = false, message = result.Message! });
                }
                catch (Exception)
                {
                    // Log error if needed
                    return Json(new { success = false, message = $"An unexpected error occurred" });
                }
            }

            return Json(new
            {
                success = false,
                message = $"You have to be logged in to confirm your order! " +
                $"<a href='/Account/Login' class='alert-link'>Login here</a>."
            }
            );
        }

        // POST: OrdersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
