using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewComponents
{
    public class keyWordTopViewComponent: ViewComponent
    {
        /// <summary>
        /// Lấy ra từ khóa tìm kiếm có thông tin dc tim kiếm nhiều nhất
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {          
            return View();
        }
    }
}
