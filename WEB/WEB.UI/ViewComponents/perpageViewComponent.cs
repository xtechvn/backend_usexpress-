using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WEB.UI.Common;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class perpageViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string s_view, PaginationEntitiesViewModel pagination_model)
        {

            pagination_model.cur_page = pagination_model.cur_page == 0 ? 1 : pagination_model.cur_page;

            double num_Page = Convert.ToDouble(pagination_model.total_item_store) / Convert.ToDouble(pagination_model.per_page);

            pagination_model.number_page = (int)(Math.Ceiling(num_Page));
            

            return View(s_view, pagination_model);
        }
    }
}
