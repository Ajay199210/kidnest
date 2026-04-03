using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Categories;
using KidNest.Services.DTOs.MD;
using KidNest.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.Services
{
    public class MdColorsService : IMdColorsService
    {
        private readonly IMdColorsRepository _mdColorsRepo;

        public MdColorsService(IMdColorsRepository mdColorsRepository)
        {
            _mdColorsRepo = mdColorsRepository;
        }

        public async Task<OperationResult> CreateMdColorAsync(MdColorCreateDTO mdColorCreateDTO)
        {
            try
            {
                //bool isNameExists = await _mdColor.ExistsByNameAsync(mdColorDTO.Name!);
                //if (isNameExists)
                //    return OperationResult.Fail("Color name exists. Please choose another one");

                var mdColorToAdd = new MdColor
                {
                    Name = mdColorCreateDTO.Name,
                    HexValue = mdColorCreateDTO.HexValue,
                    IsActive = mdColorCreateDTO.IsActive,
                    CreatedDate = DateTime.Now,
                };

                var rowsAffected = await _mdColorsRepo.AddAsync(mdColorToAdd);

                if (rowsAffected == 0)
                    return OperationResult.Fail("Failed to create the MD color. No rows affected.");

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Here you could log the exception
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while creating the MD color: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteMdColorAsync(int mdColorId)
        {
            try
            {
                //// Check if the category is linked to products before deletion.
                //bool hasProducts = await _categoriesRepo.HasProductsAsync(categoryId);
                //if (hasProducts)
                //{
                //    return OperationResult.Fail("Cannot delete category: it contains associated products.");
                //}

                bool isDeleted = await _mdColorsRepo.DeleteAsync(mdColorId);
                if (!isDeleted)
                {
                    return OperationResult.Fail("MD color deletion failed.");
                }

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Log the exception here (Serilog, NLog, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while deleting the MD color: {ex.Message}");
            }
        }

        public async Task<IEnumerable<MdColorDTO>> GetAllMdColorsAsync()
        {
            var mdColors = await _mdColorsRepo.GetAllAsync();

            return mdColors.Select(c => new MdColorDTO
            {
                Id = c.Id,
                Name = c.Name,
                HexValue = c.HexValue,
                IsActive = Convert.ToBoolean(c.IsActive),
                CreatedDate = c.CreatedDate,
            }).ToList();
        }

        public async Task<MdColorDTO?> GetMdColorByIdAsync(int mdColorId)
        {
            var mdColorDTO = await _mdColorsRepo.GetByIdAsync(mdColorId);

            if (mdColorDTO == null)
                return null;  // Or throw NotFoundException

            return new MdColorDTO
            {
                Id = mdColorDTO.Id,
                Name = mdColorDTO.Name,
                HexValue = mdColorDTO.HexValue,
                IsActive = Convert.ToBoolean(mdColorDTO.IsActive),
                CreatedDate = mdColorDTO.CreatedDate
            };
        }

        public async Task<OperationResult> UpdateMdColorAsync(MdColorDTO mdColorDTO)
        {
            try
            {
                //bool isNameExists = await _categoriesRepo.ExistsByNameAsync(categoryDTO.Name!, categoryDTO.Id);
                //if (isNameExists)
                //{
                //    return OperationResult.Fail("Category name already exists.");
                //}

                var mdColorToEdit = new MdColor
                {
                    Id = mdColorDTO.Id,
                    Name = mdColorDTO.Name,
                    HexValue = mdColorDTO.HexValue,
                    IsActive = mdColorDTO.IsActive,
                };

                bool isMdColorUpdated = await _mdColorsRepo.UpdateAsync(mdColorToEdit);

                if (isMdColorUpdated)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("MD color update failed. " +
                    "It might not exist or no changes were detected.");
            }
            catch (Exception ex)
            {
                // Log the exception here (Serilog, NLog, built-in logger, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while updating MD color: {ex.Message}");
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetMdColorsSelectListAsync()
        {
            var colors = await _mdColorsRepo.GetAllActiveAsync();

            return colors.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                //Selected // Set selected
            });
        }
    }
}
