using KidNest.Core.Shared;
using KidNest.Services.DTOs.Contents;
using KidNest.Services.Extensions;
using KidNest.Services.Interfaces;
using KidNest.Web.Hubs;
using KidNest.Web.ViewModels.Contents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class ContentsController : Controller
    {
        private readonly IContentsService _contentsService;
        private readonly IHubContext<StoreHub> _hubContext;
        private readonly IFileStorageService _fileStorageService;

        public ContentsController(IContentsService contentsService,
            IHubContext<StoreHub> hubContext,
            IFileStorageService fileStorageService)
        {
            _contentsService = contentsService;
            _hubContext = hubContext;
            _fileStorageService = fileStorageService;
        }

        // GET: ContentsController
        public async Task<IActionResult> Index()
        {
            var contents = await _contentsService.GetAllContentsAsync();

            var contentsIndexVM = contents.Select(c => new ContentIndexViewModel()
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                IsActive = c.IsActive,
            });

            return View(contentsIndexVM);
        }

        // GET: ContentsController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            ContentDTO? contentDTO = await _contentsService.GetContentByIdAsync(id);

            if (contentDTO == null)
            {
                return NotFound();
            }

            ContentDetailsViewModel contentVM = new()
            {
                Id = contentDTO.Id,
                Name = contentDTO.Name,
                Type = contentDTO.Type,
                Path = contentDTO.Path,
                IsActive = contentDTO.IsActive,
            };

            return View(contentVM);
        }

        // GET: ContentsController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ContentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContentCreateViewModel contentCreateVM)
        {
            if (!ModelState.IsValid)
            {
                return View(contentCreateVM);
            }

            ContentCreateDTO contentCreateDTO = new()
            {
                Name = contentCreateVM.Name,
                Type = contentCreateVM.Type,
                Path = contentCreateVM.Path,
                IsActive = contentCreateVM.IsActive
            };

            // Save image file
            if (contentCreateVM.File != null)
            {
                //string filePath = await _contentsService.SaveContentFileAsync(contentCreateVM.File);
                string? filePath = await _fileStorageService.SaveFileAsync(contentCreateVM.File, "uploads/img/carousel");
                contentCreateDTO.Path = filePath;
            }

            var result = await _contentsService.CreateContentAsync(contentCreateDTO);

            if (!result.Success)
            {
                ModelState.AddErrors(result);

                return View(contentCreateVM);
            }

            //await _hubContext.Clients.All.SendAsync("ContentAdded", productDTO);

            return RedirectToAction(nameof(Index));
        }

        // GET: ContentsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            ContentDTO? contentDTO = await _contentsService.GetContentByIdAsync(id);

            if (contentDTO == null)
            {
                return NotFound();
            }

            ContentEditViewModel contentVM = new()
            {
                Id = contentDTO.Id,
                Name = contentDTO.Name,
                Type = contentDTO.Type,
                Path = contentDTO.Path,
                IsActive = contentDTO.IsActive,
            };

            return View(contentVM);
        }

        // POST: ContentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContentEditViewModel contentEditVM)
        {
            if (id != contentEditVM.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(contentEditVM);
            }

            try
            {
                ContentDTO contentDTO = new()
                {
                    Id = contentEditVM.Id,
                    Name = contentEditVM.Name,
                    Type = contentEditVM.Type,
                    Path = contentEditVM.Path,
                    IsActive = contentEditVM.IsActive,
                };

                // Save content image
                if (contentEditVM.File != null)
                {
                    //string imagePath = await _contentsService.SaveContentFileAsync(contentEditVM.File);
                    string imagePath = await _fileStorageService.SaveFileAsync(contentEditVM.File, "uploads/img/carousel");
                    contentDTO.Path = imagePath;
                }

                OperationResult result = await _contentsService.UpdateContentAsync(contentDTO);

                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message!);

                    return View(contentEditVM);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: ContentsController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var contentDTO = await _contentsService.GetContentByIdAsync(id);

                if (contentDTO == null)
                    return NotFound();

                ContentDeleteViewModel contentDeleteVM = new()
                {
                    Id = contentDTO.Id,
                    Name = contentDTO.Name,
                    Path = contentDTO.Path,
                    Type = contentDTO.Name,
                    IsActive = contentDTO.IsActive,
                };

                return View(contentDeleteVM);
            }
            catch (Exception ex)
            {
                // Log error (ex, "Error fetching product")
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: ContentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, ContentDeleteViewModel contentDeleteVM)
        {
            if (id != contentDeleteVM.Id)
            {
                return NotFound();
            }

            try
            {
                OperationResult deleteResult = await _contentsService.DeleteContentAsync(id);

                if (deleteResult.Success)
                    return RedirectToAction(nameof(Index));
                else
                {
                    ModelState.AddModelError("", deleteResult.Message!);

                    return View(contentDeleteVM);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // TEST
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Publish(int id, ContentDetailsViewModel contentDetailsVM)
        //{
        //    ContentDTO contentDTO = new()
        //    {
        //        Id = id,
        //        Name = contentDetailsVM.Name,
        //        Path = contentDetailsVM.Path,
        //        Type = contentDetailsVM.Type,
        //        IsActive = contentDetailsVM.IsActive,
        //    };

        //    await _hubContext.Clients.All.SendAsync("ContentAdded", contentDTO);

        //    return View();
        //}
    }
}
