using KidNest.Services.DTOs.MD;
using KidNest.Services.Extensions;
using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.MD.Sizes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class MdSizesController : Controller
    {
        private readonly IMdSizesService _mdSizesService;

        public MdSizesController(IMdSizesService mdSizesService)
        {
            _mdSizesService = mdSizesService;
        }

        // GET: MdSizesController
        public async Task<IActionResult> Index()
        {
            var sizeDTOs = await _mdSizesService.GetAllMdSizesAsync();

            var sizeVMs = sizeDTOs.Select(s => new MdSizeIndexViewModel
            {
                Id = s.Id,
                SizeCode = s.SizeCode,
                Description = s.Description,
                IsActive = s.IsActive,
                CreatedDate = s.CreatedDate
            });

            return View(sizeVMs);
        }

        // GET: MdSizesController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var sizeDTO = await _mdSizesService.GetMdSizeByIdAsync(id);

            if (sizeDTO == null)
                return NotFound();

            MdSizeViewModel sizeVM = new()
            {
                Id = sizeDTO.Id,
                SizeCode = sizeDTO.SizeCode,
                Description = sizeDTO.Description,
                IsActive = sizeDTO.IsActive,
                CreateDate = sizeDTO.CreatedDate
            };

            return View(sizeVM);
        }

        // GET: MdSizesController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MdSizesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MdSizeCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var createDTO = new MdSizeCreateDTO
            {
                SizeCode = model.SizeCode,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var result = await _mdSizesService.CreateMdSizeAsync(createDTO);

            if (!result.Success)
            {
                ModelState.AddErrors(result);

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MdSizesController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sizeDTO = await _mdSizesService.GetMdSizeByIdAsync(id);

            if (sizeDTO == null)
                return NotFound();

            MdSizeEditViewModel model = new()
            {
                Id = sizeDTO.Id,
                Description = sizeDTO.Description,
                SizeCode = sizeDTO.SizeCode,
                IsActive = Convert.ToBoolean(sizeDTO.IsActive)
            };

            return View(model);
        }

        // POST: MdSizesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MdSizeEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var updateDTO = new MdSizeDTO
            {
                Id = id,
                Description = model.Description,
                SizeCode = model.SizeCode,
                IsActive = model.IsActive
            };

            var result = await _mdSizesService.UpdateMdSizeAsync(updateDTO);

            if (!result.Success)
            {
                ModelState.AddErrors(result);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MdSizesController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var sizeDTO = await _mdSizesService.GetMdSizeByIdAsync(id);

            if (sizeDTO == null)
                return NotFound();

            MdSizeViewModel model = new()
            {
                Id = sizeDTO.Id,
                SizeCode = sizeDTO.SizeCode,
                Description = sizeDTO.Description,
                IsActive = sizeDTO.IsActive,
                CreateDate = sizeDTO.CreatedDate
            };

            return View(model);
        }

        // POST: MdSizesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, MdSizeViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            var result = await _mdSizesService.DeleteMdSizeAsync(id);

            if (!result.Success)
            {
                ModelState.AddErrors(result);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
