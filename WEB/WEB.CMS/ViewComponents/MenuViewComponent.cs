using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WEB.CMS.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuRepository _MenuRepository;
        public MenuViewComponent(IMenuRepository menuRepository)
        {
            _MenuRepository = menuRepository;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.Menu = _MenuRepository.GetMenuParentAndChildAll().Result;
            ViewBag.UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View();
        }
    }
}
