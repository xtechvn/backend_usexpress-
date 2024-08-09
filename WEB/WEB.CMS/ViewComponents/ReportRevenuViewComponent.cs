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
    public class ReportRevenuViewComponent : ViewComponent
    {
        private readonly IMenuRepository _MenuRepository;
        public ReportRevenuViewComponent(IMenuRepository menuRepository)
        {
            _MenuRepository = menuRepository;
        }
        public IViewComponentResult Invoke()
        {
            ViewBag.Menu = _MenuRepository.GetMenuParentAndChildAll().Result;
            return View();
        }
    }
}
