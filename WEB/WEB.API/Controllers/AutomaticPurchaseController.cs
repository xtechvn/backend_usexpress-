
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using static Utilities.Contants.Constants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomaticPurchaseController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IAutomaticPurchaseAmzRepository _automaticPurchaseAmz;
        private readonly IAutomaticPurchaseHistoryRepository _automaticPurchaseHistory;
        private readonly IOrderRepository _OrderRepository;

        public AutomaticPurchaseController(IAutomaticPurchaseAmzRepository automaticPurchaseAmz, IOrderRepository orderRepository, IAutomaticPurchaseHistoryRepository automaticPurchaseHistory, IConfiguration configuration)
        {
            _automaticPurchaseAmz = automaticPurchaseAmz;
            _configuration = configuration;
            _automaticPurchaseHistory = automaticPurchaseHistory;
            _OrderRepository = orderRepository;

        }
        [HttpPost("get-buy-list.json")]
        public async Task<ActionResult> GetNewItems(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Failed";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    var list = await _automaticPurchaseAmz.GetNewPurchaseItems();
                    if(list != null && list.Count>0)
                    {
                        status = (int)ResponseType.SUCCESS;
                        message = "Success";
                        data = list;
                    }
                    else
                    {
                        message = "No Data";
                    }
                }
                else
                {
                    message = "Token Invalid";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-buy-list.json - AutomaticPurchaseController. Error with Token: "+ token+"\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status=status,
                msg=message,
                data=data
            });
        }
        [HttpPost("get-tracking-list.json")]
        public async Task<ActionResult> GetTrackingList(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Failed";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    var list = await _automaticPurchaseAmz.GetTrackingList();
                    if (list != null && list.Count > 0)
                    {
                        status = (int)ResponseType.SUCCESS;
                        message = "Success";
                        data = list;
                    }
                    else
                    {
                        message = "No Data";
                    }
                }
                else
                {
                    message = "Token Invalid";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-tracking-list.json - AutomaticPurchaseController. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data = data
            });
        }
        [HttpPost("update-purschased-detail.json")]
        public async Task<ActionResult> UpdatePurchaseDetail(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Data / Token Invalid ";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    string new_data = objParr[0]["data"].ToString();
                    long user_excution = Convert.ToInt64(objParr[0]["user_id"].ToString());
                    string log= objParr[0]["log"].ToString();
                    if (new_data!=null && new_data.Trim() != "" && user_excution > 0)
                    {
                        AutomaticPurchaseAmz new_detail = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(new_data);
                        if (new_detail != null && new_detail.ProductCode != null && new_detail.OrderCode != null && new_detail.OrderCode.Trim() != "" && new_detail.OrderCode.Trim() != "" && new_detail.Quanity > 0 && new_detail.Amount > 0)
                        {

                            new_detail.Id = await _automaticPurchaseAmz.AddOrUpdatePurchaseDetail(new_detail);
                            AutomaticPurchaseHistory history = new AutomaticPurchaseHistory()
                            {
                                AutomaticPurchaseId = new_detail.Id,
                                PurchaseStatus = new_detail.PurchaseStatus,
                                DeliveryStatus = new_detail.DeliveryStatus,
                                CreateDate = DateTime.Now,
                                PurchaseLog = log,
                                UserExcution = user_excution,
                            };
                            await _automaticPurchaseHistory.AddNewHistory(history);
                            status = (int)ResponseType.SUCCESS;
                            message = "Update Success - " + new_detail.Id;
                        }
                        else
                        {
                            message += ": " + new_data;
                        }
                    }
                    else
                    {
                        message += ": " + new_data;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-buy-list.json - AutomaticPurchaseController. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data = data
            });
        }
        [HttpPost("update-tracking-detail.json")]
        public async Task<ActionResult> UpdateTrackingDetail(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Data / Token Invalid ";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    string new_data = objParr[0]["data"].ToString();
                    long user_excution = Convert.ToInt64(objParr[0]["user_id"].ToString());
                    string log = objParr[0]["log"].ToString();
                    if (new_data != null && new_data.Trim() != "" && user_excution > 0)
                    {
                        AutomaticPurchaseAmz new_detail = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(new_data);
                        if (new_detail != null && new_detail.ProductCode != null && new_detail.OrderCode != null && new_detail.OrderCode.Trim() != "" && new_detail.OrderCode.Trim() != "")
                        {
                            var exists_item = await _automaticPurchaseAmz.GetById(new_detail.Id);
                            if (exists_item != null)
                            {
                                exists_item.DeliveryMessage = new_detail.DeliveryMessage;
                                exists_item.DeliveryStatus = new_detail.DeliveryStatus;
                                exists_item.OrderEstimatedDeliveryDate = new_detail.OrderEstimatedDeliveryDate;
                                await _automaticPurchaseAmz.UpdatePurchaseDetail(exists_item);
                                status = (int)ResponseType.SUCCESS;
                                message = "Update Success - " + exists_item.Id;
                            }

                        }
                        else
                        {
                            message += ": " + new_data;
                        }
                    }
                    else
                    {
                        message += ": " + new_data;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-buy-list.json - AutomaticPurchaseController. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data = data
            });
        }
        [HttpPost("check-if-purchased.json")]
        public async Task<ActionResult> CheckIfPurchased(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Data / Token Invalid ";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    string new_data = objParr[0]["data"].ToString();
                    if (new_data != null && new_data.Trim() != "")
                    {
                        AutomaticPurchaseAmz new_detail = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(new_data);
                        if (new_detail != null && new_detail.ProductCode != null && new_detail.OrderCode != null && new_detail.OrderCode.Trim() != "" && new_detail.OrderCode.Trim() != "" && new_detail.Quanity > 0 && new_detail.Amount > 0)
                        {
                            var purchase_id = await _automaticPurchaseAmz.GetIDByDetail(new_detail);
                            if(purchase_id > 0)
                            {
                                status= status = (int)ResponseType.SUCCESS;
                                message = "Success: "+purchase_id;
                            }
                            else
                            {
                                message += "Not Found or New";
                            }
                           
                        }
                        else
                        {
                            message += ": " + new_data;
                        }
                    }
                    else
                    {
                        message += ": " + new_data;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-buy-list.json - AutomaticPurchaseController. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data = data
            });
        }
        [HttpPost("get-purchase-item-history.json")]
        public async Task<ActionResult> GetAutomaticPurchaseHistory(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Token / Data Invalid";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    string order_code = objParr[0]["order_code"].ToString();
                    if(order_code==null || order_code.Trim() == "")
                    {
                        return Ok(new
                        {
                            status = status,
                            msg = message,
                            data = data
                        });
                    }
                    var list = await _automaticPurchaseHistory.GetByAutomaticPurchaseHistoryByOrderCode(order_code);
                    if (list != null && list.Count > 0)
                    {
                        status = (int)ResponseType.SUCCESS;
                        message = "Success";
                        data = list;
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-purchase-item-history.json - AutomaticPurchaseController. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data = data
            });
        }
        [HttpPost("add-new-amz.json")]
        public async Task<ActionResult> AddNewAutomaticBuyOrder(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Token / Data Invalid";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id = Convert.ToInt64(objParr[0]["order_id"].ToString());
                    if (order_id > 0)
                    {
                        var order = await _OrderRepository.CheckOrderDetail(order_id);
                        if (order != null && order.LabelId == (int)LabelType.amazon && order.OrderStatus == (int)OrderStatus.PAID_ORDER)
                        {
                            var result = await _automaticPurchaseAmz.AddNewPurchaseDetail(order_id);
                            if (result == (int)ResponseType.SUCCESS)
                            {
                                status = (int)ResponseType.SUCCESS;
                                message = "Success";
                            }
                        }
                        else
                        {
                            message = "Wrong OrderStatus or Label.";
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("add-new.json - AutomaticPurchaseController. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data = data
            });
        }
        [HttpPost("add-new-item-from-oldDB.json")]
        public async Task<ActionResult> AddNewAutomaticBuyItem(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Token / Data Invalid";
            dynamic data_rs = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    string data = objParr[0]["data"].ToString();
                    AutomaticPurchaseAmz model = null;
                    try
                    {
                        model = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(data);
                    }
                    catch { }
                    if (model!=null && model.ProductCode!=null && model.Amount>0 && model.Quanity > 0 && model.OrderCode!=null && model.OrderCode.Trim()!="")
                    {
                        var exists_code = await _OrderRepository.GetOrderDetailByContractNo(model.OrderCode);
                        if (exists_code!=null && exists_code.OrderNo.Contains(model.OrderCode))
                        {
                            model.OrderId = exists_code.Id;
                            model.ProductCode = model.ProductCode.ToUpper();
                            var result = await _automaticPurchaseAmz.AddNewPurchaseDetail(model);
                            if (result > 0)
                            {
                                status = (int)ResponseType.SUCCESS;
                                message = "Success";
                                data_rs = model;
                            }
                            else
                            {
                                message = "Cannot Add New Item to DB. ";
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("add-new-item-from-oldDB.json - AddNewAutomaticBuyItem. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data= data_rs
            });
        }
    }
}
