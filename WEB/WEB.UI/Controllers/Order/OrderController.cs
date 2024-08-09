using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.UI.FilterAttribute;
using WEB.UI.Service;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers.Order
{
    public class OrderController : Controller
    {
        private readonly IViewRenderService ViewRenderService;
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public OrderController(IViewRenderService _ViewRenderService, IConfiguration _Configuration, IHttpContextAccessor httpContextAccessor)
        {
            ViewRenderService = _ViewRenderService;
            Configuration = _Configuration;
            HttpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [AjaxAuthorize()]
        [Route("quan-ly-don-hang")]
        public async Task<IActionResult> getListingOrder()
        {
            try
            {
                var model = new OrderManagerViewModel
                {
                    receiver_name = HttpContext.User.Identities.ToList()[0].Name,
                    client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value)
                };

                return View("~/Views/Order/OrderHistory.cshtml", model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-OrderController] getListingOrder" + ex.ToString());
                return Content("Hệ thống đang bảo trì. Xin vui lòng quay lại sau");
            }
        }

        [HttpPost]
        [AjaxAuthorize()]
        [Route("order/get-order-by-status")]
        public async Task<IActionResult> getOrderByStatus(int order_status, string input_search, int total_row_current, string view_type)
        {
            try
            {
                int page_size = 5;
                var order_service = new OrderService(Configuration);
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                int page_index = (total_row_current / page_size) + 1;
                var order_result = await order_service.getListingOrder(client_id, order_status, input_search == null ? string.Empty : input_search.Trim(), page_index, page_size);
                if (order_result != string.Empty)
                {
                    var JsonParent = JArray.Parse("[" + order_result + "]");
                    string order = JsonParent[0]["data_list"].ToString();


                    var json_data_list = JArray.Parse("[" + order + "]");
                    string order_list = json_data_list[0]["dataList"].ToString();
                    string total_order = json_data_list[0]["totalOrder"].ToString();


                    var order_list_model = JsonConvert.DeserializeObject<List<OrderItemHistoryViewModel>>(order_list);
                    if (order_list_model.Count == 0)
                    {
                        return Ok(new { status = ResponseType.EMPTY });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.SUCCESS, total_order = total_order, page_size = page_size, order_status = order_status, view_type = view_type, order_view_more = await this.RenderViewToStringAsync("/Views/Shared/Components/orders/listing/tr_item_order.cshtml", order_list_model), order_list = await this.RenderViewToStringAsync("/Views/Shared/Components/orders/listing/default.cshtml", order_list_model) });
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.FAILED });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-OrderController] getOrderByStatus" + ex.ToString());
                return Ok(new { status = ResponseType.FAILED });
            }
        }

        [HttpGet]
        [AjaxAuthorize()]
        [Route("chi-tiet-don-hang-{order_id}")]
        public async Task<IActionResult> getOrderDetail(int order_id)
        {
            try
            {
                var order_service = new OrderService(Configuration);
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                var order_result = await order_service.getOrderDetail(order_id, client_id);
                if (order_result != string.Empty)
                {
                    //var JsonParent = JArray.Parse("[" + order_result + "]");
                    //string order_detail = JsonParent[0]["data_list"].ToString();

                    var order_detail_model = JsonConvert.DeserializeObject<OrderDetailViewModel>(order_result);
                    order_detail_model.receiver_name = HttpContext.User.Identities.ToList()[0].Name;
                    order_detail_model.orderId = order_id;
                    if (order_detail_model != null)
                    {
                        order_detail_model.createdOn = CommonHelper.ReverDateTimeTiny(order_detail_model.createdOn.Split(" ").First()) + " " + order_detail_model.createdOn.Split(" ").Last();
                        return View("~/Views/Order/OrderDetail.cshtml", order_detail_model);
                    }
                    else
                    {
                        return Content("Hệ thống đang bảo trì. Xin vui lòng quay lại sau");
                    }
                }
                else
                {
                    LogHelper.InsertLogTelegram("[FR-OrderController] chi-tiet-don-hang-{order_id} khong tim thay don co order_id = " + order_id + " client_id= " + client_id);
                    return Redirect("/Error/404");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-OrderController] chi-tiet-don-hang-{order_id}" + ex.ToString());
                return Redirect("/Error/404");
            }
        }

        /// <summary>
        /// Lịch sử đơn hàng
        /// </summary>
        /// <param name="order_id"></param>
        /// <returns></returns>
        [HttpPost]
        [AjaxAuthorize()]
        [Route("order/order-progress.json")]
        public async Task<IActionResult> getOrderProgress(string order_no)
        {
            try
            {
                var order_service = new OrderService(Configuration);

                var order_result = await order_service.getOrderProgress(order_no);
                if (order_result != string.Empty)
                {
                    var progress_list = JsonConvert.DeserializeObject<List<OrderProgressViewModel>>(order_result);

                    if (progress_list != null)
                    {
                        return Ok(new { status = ResponseType.SUCCESS,data = progress_list });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.EMPTY});
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.FAILED });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[getOrderProgress] order-progress-{order_id}" + ex.ToString());
                return Ok(new { status = ResponseType.ERROR });
            }
        }
    }
}