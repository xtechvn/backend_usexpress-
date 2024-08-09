using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.UI.Controllers.Client;

namespace WEB.UI.ViewComponents
{
    public class footerViewComponent : ViewComponent
    {
        private readonly IConfiguration Configuration;
        public footerViewComponent(IConfiguration _Configuration)
        {
            Configuration = _Configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
         
            var obj_help = new HelpService(Configuration);
            var lits_menu_faq = await obj_help.getListMenuHelp(Convert.ToInt32(Configuration["News:cate_id_help"]));
            if(lits_menu_faq != null)
            {
                return View(lits_menu_faq);
            }
            else
            {
                return Content("");
            }            
        }
    }
}
