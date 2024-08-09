using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;

namespace WEB.UI.ViewComponents
{
    public class RegisterViewComponent : ViewComponent
    {
        /// <summary>
        /// form dang ky he thong
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //ViewData["ReturnUrl"] = Request.Path;
            var model = new ClientViewModel
            {
                ClientId = -1,
                SourceRegisterId = Convert.ToInt32(ClientSourceType.SourceType.PC)
            };
            return View(model);
        }
    }
}
