using KidNest.Core.Entities;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Categories;
using KidNest.Services.DTOs.MD;
using KidNest.Services.DTOs.Products;
using KidNest.Services.Extensions;
using KidNest.Services.Interfaces;
using KidNest.Services.Services;
using KidNest.Web.ViewModels.Categories;
using KidNest.Web.ViewModels.MD.Colors;
using KidNest.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class MdColorsController : Controller
    {
        private readonly IMdColorsService _mdColorsService;

        public MdColorsController(IMdColorsService mdColorsService)
        {
            _mdColorsService = mdColorsService;
        }

        // GET: MdColorsController
        public async Task<IActionResult> Index()
        {
            var mdColorDTOs = await _mdColorsService.GetAllMdColorsAsync();

            var mdColorsIndexVM = mdColorDTOs.Select(c => new MdColorIndexViewModel()
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = Convert.ToBoolean(c.IsActive),
                CreatedDate = c.CreatedDate
            });

            return View(mdColorsIndexVM);
        }

        // GET: MdColorsController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            MdColorDTO? mdColorDTO = await _mdColorsService.GetMdColorByIdAsync(id);

            if (mdColorDTO == null)
            {
                return NotFound();
            }

            MdColorViewModel mdColorVM = new()
            {
                Id = mdColorDTO.Id,
                Name = mdColorDTO.Name,
                HexValue = mdColorDTO.HexValue,
                IsActive = mdColorDTO.IsActive,
                CreateDate = mdColorDTO.CreatedDate
            };

            return View(mdColorVM);
        }

        // GET: MdColorsController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MdColorsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MdColorCreateViewModel mdColorCreateVM)
        {
            if (!ModelState.IsValid)
            {
                return View(mdColorCreateVM);
            }

            try
            {
                MdColorCreateDTO mdColorCreateDTO = new()
                {
                    Name = mdColorCreateVM.Name,
                    HexValue = mdColorCreateVM.HexValue,
                    IsActive = mdColorCreateVM.IsActive,
                };

                var result = await _mdColorsService.CreateMdColorAsync(mdColorCreateDTO);

                if (!result.Success)
                {
                    ModelState.AddErrors(result);

                    return View(mdColorCreateVM);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(mdColorCreateVM);
            }
        }

        // GET: MdColorsController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            MdColorDTO? mdColorDTO = await _mdColorsService.GetMdColorByIdAsync(id);

            if (mdColorDTO == null)
            {
                return NotFound();
            }

            MdColorEditViewModel mdColorEditVM = new()
            {
                Id = mdColorDTO.Id,
                Name = mdColorDTO.Name,
                HexValue = mdColorDTO.HexValue,
                IsActive = Convert.ToBoolean(mdColorDTO.IsActive)
            };

            return View(mdColorEditVM);
        }

        // POST: MdColorsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MdColorEditViewModel mdColorEditVM)
        {
            if (!ModelState.IsValid)
            {
                return View(mdColorEditVM);
            }

            if (id != mdColorEditVM.Id)
            {
                return NotFound();
            }

            try
            {
                MdColorDTO mdColorDTO = new()
                {
                    Id = mdColorEditVM.Id,
                    Name = mdColorEditVM.Name,
                    HexValue = mdColorEditVM.HexValue,
                    IsActive = mdColorEditVM.IsActive,
                };

                var result = await _mdColorsService.UpdateMdColorAsync(mdColorDTO);

                if (!result.Success)
                {
                    ModelState.AddErrors(result);

                    return View(mdColorEditVM);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: MdColorsController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var mdColorDTO = await _mdColorsService.GetMdColorByIdAsync(id);

                if (mdColorDTO == null)
                    return NotFound();

                MdColorViewModel mdColorVM = new()
                {
                    Id = mdColorDTO.Id,
                    Name = mdColorDTO.Name,
                    HexValue = mdColorDTO.HexValue,
                    IsActive = mdColorDTO.IsActive,
                    CreateDate = mdColorDTO.CreatedDate
                };

                return View(mdColorVM);
            }
            catch (Exception ex)
            {
                // Log error (ex, "Error fetching product")
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: MdColorsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, MdColorViewModel mdColorVM)
        {
            if (id != mdColorVM.Id)
            {
                return NotFound();
            }

            try
            {
                OperationResult deleteResult = await _mdColorsService.DeleteMdColorAsync(id);

                if (deleteResult.Success)
                    return RedirectToAction(nameof(Index));
                else
                {
                    ModelState.AddErrors(deleteResult);

                    return View(mdColorVM);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
