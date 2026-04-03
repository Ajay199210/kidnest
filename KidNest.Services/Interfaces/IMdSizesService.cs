using KidNest.Core.Shared;
using KidNest.Services.DTOs.MD;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.Interfaces
{
    public interface IMdSizesService
    {
        Task<IEnumerable<MdSizeDTO>> GetAllMdSizesAsync();
        Task<MdSizeDTO?> GetMdSizeByIdAsync(int mdSizeId);
        Task<OperationResult> CreateMdSizeAsync(MdSizeCreateDTO mdSizeCreateDTO);
        Task<OperationResult> UpdateMdSizeAsync(MdSizeDTO mdSizeDTO);
        Task<OperationResult> DeleteMdSizeAsync(int mdSizeId);

        Task<IEnumerable<SelectListItem>> GetMdSizesSelectListAsync();
    }
}
