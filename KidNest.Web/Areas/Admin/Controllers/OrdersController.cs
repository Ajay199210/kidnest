using KidNest.Core.Entities;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Orders;
using KidNest.Services.DTOs.Products;
using KidNest.Services.Interfaces;
using KidNest.Services.Services;
using KidNest.Web.ViewModels.Orders;
using KidNest.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class OrdersController : Controller
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrdersData([FromBody] DataTableRequest request)
        {
            try
            {
                var result = await _ordersService.GetPaginatedOrdersAsync(request);

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = result.TotalCount,
                    recordsFiltered = result.TotalCount,
                    data = result.Items
                });
            }
            catch
            {
                return StatusCode(500, "Server error while processing your request");
            }
        }

        // GET: OrdersController
        public async Task<IActionResult> Index()
        {
            var orders = await _ordersService.GetAllOrdersAsync();

            var viewModel = orders.Select(o => new OrderIndexViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                UserEmail = o.UserEmail,
                //OrderItemsCount = o.OrderItemsDTO.Sum(i => i.Quantity),
                Status = o.Status
            }).ToList();

            return View(viewModel);
        }

        // GET: OrdersController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            OrderDTO? orderDTO = await _ordersService.GetOrderByIdAsync(id);
            if (orderDTO == null)
                return NotFound();

            var orderDetailsVM = new OrderDetailsViewModel
            {
                Id = orderDTO.Id,
                Status = orderDTO.Status,
                OrderDate = orderDTO.OrderDate,
                UserEmail = orderDTO.UserEmail,
                UserAddress = orderDTO.UserAddress,
                ModifiedDate = orderDTO.ModifiedDate,
                ModifiedBy = orderDTO.ModifiedBy,
                ItemsList = orderDTO.OrderItemsDTO.Select(item => new OrderItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Name,
                    Price = item.Price,
                    Discount = item.Discount,
                    Quantity = item.Quantity,
                    Color = item.ColorName,
                    ColorHex = item.ColorHex,
                    Size = item.SizeCode
                }).ToList(),
                TotalItemsCount = orderDTO.TotalItemCount,
                TotalOrderPrice = orderDTO.TotalOrderPrice
            };

            return View(orderDetailsVM);
        }

        // GET: OrdersController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OrdersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        //// GET: OrdersController/Edit/5
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var orderDTO = await _ordersService.GetOrderByIdAsync(id);
        //    if (orderDTO == null)
        //        return NotFound();

        //    var editVM = new OrderEditViewModel
        //    {
        //        Id = orderDTO.Id,
        //        Status = orderDTO.Status,
        //        ModifiedDate = DateTime.UtcNow, // Optional: Set current time
        //        ModifiedBy = User.Identity?.Name ?? "Admin",
        //    };

        //    return View(editVM);
        //}

        //// POST: OrdersController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: OrdersController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            OrderDTO? orderDTO = await _ordersService.GetOrderByIdAsync(id);
            if (orderDTO == null)
                return NotFound();

            var orderDetailsVM = new OrderDeleteViewModel
            {
                Id = orderDTO.Id,
                Status = orderDTO.Status,
                OrderDate = orderDTO.OrderDate,
                UserEmail = orderDTO.UserEmail,
                ModifiedDate = orderDTO.ModifiedDate,
                ModifiedBy = orderDTO.ModifiedBy,
                ItemsList = orderDTO.OrderItemsDTO.Select(item => new OrderItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Name,
                    Price = item.Price,
                    Discount = item.Discount,
                    Quantity = item.Quantity
                }).ToList(),
                TotalItemsCount = orderDTO.TotalItemCount,
                TotalOrderPrice = orderDTO.TotalOrderPrice
            };

            return View(orderDetailsVM);
        }

        // POST: OrdersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, OrderDeleteViewModel orderDeleteVM)
        {
            if (id != orderDeleteVM.Id)
            {
                return NotFound();
            }

            try
            {
                OperationResult deleteResult = await _ordersService.DeleteOrderAsync(id);

                if (deleteResult.Success)
                    return RedirectToAction(nameof(Index));
                else
                {
                    ModelState.AddModelError("", deleteResult.Message!);

                    return View(orderDeleteVM);
                }
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
