using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
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
    public class MdSizesService : IMdSizesService
    {
        private readonly IMdSizesRepository _mdSizesRepo;

        public MdSizesService(IMdSizesRepository mdSizesRepository)
        {
            _mdSizesRepo = mdSizesRepository;
        }

        public async Task<OperationResult> CreateMdSizeAsync(MdSizeCreateDTO mdSizeCreateDTO)
        {
            try
            {
                var mdSizeToAdd = new MdSize
                {
                    Description = mdSizeCreateDTO.Description,
                    SizeCode = mdSizeCreateDTO.SizeCode,
                    IsActive = mdSizeCreateDTO.IsActive,
                    CreatedDate = DateTime.Now
                };

                var rowsAffected = await _mdSizesRepo.AddAsync(mdSizeToAdd);

                if (rowsAffected == 0)
                    return OperationResult.Fail("Failed to create the MD size. No rows affected.");

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"An unexpected error occurred while creating the MD size: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteMdSizeAsync(int mdSizeId)
        {
            try
            {
                bool isDeleted = await _mdSizesRepo.DeleteAsync(mdSizeId);
                if (!isDeleted)
                {
                    return OperationResult.Fail("MD size deletion failed.");
                }

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"An unexpected error occurred while deleting the MD size: {ex.Message}");
            }
        }

        public async Task<IEnumerable<MdSizeDTO>> GetAllMdSizesAsync()
        {
            var mdSizes = await _mdSizesRepo.GetAllAsync();

            return mdSizes.Select(s => new MdSizeDTO
            {
                Id = s.Id,
                Description = s.Description,
                SizeCode = s.SizeCode,
                IsActive = Convert.ToBoolean(s.IsActive),
                CreatedDate = s.CreatedDate
            }).ToList();
        }

        public async Task<MdSizeDTO?> GetMdSizeByIdAsync(int mdSizeId)
        {
            var mdSize = await _mdSizesRepo.GetByIdAsync(mdSizeId);

            if (mdSize == null)
                return null;

            return new MdSizeDTO
            {
                Id = mdSize.Id,
                Description = mdSize.Description,
                SizeCode = mdSize.SizeCode,
                IsActive = Convert.ToBoolean(mdSize.IsActive),
                CreatedDate = mdSize.CreatedDate
            };
        }

        public async Task<OperationResult> UpdateMdSizeAsync(MdSizeDTO mdSizeDTO)
        {
            try
            {
                var mdSizeToUpdate = new MdSize
                {
                    Id = mdSizeDTO.Id,
                    Description = mdSizeDTO.Description,
                    SizeCode = mdSizeDTO.SizeCode,
                    IsActive = mdSizeDTO.IsActive
                };

                bool isUpdated = await _mdSizesRepo.UpdateAsync(mdSizeToUpdate);

                if (isUpdated)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("MD size update failed. It might not exist or no changes were detected.");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"An unexpected error occurred while updating the MD size: {ex.Message}");
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetMdSizesSelectListAsync()
        {
            var sizes = await _mdSizesRepo.GetAllActiveAsync();

            return sizes.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Description
            });
        }
    }

}
