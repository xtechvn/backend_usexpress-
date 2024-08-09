using Entities.Models;
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

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderProgressController : BaseController
    {
        private readonly IOrderProgressRepository _orderProgressRepository;
        public IConfiguration _Configuration;
        public OrderProgressController(IOrderProgressRepository orderProgressRepository, IConfiguration Configuration)
        {
            _Configuration = Configuration;
            _orderProgressRepository = orderProgressRepository;
        }

        /// <summary>
        /// Lấy ra tiến trình của 1 đơn hàng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-order-progress.json")]
        public async Task<ActionResult> GetOrderProgressByOrderNo(string token)
        {
            try
            {
                JArray objParr = null;

                 //string j_param = "{'OrderNo':'UAM-1D18184'}";
                 //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string order_no =  objParr[0]["OrderNo"].ToString();
                    List<OrderProgress> detail = await _orderProgressRepository.GetOrderProgressesByOrderNoAsync(order_no);
                    List<OrderProgressViewModel> detail_result = new List<OrderProgressViewModel>();
                    if (detail != null || detail.Count>0)
                    {
                        foreach (var item in detail)
                        {
                            detail_result.Add(new OrderProgressViewModel()
                            {                               
                                CreateDate = item.CreateDate.ToString("dd/MM/yyyy HH:mm"),                                
                                OrderStatus = item.OrderStatus.ToString().Trim()
                            });
                        }
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Successful",
                            order_progress = detail_result,
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "No Record was found.",
                            order_progress = detail,
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token không hợp lệ",
                        token = token
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderProgressController - GetOrderProgressByOrderNo: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution"
                });
            }
        }

        /// <summary>
        ///Lưu Tiến trình của 1 đơn hàng từ lúc khởi tạo đến lúc nhận hàng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("push-order-progress.json")]
        public async Task<ActionResult> SetOrderProgress(string token)
        {
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string order_no = objParr[0]["OrderNo"].ToString();
                    short order_status = Convert.ToInt16(objParr[0]["OrderStatus"].ToString());
                    DateTime created_on = DateTime.Parse(objParr[0]["CreateDate"].ToString());
                    OrderProgress record = new OrderProgress()
                    {

                        OrderNo = order_no,
                        OrderStatus = order_status,
                        CreateDate = created_on
                    };
                    int status = (int)ResponseType.FAILED;
                    string msg = "Item Exists";
                    var list = await _orderProgressRepository.GetOrderProgressesByOrderNoAsync(record.OrderNo);
                    if(list== null)
                    {
                        status = await _orderProgressRepository.SetOrderProgreess(record);
                        switch (status)
                        {
                            case (int)ResponseType.SUCCESS: msg = "Successful"; break;
                            case (int)ResponseType.FAILED: msg = "Cannot Set Data"; break;
                            case (int)ResponseType.ERROR: msg = "Error on Set Data"; break;
                        }
                    }
                    else
                    {
                        bool alreadyExists = list.Any(x => x.OrderStatus == record.OrderStatus);
                        if (!alreadyExists)
                        {
                            status = await _orderProgressRepository.SetOrderProgreess(record);
                            switch (status)
                            {
                                case (int)ResponseType.SUCCESS: msg = "Successful"; break;
                                case (int)ResponseType.FAILED: msg = "Cannot Set Data"; break;
                                case (int)ResponseType.ERROR: msg = "Error on Set Data"; break;
                            }
                        }
                    }
                    return Ok(new
                    {
                        status = status,
                        msg = msg,
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Invailid Token.",
                        token = token
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderProgressController - GetOrderProgressByOrderNo: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution",
                    token = token
                });
            }
        }

    }
}
