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
    public class RevenuTodayViewComponent : ViewComponent
    {
        private readonly IOrderRepository _OrderRepository;
        public RevenuTodayViewComponent(IOrderRepository orderRepository)
        {
            _OrderRepository = orderRepository;
        }
        public IViewComponentResult Invoke()
        {
            var data = _OrderRepository.SummaryRevenuToday();
            return View(data);
        }
    }
}
