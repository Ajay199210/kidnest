using KidNest.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Core.Interfaces
{
    public interface ISettingsRepository
    {
        Task<SiteSettings?> GetAsync();
        Task<bool> UpdateAsync(SiteSettings settings);
    }
}
