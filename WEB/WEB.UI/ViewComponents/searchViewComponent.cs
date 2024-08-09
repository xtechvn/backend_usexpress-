using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class searchViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new searchModel
            {
                search_type = -1
            };
            return View(model);
        }
    }
}
