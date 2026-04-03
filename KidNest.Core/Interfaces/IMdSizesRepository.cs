using KidNest.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Core.Interfaces
{
    public interface IMdSizesRepository
    {
        Task<int> AddAsync(MdSize mdSize);
        Task<bool> UpdateAsync(MdSize mdSize);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<MdSize>> GetAllAsync();
        Task<IEnumerable<MdSize>> GetAllActiveAsync();
        Task<MdSize?> GetByIdAsync(int id);
    }
}
