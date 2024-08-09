using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB.UI.Controllers.Order;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class ordersListViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        public ordersListViewComponent(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync(string view_comp, long client_id, int order_status, string input_search, int current_page, int page_size)
        {
            try
            {
                var order_service = new OrderService(configuration);
                var order_result = await order_service.getListingOrder(client_id, order_status, input_search, current_page, page_size);
                if (order_result != string.Empty)
                {
                    var JsonParent = JArray.Parse("[" + order_result + "]");
                    string order  = JsonParent[0]["data_list"].ToString();
                    

                    var json_data_list = JArray.Parse("[" + order + "]");
                    string order_list = json_data_list[0]["dataList"].ToString();
                    //string totalOrder = json_data_list[0]["totalOrder"].ToString();

                    var order_list_model = JsonConvert.DeserializeObject<List<OrderItemHistoryViewModel>>(order_list);

                    return View(view_comp, order_list_model);
                }
                else
                {
                    return Content("");
                }
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("fr ordersListViewComponent " + ex.Message);
                return Content("");
            }
        }
    }
}
