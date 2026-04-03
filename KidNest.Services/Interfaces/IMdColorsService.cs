using KidNest.Core.Shared;
using KidNest.Services.DTOs.Categories;
using KidNest.Services.DTOs.MD;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.Interfaces
{
    public interface IMdColorsService
    {
        Task<IEnumerable<MdColorDTO>> GetAllMdColorsAsync();
        Task<MdColorDTO?> GetMdColorByIdAsync(int mdColorId);
        Task<OperationResult> CreateMdColorAsync(MdColorCreateDTO mdColorDTO);
        Task<OperationResult> UpdateMdColorAsync(MdColorDTO mdColorDTO);
        Task<OperationResult> DeleteMdColorAsync(int mdColorId);

        Task<IEnumerable<SelectListItem>> GetMdColorsSelectListAsync();
    }
}
