using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace WEB.UI.ViewComponents
{
    public class LoginViewComponent: ViewComponent
    {
        /// <summary>
        /// form dang nhap he thong
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new ClientLogOnViewModel();

            return View(model);
        }
    }
}
