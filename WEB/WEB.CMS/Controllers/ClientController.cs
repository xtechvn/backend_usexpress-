using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Entities.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ClientController : Controller
    {
        private readonly ICommonRepository _CommonRepository;
        private readonly ILabelRepository _LabelRepository;
        private readonly IClientRepository _ClientRepository;
        private readonly IOrderRepository _OrderRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        public static string EncryptApi = "5fDmJ8Ze";
        public ClientController(ICommonRepository commonRepository, IOrderRepository orderRepository,
            ILabelRepository labelRepository, IClientRepository clientRepository, IWebHostEnvironment hostEnvironment)
        {
            _CommonRepository = commonRepository;
            _LabelRepository = labelRepository;
            _ClientRepository = clientRepository;
            _OrderRepository = orderRepository;
            _WebHostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var Date = Request.Query["Date"].ToString();
            var IsReturningClient = Request.Query["IsReturningClient"].ToString();
            var IsPaymentInDay = Request.Query["IsPaymentInDay"].ToString();

            ViewBag.FilterDate = string.IsNullOrEmpty(Date) ? string.Empty : Date.Replace("-", "/");
            ViewBag.IsReturningClient = string.IsNullOrEmpty(IsReturningClient) ? 0 : 1;
            ViewBag.IsPaymentInDay = string.IsNullOrEmpty(IsPaymentInDay) ? 0 : 1;
            ViewBag.ProvinceList = await _CommonRepository.GetProvinceList();
            ViewBag.LabelList = _LabelRepository.GetListAll();
            ViewBag.RevenueMinMax = await _OrderRepository.GetMinMaxOrderAmount();
            return View();
        }

        [HttpPost]
        public async Task<string> GetDistrictList(string provinceId)
        {
            var _DistrictList = await _CommonRepository.GetDistrictListByProvinceId(provinceId);
            return JsonConvert.SerializeObject(_DistrictList.Select(s => new
            {
                Value = s.DistrictId,
                Text = s.Name
            }));
        }

        [HttpPost]
        public async Task<string> GetWardList(string districtId)
        {
            var _WardList = await _CommonRepository.GetWardListByDistrictId(districtId);
            return JsonConvert.SerializeObject(_WardList.Select(s => new
            {
                Value = s.WardId,
                Text = s.Name
            }));
        }

        [HttpPost]
        public IActionResult Search(ClientSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<ClientListingModel>();
            try
            {
                model = _ClientRepository.GetPagingList(searchModel, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }

        /// <summary>
        /// Report Client
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ExportExcel(ClientSearchModel searchModel)
        {
            try
            {
                string _FileName = "Client_" + Guid.NewGuid() + ".xlsx";
                string _UploadFolder = @"Template/Export";
                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                if (!Directory.Exists(_UploadDirectory))
                {
                    Directory.CreateDirectory(_UploadDirectory);
                }

                string FilePath = Path.Combine(_UploadDirectory, _FileName);

                var rsPath = _ClientRepository.ReportClient(searchModel, FilePath);

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

        [HttpPost]
        public async Task<IActionResult> Detail(int Id, int tabActive)
        {
            var model = new ClientDetailViewModel();
            try
            {
                model.Detail = await _ClientRepository.GetDetail(Id);
                model.AddressList = await _ClientRepository.GetClientAddressList(Id);
                model.OrderList = await _OrderRepository.GetOrderListByClientId((long)Id);
                model.ReferralOrderList = await _OrderRepository.GetOrderListByReferralId(model.Detail.ReferralId);
            }
            catch
            {

            }
            ViewBag.TabActive = tabActive;
            return PartialView(model);
        }

        [HttpPost]
        public IActionResult History(int Id)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<JsonResult> ResetPassword(long clientId)
        {
            try
            {
                var rs = await _ClientRepository.ResetPassword(clientId);
                if (!string.IsNullOrEmpty(rs))
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        result = rs
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

        /// <summary>
        /// reset mật khẩu mặc định về 123456 - chỉ dành cho push reset qua cms old
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ResetPasswordDefault(long clientId)
        {
            try
            {
                var clientInfo = await _ClientRepository.getClientDetail(clientId);
                var apiPrefix = ReadFile.LoadConfig().API_USEXPRESS + ReadFile.LoadConfig().API_RESET_PASSWORD_DEFAULT;
                //var apiPrefix =/* ReadFile.LoadConfig().API_USEXPRESS */ "http://qc.oldversion.revivifyvietnam.com/" + ReadFile.LoadConfig().API_RESET_PASSWORD_DEFAULT;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API;
                var passwordDefault = EncodeHelpers.MD5Hash("123456");
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string> {
                        { "client_map_id", clientInfo.ClientMapID.ToString()},
                        { "password_new",passwordDefault}};
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                var content = new FormUrlEncodedContent(new[]
                      {
                         new KeyValuePair<string, string>("token", token),
                     });
                //LogHelper.InsertLogTelegram("ClientMapId - ResetPasswordDefault: " + clientId);
                //LogHelper.InsertLogTelegram("Token - ResetPasswordDefault: " + token);
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                if (resultContent.status == "failed")
                {
                    LogHelper.InsertLogTelegram("CMS: Reset password default fail. Result: " + resultContent.status + ". Message: " + resultContent.msg
                        + " . token = " + resultContent.token);
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = resultContent.msg
                    });
                }
                else
                {
                    //cập nhật mật khẩu ở cms new
                    clientInfo.PasswordBackup = clientInfo.Password;
                    clientInfo.Password = passwordDefault;
                    var rs = await _ClientRepository.UpdateClient(clientInfo);
                    if (rs > 0)
                    {
                        return new JsonResult(new
                        {
                            isSuccess = true,
                            message = "Mật khẩu đã được Reset về 123456 bên hệ thống cũ"
                        });
                    }
                    else
                    {
                        return new JsonResult(new
                        {
                            isSuccess = true,
                            message = "Mật khẩu đã được Reset về 123456 bên hệ thống cũ, cập nhật mật khẩu bên hệ thống mới thất bại"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS: ResetPasswordDefault-ClientController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ChangeStatus(long clientId)
        {
            try
            {
                var rs = await _ClientRepository.ChangeStatus(clientId);
                if (rs != -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        status = rs
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

        [HttpPost]
        public async Task<JsonResult> ChangeDefaultAddress(long addressId)
        {
            try
            {
                var rs = await _ClientRepository.ChangeDefaultAddress(addressId);
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
        public async Task<int> PutPassword(int clientId, string password_new)
        {
            string token = "";
            try
            {
                var apiPrefix = "http://usexpress.vn/client/ressetpassword";
                string j_param = "{'password_new':'" + password_new + "','client_id':'" + clientId + "'}";
                token = CommonHelper.Encode(j_param, EncryptApi);

                HttpClient httpClient = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token)
                });

                var rs = httpClient.PostAsync(apiPrefix, content).Result;
                dynamic result = JObject.Parse(rs.Content.ReadAsStringAsync().Result);
                if (result.status == ResponseType.FAILED || result.status == ResponseType.ERROR)
                {
                    LogHelper.InsertLogTelegram("Loi cap nhat password moi cho client Id = " + clientId + " - ClientController " + result.msg);
                    return result.status == ResponseType.FAILED;
                }
                return (int)ResponseType.SUCCESS;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("putPassword - ClientController " + ex.Message);
                return (int)ResponseType.ERROR;
            }
        }
        public IActionResult MappingClient()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MappingClientJson(List<string> listClient)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiQueueService = ReadFile.LoadConfig().API_CMS_URL
                   + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                foreach (var item in listClient)
                {
                    var j_param = new Dictionary<string, string>()
                        {
                            {"data_push",item.Trim() },
                            { "type",TaskQueueName.client_old_convert_queue}
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
                        LogHelper.InsertLogTelegram("MappingClientJson - ClientController. Đồng bộ đơn hàng " + item + " thất bại. Chi tiết: " + msg);
                    }
                }
                return new JsonResult(new
                {
                    Message = "Khách hàng đã được đồng bộ. Xin vui lòng bấm tìm kiếm để tải lại",
                    Status = (int)ResponseType.SUCCESS,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("MappingClientJson - ClientController" + ex);
                return new JsonResult(new
                {
                    Message = "Có lỗi xảy ra khi đồng bộ. Liên hệ kĩ thuật viên để biết thêm thông tin",
                    Status = (int)ResponseType.ERROR,
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PushToUsexpressOld(int clientId)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiQueueService = ReadFile.LoadConfig().API_CMS_URL
                   + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                var address_id = await _ClientRepository.GetAddressIDByClientID(clientId);
                var client_detail = new
                {
                    client_id = clientId,
                    address_id = address_id,
                    order_id = -1
                };
                var j_param = new Dictionary<string, string>()
                    {
                        {"data_push",JsonConvert.SerializeObject(client_detail) },
                        { "type",TaskQueueName.client_new_convert_queue}
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
                    LogHelper.InsertLogTelegram("PushToUsexpressOld - ClientController" +
                        ". Push khách hàng" + clientId + " to usexpress old thất bại. Chi tiết: " + msg);
                }
                return new JsonResult(new
                {
                    Message = "Khách hàng đã được push sang usexpress old",
                    Status = (int)ResponseType.SUCCESS,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PushToUsexpressOld - ClientController" + ex);
                return new JsonResult(new
                {
                    Message = "Có lỗi xảy ra khi đồng bộ. Liên hệ kĩ thuật viên để biết thêm thông tin",
                    Status = (int)ResponseType.ERROR,
                });
            }
        }
    }
}