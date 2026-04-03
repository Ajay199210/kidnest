using KidNest.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Core.Interfaces
{
    public interface IMdColorsRepository
    {
        Task<int> AddAsync(MdColor mdColor);
        Task<bool> UpdateAsync(MdColor mdColor);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<MdColor>> GetAllAsync();
        Task<IEnumerable<MdColor>> GetAllActiveAsync();
        Task<MdColor?> GetByIdAsync(int id);
    }
}
