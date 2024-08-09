using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Claims;
using System.Threading.Tasks;
using Aspose.Cells;
using Aspose.Cells.Drawing;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Common;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using Range = Aspose.Cells.Range;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _OrderRepository;
        private readonly IClientRepository _ClientRepository;
        private readonly IOrderProgressRepository _OrderProgressRepository;
        private readonly ICommonRepository _CommonRepository;
        private readonly ICashbackRepository _CashbackRepository;
        private readonly IPaymentRepository _PaymentRepository;
        private readonly ILabelRepository _LabelRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IAutomaticPurchaseAmzRepository _AutomaticPurchaseAmzRepository;
        public OrderController(IOrderRepository orderRepository, IOrderProgressRepository orderProgressRepository,
            ICommonRepository commonRepository, ICashbackRepository cashbackRepository,
            IPaymentRepository paymentRepository, IWebHostEnvironment hostEnvironment,
            IClientRepository clientRepository, ILabelRepository labelRepository,
            IAutomaticPurchaseAmzRepository automaticPurchaseAmzRepository)
        {
            _OrderRepository = orderRepository;
            _ClientRepository = clientRepository;
            _OrderProgressRepository = orderProgressRepository;
            _CommonRepository = commonRepository;
            _CashbackRepository = cashbackRepository;
            _PaymentRepository = paymentRepository;
            _LabelRepository = labelRepository;
            _WebHostEnvironment = hostEnvironment;
            _AutomaticPurchaseAmzRepository = automaticPurchaseAmzRepository;
        }

        public async Task<IActionResult> Index()
        {
            var _Date = Request.Query["Date"].ToString();
            var _PaymentStatus = Request.Query["PaymentStatus"].ToString();
            var _IsOrderError = Request.Query["IsOrderError"].ToString();
            var _ProductCode = Request.Query["ProductCode"].ToString();

            ViewBag.FilterDate = string.IsNullOrEmpty(_Date) ? string.Empty : _Date.Replace("-", "/");
            ViewBag.FilterPaymentStatus = !string.IsNullOrEmpty(_PaymentStatus) ? int.Parse(_PaymentStatus) : -1;
            ViewBag.ListOrderStatus = await _CommonRepository.GetAllCodeByType(AllCodeType.ORDER_STATUS);
            ViewBag.ListPaymentType = await _CommonRepository.GetAllCodeByType(AllCodeType.PAYMENT_TYPE);
            ViewBag.ListUTMSource = await _CommonRepository.GetAllCodeByType(AllCodeType.UTM_SOURCE);
            ViewBag.FilterErrorOrder = !string.IsNullOrEmpty(_IsOrderError) ? int.Parse(_IsOrderError) : -1;
            ViewBag.FilterProductCode = !string.IsNullOrEmpty(_ProductCode) ? _ProductCode : string.Empty;
            ViewBag.LabelList = _LabelRepository.GetListAll();
            return View();
        }

        /// <summary>
        /// Search Order
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Search(OrderSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<OrderGridModel>();
            try
            {
                model = _OrderRepository.GetPagingList(searchModel, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }

        /// <summary>
        /// Report Order
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExportExcel(OrderSearchModel searchModel)
        {
            try
            {
                string _FileName = "Order_" + Guid.NewGuid() + ".xlsx";
                string _UploadFolder = @"Template/Export";
                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                if (!Directory.Exists(_UploadDirectory))
                {
                    Directory.CreateDirectory(_UploadDirectory);
                }

                string FilePath = Path.Combine(_UploadDirectory, _FileName);

                var rsPath = _OrderRepository.ReportOrder(searchModel, FilePath);

                if (!string.IsNullOrEmpty(rsPath))
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xuất dữ liệu thành công",
                        path = "/" + _UploadFolder + "/" + _FileName
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xuất dữ liệu thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        [HttpGet]
        public IActionResult ExportOrderExpectedExcel()
        {
            try
            {
                string _FileName = "Order_" + Guid.NewGuid() + ".xlsx";
                string _UploadFolder = @"Template/Export";
                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                if (!Directory.Exists(_UploadDirectory))
                {
                    Directory.CreateDirectory(_UploadDirectory);
                }

                string FilePath = Path.Combine(_UploadDirectory, _FileName);

                var rsPath = _OrderRepository.ExportOrderExpected(FilePath);

                if (!string.IsNullOrEmpty(rsPath))
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xuất dữ liệu thành công",
                        path = "/" + _UploadFolder + "/" + _FileName
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xuất dữ liệu thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        /// <summary>
        /// Order Detail
        /// </summary>
        /// <param name="Id"> order identity</param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int Id)
        {
            var model = new OrderDetailViewModel();

            try
            {
                var _OrderInfo = _OrderRepository.GetOrderDetail(Id);
                var _OrderItemList = _OrderRepository.GetOrderItemList(Id);
                var _AmountVND = _OrderRepository.GetOrderTotalAmount(Id);
                var _ListOrderStatus = _CommonRepository.GetAllCodeByType(AllCodeType.ORDER_STATUS);
                var _ListPaymentType = _CommonRepository.GetAllCodeByType(AllCodeType.PAYMENT_TYPE);

                var order_info = await _OrderInfo;
                order_info.AmountVnd = await _AmountVND;
                model.OrderInfo = order_info;

                model.OrderItemList = await _OrderItemList;
                ViewBag.ListOrderStatus = await _ListOrderStatus;
                ViewBag.ListPaymentType = await _ListPaymentType;
            }
            catch
            {

            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(long OrderId, int OrderStatus, int PaymentType, DateTime PaymentDate)
        {
            try
            {
                string j_param = string.Empty;

                var order_entity = await _OrderRepository.FindAsync(OrderId);

                #region render log activity
                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                {
                    string orderStatusName = string.Empty, paymentTypeName = string.Empty;
                    var _UserName = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                    var _UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    var ListOrderStatus = await _CommonRepository.GetAllCodeByType(AllCodeType.ORDER_STATUS);
                    var ListPaymentType = await _CommonRepository.GetAllCodeByType(AllCodeType.PAYMENT_TYPE);

                    var OrderStatusModel = ListOrderStatus.FirstOrDefault(s => s.CodeValue == order_entity.OrderStatus);
                    var PaymentTypeModel = ListPaymentType.FirstOrDefault(s => s.CodeValue == order_entity.PaymentType);

                    var log_obj = new
                    {
                        user_type = 0,
                        user_id = _UserId,
                        user_name = _UserName,
                        j_data_log = JsonConvert.SerializeObject(new
                        {
                            order_status = order_entity.OrderStatus,
                            order_status_name = OrderStatusModel != null ? OrderStatusModel.Description : string.Empty,
                            payment_type = order_entity.OrderStatus,
                            payment_type_name = PaymentTypeModel != null ? PaymentTypeModel.Description : string.Empty,
                            payment_date = order_entity.PaymentDate
                        }),
                        log_type = 0,
                        key_word_search = order_entity.OrderNo
                    };

                    j_param = JsonConvert.SerializeObject(log_obj);
                }
                #endregion

                order_entity.OrderStatus = OrderStatus;
                order_entity.PaymentType = (short)PaymentType;
                order_entity.PaymentDate = PaymentDate != DateTime.MinValue ? PaymentDate : (DateTime?)null;

                var rs = await _OrderRepository.UpdateAsync(order_entity);

                //--Update Order Progress:
                var record = new OrderProgress()
                {
                    CreateDate = DateTime.Now,
                    OrderNo = order_entity.OrderNo,
                    OrderStatus = (short)order_entity.OrderStatus
                };
                var list = await _OrderProgressRepository.GetOrderProgressesByOrderNoAsync(record.OrderNo);
                if (list == null)
                {
                    await _OrderProgressRepository.SetOrderProgreess(record);
                }
                else
                {
                    bool alreadyExists = list.Any(x => x.OrderStatus == record.OrderStatus);
                    if (!alreadyExists)
                    {
                        await _OrderProgressRepository.SetOrderProgreess(record);

                    }
                }
                //-- Push to Old
                if (rs > 0)
                {
                    await PushOrderToUsOld(OrderId);

                    #region push log
                    if (!string.IsNullOrEmpty(j_param))
                    {
                        HttpClient httpClient = new HttpClient();
                        var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_LOG_ACTIVITY;
                        var KEY_TOKEN_API = ReadFile.LoadConfig().KEY_TOKEN_API;
                        var token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                        var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                        await httpClient.PostAsync(apiPrefix, content);
                    }
                    #endregion

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> OrderHistory(string OrderNo)
        {
            var models = await _OrderProgressRepository.GetOrderProgressesByOrderNoAsync(OrderNo);
            var KerryProgressList = new List<OrderKerryProgressModel>();
            try
            {
                using (var httpclient = new HttpClient())
                {
                    var str_url = "{\"token_key\":\"OpuA90w4ZHcW3ZfVYZbO3g==\",\"waybill_number\":\"" + OrderNo + "\"}";
                    var api_kerry = "https://gw.kerryexpress.com.vn/api/WS004GetOrderTrackingKe?data=" + Uri.EscapeDataString(str_url);
                    var response = await httpclient.GetAsync(api_kerry);
                    var content = await response.Content.ReadAsStringAsync();

                    var JsonData = JObject.Parse(content);
                    if (!(JsonData["status"] != null && JsonData["status"].ToString() == "error"))
                    {
                        var JsonTracking = JArray.Parse(JsonData["Tracking"].ToString());

                        KerryProgressList = JsonTracking.Select(s => new OrderKerryProgressModel
                        {
                            status_date = s["status_date"].ToString(),
                            warehouse = s["warehouse"].ToString(),
                            delivery_man = s["delivery_man"].ToString(),
                            phone_number = s["phone_number"].ToString(),
                            giaohangthanhcong = int.TryParse(s["giaohangthanhcong"].ToString(), out int g) ? g : 0
                        }).ToList();
                    }
                }
            }
            catch
            {

            }

            ViewBag.EstimatedDeliveryList = await _AutomaticPurchaseAmzRepository.GetEstimatedDeliveryDateByOrderNo(OrderNo);
            ViewBag.KerryProgressList = KerryProgressList;
            return PartialView(models);
        }

        public async Task<IActionResult> OrderAddress(long addressId, long orderId)
        {
            AddressModel model = null;
            try
            {
                var addressModel = await _ClientRepository.getAddressClientById(addressId);
                model = new AddressModel
                {
                    Id = addressModel.Id,
                    ReceiverName = addressModel.ReceiverName,
                    ClientId = addressModel.ClientId,
                    Phone = addressModel.Phone,
                    ProvinceId = addressModel.ProvinceId,
                    DistrictId = addressModel.DistrictId,
                    WardId = addressModel.WardId,
                    Address = addressModel.Address,
                    IsActive = addressModel.IsActive,
                    OrderId = orderId
                };

                var clientModel = await _ClientRepository.GetDetail(addressModel.ClientId);

                model.Email = clientModel != null ? clientModel.Email : string.Empty;

            }
            catch
            {

            }
            ViewBag.ProvinceList = await _CommonRepository.GetProvinceList();
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderAddress(AddressModel model)
        {
            try
            {
                var address = new AddressClientViewModel
                {
                    Id = model.Id,
                    ClientId = model.ClientId,
                    ReceiverName = model.ReceiverName,
                    IsActive = model.IsActive,
                    Phone = model.Phone,
                    ProvinceId = model.ProvinceId,
                    DistrictId = !string.IsNullOrEmpty(model.DistrictId) ? model.DistrictId : "-1",
                    WardId = !string.IsNullOrEmpty(model.WardId) ? model.WardId : "-1",
                    Address = model.Address,
                    CreatedOn = DateTime.Now,
                    Status = (int)StatusType.BINH_THUONG,
                    order_id = model.OrderId
                };

                HttpClient httpClient = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_SYNC_ADDRESS_CLIENT;
                var KEY_TOKEN_API = ReadFile.LoadConfig().KEY_TOKEN_API;
                string j_param = "{'address_item':'" + Newtonsoft.Json.JsonConvert.SerializeObject(address) + "'}";
                string token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                var status = (string)resultContent.status;

                if (status == ResponseType.SUCCESS.ToString())
                {
                    if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                    {
                        string j_action_log = "User " + HttpContext.User.FindFirst(ClaimTypes.Name).Value + " đã đổi thông tin địa chỉ giao hàng của đơn hàng " + address.order_id + " thành " + JsonConvert.SerializeObject(address);
                        LoggingActivity.AddLog(int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value), HttpContext.User.FindFirst(ClaimTypes.Name).Value, (int)LogActivityType.CHANGE_ORDER_CMS, j_action_log);
                    }
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = (string)resultContent.msg
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = (string)resultContent.msg
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<double> GetOrderTotalAmount(int Id)
        {
            return await _OrderRepository.GetOrderTotalAmount(Id);
        }

        #region Cashback
        public async Task<IActionResult> Cashback(int orderId)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            var model = await _CashbackRepository.GetListByOrderId(orderId);
            ViewBag.UserLogin = _UserLogin;
            return PartialView(model);
        }

        public async Task<IActionResult> SaveCashback(Cashback model)
        {
            try
            {
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                model.UserId = _UserLogin;

                long rs = 0;
                if (model.Id > 0)
                {
                    model.ModifiedOn = DateTime.Now;
                    rs = await _CashbackRepository.Update(model);
                }
                else
                {
                    model.ModifiedOn = DateTime.Now;
                    model.CreatedOn = DateTime.Now;
                    rs = await _CashbackRepository.Create(model);
                }

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> DeleteCashback(long cashbackId)
        {
            try
            {
                var rs = await _CashbackRepository.Delete(cashbackId);

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        #endregion

        #region Payment
        public async Task<IActionResult> Payment(int orderId)
        {
            var _UserLogin = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            var model = await _PaymentRepository.GetListByOrderId(orderId);
            // ViewBag.ListProduct = await _OrderRepository.GetOrderItemList(orderId);
            ViewBag.ListPaymentType = await _CommonRepository.GetAllCodeByType(AllCodeType.PAYMENT_TYPE);
            ViewBag.UserLogin = _UserLogin;
            return PartialView(model);
        }

        public async Task<IActionResult> SavePayment(Payment model)
        {
            try
            {
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                model.UserId = _UserLogin;

                long rs = 0;
                if (model.Id > 0)
                {
                    model.ModifiedOn = DateTime.Now;
                    rs = await _PaymentRepository.Update(model);
                }
                else
                {
                    model.ModifiedOn = DateTime.Now;
                    model.CreatedOn = DateTime.Now;
                    rs = await _PaymentRepository.Create(model);
                }

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> DeletePayment(long paymentId)
        {
            try
            {
                var rs = await _PaymentRepository.Delete(paymentId);

                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        #endregion

        public async Task<string> GetSuggestionOrder(string orderNo)
        {
            var data = await _OrderRepository.GetOrderSuggestionList(orderNo);
            return JsonConvert.SerializeObject(data);
        }

        public async Task<long> FindOrderIdByOrderNo(string orderNo)
        {
            return await _OrderRepository.FindOrderIdByOrderNo(orderNo);
        }

        public IActionResult MappingOrder()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MappingOrderJson(List<string> listOrder)
        {
            try
            {
                if (!listOrder.Any())
                {
                    return new JsonResult(new
                    {
                        Message = "Vui lòng nhập đơn hàng để đồng bộ",
                        Status = (int)ResponseType.FAILED,
                    });
                }
                HttpClient httpClient = new HttpClient();
                var apiQueueService = ReadFile.LoadConfig().API_CMS_URL
                    + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                foreach (var item in listOrder)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    var j_param = new Dictionary<string, string>()
                        {
                            {"data_push",item.Trim() },
                            { "type",TaskQueueName.order_old_convert_queue}
                        };
                    var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                    var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("token", token),
                        });
                    var result = await httpClient.PostAsync(apiQueueService, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    var status = (string)resultContent.status;
                    var msg = (string)resultContent.msg;
                    if (status != Constants.Success)
                    {
                        LogHelper.InsertLogTelegram("MappingOrderAsync - OrderController. Đồng bộ đơn hàng " + item + " thất bại. Chi tiết: " + msg);
                    }
                }
                return new JsonResult(new
                {
                    Message = "Đơn hàng đã được đồng bộ. Xin vui lòng bấm tìm kiếm để tải lại",
                    Status = (int)ResponseType.SUCCESS,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("MappingOrderAsync - OrderController" + ex);
                return new JsonResult(new
                {
                    Message = "Có lỗi xảy ra khi đồng bộ. Liên hệ kĩ thuật viên để biết thêm thông tin",
                    Status = (int)ResponseType.ERROR,
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MappingOneOrderJson(long orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    return new JsonResult(new
                    {
                        Message = "Vui lòng nhập đơn hàng để đồng bộ",
                        Status = (int)ResponseType.FAILED,
                    });
                }
                HttpClient httpClient = new HttpClient();
                var apiQueueService = ReadFile.LoadConfig().API_CMS_URL
                    + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                var orderInfo = _OrderRepository.FindOrderByOrderId(orderId).Result;
                if (orderInfo != null)
                {
                    var j_param = new Dictionary<string, string>()
                        {
                            {"data_push",orderInfo.OrderNo },
                            { "type",TaskQueueName.order_old_convert_queue}
                        };
                    var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                    var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("token", token),
                        });
                    var result = await httpClient.PostAsync(apiQueueService, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    var status = (string)resultContent.status;
                    var msg = (string)resultContent.msg;
                    if (status != Constants.Success)
                    {
                        LogHelper.InsertLogTelegram("MappingOrderAsync - OrderController." +
                            " Đồng bộ đơn hàng " + orderInfo.OrderNo + " thất bại. Chi tiết: " + msg);
                    }
                }
                return new JsonResult(new
                {
                    Message = "Đơn hàng đã được đồng bộ. Xin vui lòng bấm tìm kiếm để tải lại",
                    Status = (int)ResponseType.SUCCESS,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("MappingOrderAsync - OrderController" + ex);
                return new JsonResult(new
                {
                    Message = "Có lỗi xảy ra khi đồng bộ. Liên hệ kĩ thuật viên để biết thêm thông tin",
                    Status = (int)ResponseType.ERROR,
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PushOrderToUsOld(long orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    return new JsonResult(new
                    {
                        Message = "Vui lòng chọn đơn hàng để push sang hệ thống cũ",
                        Status = (int)ResponseType.FAILED,
                    });
                }
                HttpClient httpClient = new HttpClient();
                var apiQueueService = ReadFile.LoadConfig().API_CMS_URL
                    + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                var orderInfo = _OrderRepository.FindOrderByOrderId(orderId).Result;
                if (orderInfo != null)
                {
                    var j_param = new Dictionary<string, string>()
                        {
                            {"data_push", orderId.ToString() },
                            { "type",TaskQueueName.order_new_convert_queue}
                        };
                    var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                    var content = new FormUrlEncodedContent(new[]
                       {
                         new KeyValuePair<string, string>("token", token),
                        });
                    var result = await httpClient.PostAsync(apiQueueService, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    var status = (string)resultContent.status;
                    var msg = (string)resultContent.msg;
                    if (status != Constants.Success)
                    {
                        LogHelper.InsertLogTelegram("PushOrderToUsOld - OrderController." +
                            " Push đơn hàng " + orderInfo.OrderNo + " qua hệ thống cũ thất bại. Chi tiết: " + msg
                            + " Token = " + token);
                    }
                }
                return new JsonResult(new
                {
                    Message = "Đơn hàng đã được push sang hệ thống cũ",
                    Status = (int)ResponseType.SUCCESS,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PushOrderToUsOld - OrderController" + ex + ". OrderId = " + orderId);
                return new JsonResult(new
                {
                    Message = "Có lỗi xảy ra khi push order sang hệ thống cũ. Liên hệ kĩ thuật viên để biết thêm thông tin",
                    Status = (int)ResponseType.ERROR,
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PushOrderToAffilliate(int orderId, string aff_network)
        {
            int status = 0;
            string message = "Push đơn sang Accesstrade thành công";
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiQueueService = "";
                switch (aff_network)
                {
                    case "accesstrade":
                        {
                            apiQueueService = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().AT_CREATE_ORDER;
                        }
                        break;
                    case "adpia":
                        {
                            apiQueueService = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().ADPIA_CREATE_ORDER;
                        }
                        break;
                    default:
                        {
                            return new JsonResult(new
                            {
                                msg = "Affilliate chưa được hỗ trợ.",
                                status = (int)ResponseType.ERROR
                            });
                        }
                }
                string EncryptApi = ReadFile.LoadConfig().KEY_TOKEN_API;
                var orderInfo = _OrderRepository.FindOrderByOrderId(orderId).Result;
                var j_param = new Dictionary<string, string>()
                        {
                            {"order_id", orderId.ToString() },
                        };
                var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                var content = new FormUrlEncodedContent(new[]
                   {
                         new KeyValuePair<string, string>("token", token),
                        });
                var result = await httpClient.PostAsync(apiQueueService, content);
                message = await result.Content.ReadAsStringAsync();
                dynamic resultContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                status = Convert.ToInt32(resultContent["status"]);
                message = resultContent["msg"];

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PushToAT - OrderController" + ex.ToString() + ". OrderId = " + orderId + " .Msg = " + message);
                status = (int)ResponseType.ERROR;
                message = "Lỗi trong quá trình xử lý. Chi tiết: " + ex.ToString().Trim().Substring(0, 50);
            }
            return new JsonResult(new
            {
                msg = message,
                status = status
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderAffilliate(int orderId, string aff_network)
        {
            int status = 0;
            string message = "Push đơn sang Accesstrade thành công";
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiQueueService = "";
                switch (aff_network)
                {
                    case "accesstrade":
                        {
                            apiQueueService = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().AT_UPDATE_ORDER;
                        }
                        break;
                    case "adpia":
                        {
                            apiQueueService = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().ADPIA_UPDATE_ORDER;
                        }
                        break;
                    default:
                        {
                            return new JsonResult(new
                            {
                                msg = "Affilliate chưa được hỗ trợ.",
                                status = (int)ResponseType.ERROR
                            });
                        }
                }
                string EncryptApi = ReadFile.LoadConfig().KEY_TOKEN_API;
                var orderInfo = _OrderRepository.FindOrderByOrderId(orderId).Result;
                var j_param = new Dictionary<string, string>()
                        {
                            {"order_id", orderId.ToString() },
                            {"order_status", orderInfo.OrderStatus.ToString() }
                        };
                var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                var content = new FormUrlEncodedContent(new[]
                   {
                         new KeyValuePair<string, string>("token", token),
                        });
                var result = await httpClient.PostAsync(apiQueueService, content);
                message = await result.Content.ReadAsStringAsync();
                dynamic resultContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                status = Convert.ToInt32(resultContent["status"]);
                message = resultContent["msg"];

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAffilliate - OrderController" + ex.ToString() + ". OrderId = " + orderId + " .Msg = " + message);
                status = (int)ResponseType.ERROR;
                message = "Lỗi trong quá trình xử lý. Chi tiết: " + ex.ToString().Trim().Substring(0, 50);
            }
            return new JsonResult(new
            {
                msg = message,
                status = status
            });
        }
    }
}
