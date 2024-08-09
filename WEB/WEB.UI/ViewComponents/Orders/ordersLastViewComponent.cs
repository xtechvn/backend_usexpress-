using Entities.ViewModels;
using Entities.ViewModels.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB.UI.Controllers.Order;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class ordersLastViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        public ordersLastViewComponent(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync(string view_comp, long client_id)
        {
            try
            {
                var order_service = new OrderService(configuration);
                var order_last_result = await order_service.getOrderLastByClientId(client_id);
                if (order_last_result != string.Empty)
                {
                    var model = JsonConvert.DeserializeObject<OrderItemHistoryViewModel>(order_last_result);
                    return View(view_comp, model);
                }
                else
                {                    
                    return Content("");
                }
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("fr orderLast  " + ex.Message);
                return Content("");
            }
        }
    }
}
