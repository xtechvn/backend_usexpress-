using Entities.ViewModels.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB.UI.Controllers.Order;

namespace WEB.UI.ViewComponents
{
    public class ordersTabViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        public ordersTabViewComponent(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync(string view_comp, long client_id)
        {
            try
            {
                var order_service = new OrderService(configuration);
                var tab_list_order_result = await order_service.getListTabOrder(client_id);
                if (tab_list_order_result != string.Empty)
                {
                    var order_tab_list_model = JsonConvert.DeserializeObject<List<OrderTabModel>>(tab_list_order_result);
                    return View(view_comp, order_tab_list_model);
                }
                else
                {                    
                    return Content("");
                }
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("fr getListTabOrder " + ex.Message);
                return Content("");
            }
        }
    }
}
