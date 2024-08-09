using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.API.Common;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsExpressAppController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IClientRepository _ClientRepository;
        private readonly IOrderItemRepository _OrderItemRepository;
        private readonly IOrderRepository _OrderRepository;
        private readonly IProductRepository _ProductRepository;
        private readonly IImageProductRepository _ImageProductRepository;
        private readonly IAllCodeRepository _AllCodeRepository;
        public UsExpressAppController(IClientRepository clientRepository, IOrderItemRepository orderItemRepository, IOrderRepository orderRepository, IProductRepository productRepository,
            IImageProductRepository imageProductRepository, IAllCodeRepository allCodeRepository, IConfiguration configuration)
        {
            _ClientRepository = clientRepository;
            _OrderItemRepository = orderItemRepository;
            _OrderRepository = orderRepository;
            _ProductRepository = productRepository;
            _ImageProductRepository = imageProductRepository;
            _AllCodeRepository = allCodeRepository;
            _configuration = configuration;
        }
        /// <summary>
        /// api lấy chi tiết đơn hàng bao gồm thông tin đơn hàng và
        /// chi tiết các sp của đơn hàng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("getOrderDetail")]
        public async Task<ActionResult> GetOrderDetailByOrderNo(string token)
        {
            JArray objParr = null;
            //string j_param = "{'orderNo':'UAM-0A01016'}";
            //token = CommonHelper.Encode(j_param, EncryptApi);
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, ReadFile.LoadConfig().USExpressAppKey))
                {
                    string orderNo = objParr[0]["orderNo"].ToString();
                    string client_phone = objParr[0]["client_phone"].ToString();
                    if(client_phone==null||client_phone.Trim()==""||orderNo==null || orderNo.Trim() == "")
                    {

                    }
                    else
                    {
                        var order = await _OrderRepository.GetOrderDetailByOrderNo(orderNo);
                        return Ok(new
                        {
                            succeeded = true,
                            error = "",
                            order=order.order,
                            order_detail=order.order_detail
                        });
                    }
                }
                return Ok(new
                {
                    succeeded = false,
                    error = "Token invalid.",
                    order = new { },
                    order_detail = new List<object>()
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UsExpressAppController - GetOrderDetailByOrderNo " + ex.ToString());
                return Ok(new
                {
                    succeeded = false,
                    error = "Error on Excution.",
                    order = new { },
                    order_detail = new List<object>()
                });
            }
        }
        [HttpPost("getOrdersListbyPhone")]
        public async Task<ActionResult> GetOrderListByClientPhone(string token)
        {
            JArray objParr = null;
            try
            {
                
                if (CommonHelper.GetParamWithKey(token, out objParr, ReadFile.LoadConfig().USExpressAppKey))
                {
                    string client_phone = objParr[0]["client_phone"].ToString();
                    string time = objParr[0]["time_now"].ToString();
                    DateTime time_api=new DateTime(0);
                    try
                    {
                        time_api = DateTime.Parse(time);
                    }
                    catch (Exception)
                    {

                    }
                    if (client_phone == null || client_phone.Trim() == "" || time == null || time.Trim() == "" || time_api.AddMinutes(15) <= DateTime.Now)
                    {
                        
                    }
                    else
                    {
                        var orderInfo = await _OrderRepository.GetOrderListByClientPhone(client_phone);
                        return Ok(new
                        {
                            succeeded = true,
                            error = "",
                            order=orderInfo
                        });
                    }

                }
                return Ok(new
                {
                    succeeded = false,
                    error = "Token invalid.",
                    order = new List<object>()
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UsExpressAppController - GetOrderListByClientPhone " + ex.ToString());
                return Ok(new
                {
                    succeeded = false,
                    error = "Error on Excution.",
                    order = new List<object>()
                });
            }
        }
        [HttpPost("getOrderTracking")]
        public async Task<ActionResult> GetOrderTrackingByOrderNo(string token)
        {
            JArray objParr = null;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, ReadFile.LoadConfig().USExpressAppKey))
                {
                    string orderNo = objParr[0]["orderNo"].ToString();
                    string client_phone = objParr[0]["client_phone"].ToString();
                    if (client_phone == null || client_phone.Trim() == "" || orderNo == null || orderNo.Trim() == "")
                    {

                    }
                    else
                    {
                        var orderInfo = await _OrderRepository.GetOrderTrackingByOrderNo(orderNo);
                        return Ok(new
                        {
                            succeeded = true,
                            error = "",
                            order_tracking= orderInfo
                        });
                    }
                }
                return Ok(new
                {
                    succeeded = false,
                    error = "Token invalid.",
                    order_tracking = new List<object>()
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UsExpressAppController - GetOrderTrackingByOrderNo " + ex.ToString());
                return Ok(new
                {
                    succeeded = false,
                    error = "Error on Excution.",
                    order_tracking = new List<object>(),
                });
            }
        }
    }
}
