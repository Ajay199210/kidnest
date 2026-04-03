using KidNest.Core.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.Extensions
{
    public static class ModelStateExtensions
    {
        public static void AddErrors(this ModelStateDictionary modelState, OperationResult result)
        {
            if (result == null || result.Success)
                return;

            foreach (var error in result.Errors)
            {
                modelState.AddModelError(string.Empty, error);
            }
        }
    }
}
