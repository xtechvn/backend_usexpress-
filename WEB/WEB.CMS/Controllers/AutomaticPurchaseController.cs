using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.AutomaticPurchase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Common;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    public class AutomaticPurchaseController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IAutomaticPurchaseAmzRepository _automaticPurchaseAmzRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        public AutomaticPurchaseController( IConfiguration configuration, IAutomaticPurchaseAmzRepository automaticPurchaseAmzRepository, IAllCodeRepository allCodeRepository)
        {
            _configuration = configuration;
            _automaticPurchaseAmzRepository = automaticPurchaseAmzRepository;
            _allCodeRepository = allCodeRepository;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.List_AutomaticBuy_Status = await _allCodeRepository.GetListSortByName(AllCodeType.AUTOMATICPURCHASE_STATUS);
            return View();
        }
        /// <summary>
        /// Search
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Search(AutomaticPurchaseSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<AutomaticPurchaseAmzViewModel>();
            try
            {
                model = await _automaticPurchaseAmzRepository.GetPagingList(searchModel, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }
        public IActionResult Detail()
        {
            try
            {
                var automatic_id = Convert.ToInt64(Request.Query["id"].ToString());
                ViewBag.Id = automatic_id;
            }
            catch (Exception)
            {
                ViewBag.Id = -1;
            }
            ViewBag.Purchase_Status =  _allCodeRepository.GetListByType(AllCodeType.AUTOMATICPURCHASE_STATUS);
            ViewBag.Delivering_Status = _allCodeRepository.GetListByType(AllCodeType.ORDERDELIVERY_STATUS);

            return View();
        }
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var model = await _automaticPurchaseAmzRepository.GetById(id);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = JsonConvert.SerializeObject(model),
                });

            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS - AutomaticPurchaseController - GetDetail - Error: " + ex.ToString());

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                data=""
            });
        }
        [HttpPost]
        public string GetProductCodeFromURL(string url)
        {
            try
            {
                var pcode = "";
                var product_code_check = CommonHelper.CheckAsinByLink(url, out pcode);
                if (product_code_check)
                    return pcode;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS - AutomaticPurchaseController - GetProductCodeFromURL - Error: " + ex.ToString());

            }
            return "";
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAutoPurchaseDetail(AutomaticPurchaseAmz data)
        {
            string msg = "";
            int status = (int)ResponseType.FAILED;
            try
            {
                if(data.Amount<=0 || data.Quanity<=0|| data.PurchaseUrl==null || data.PurchaseUrl.Trim() == "" ||data.PurchaseStatus<0 
                    || data.OrderMappingId == null || data.OrderMappingId.Trim() == "" || data.AutoBuyMappingId <= 0)
                {
                    msg = "Dữ liệu không chính xác, vui lòng thử lại.";
                }
                else
                {
                    var exists = await _automaticPurchaseAmzRepository.GetById(data.Id);
                    /*
                    if(exists.PurchaseStatus==(int)AutomaticPurchaseStatus.New || exists.PurchaseStatus == (int)AutomaticPurchaseStatus.PurchaseSuccess || exists.PurchaseStatus == (int)AutomaticPurchaseStatus.PurchaseCancelled)
                    {
                        return Ok(new
                        {
                            status = status,
                            msg = "Không thể thay đổi thông tin mua tự động, vui lòng kiểm tra lại."
                        });
                    }*/
                    var product_code = GetProductCodeFromURL(data.PurchaseUrl);
                    if (product_code == null && product_code.Trim()=="")
                    {
                        return Ok(new
                        {
                            status = status,
                            msg = "Dữ liệu không chính xác, vui lòng thử lại."
                        });
                    }
                    if (exists == null && exists.Id!=data.Id)
                    {
                        return Ok(new
                        {
                            status = status,
                            msg = "Không tìm thấy dữ liệu"
                        });
                    }
                    exists.Amount = data.Amount;
                    exists.PurchaseUrl = data.PurchaseUrl;
                    exists.ProductCode = product_code;
                    exists.Quanity = data.Quanity;
                    exists.PurchaseStatus = data.PurchaseStatus;
                    exists.OrderedSuccessUrl= data.OrderedSuccessUrl;
                    exists.OrderDetailUrl= data.OrderDetailUrl;
                    exists.PurchasedSellerStoreUrl= data.PurchasedSellerStoreUrl;
                    exists.DeliveryMessage = data.DeliveryMessage;
                    exists.ManualNote = data.ManualNote;

                    if (data.PurchaseStatus == (int)AutomaticPurchaseStatus.PurchaseSuccess)
                    {
                        exists.PurchaseMessage = "Manual Purchase Success. Time: " + DateTime.Now.ToString("G");
                    }
                    var update = await _automaticPurchaseAmzRepository.UpdatePurchaseDetail(exists);

                    string j_action_log = "User " + HttpContext.User.FindFirst(ClaimTypes.Name).Value + "cập nhật thông tin mua tự động cho [" + exists.OrderCode + " - "+exists.ProductCode+"] : " + JsonConvert.SerializeObject(exists);
                    LoggingActivity.AddLog(int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value), HttpContext.User.FindFirst(ClaimTypes.Name).Value, (int)LogActivityType.AUTOMATIC_PURCHASE_CHANGE, j_action_log);
                    if (update == (int)ResponseType.SUCCESS)
                    {
                       await UpdateToDBOld(exists);
                        msg = "Cập nhật thành công";
                        status = (int)ResponseType.SUCCESS;
                    }
                    else
                    {
                        msg = "Cập nhật thất bại.";
                    }
                }
            }
            catch (Exception ex)
            {
                status = (int)ResponseType.ERROR;
                LogHelper.InsertLogTelegram("CMS - AutomaticPurchaseController - UpdateAutoPurchaseDetail: " + ex);
            }
            return Ok(new
            {
                status = status,
                msg = msg
            });

        }
        private async Task UpdateToDBOld(AutomaticPurchaseAmz data)
        {
            try
            {
                string key = ReadFile.LoadConfig().API_OLD_KEY;
                HttpClient httpClient_2 = new HttpClient();
                httpClient_2.DefaultRequestHeaders.Accept.Clear();
                httpClient_2.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var apiQueueService = ReadFile.LoadConfig().API_OLD_UPDATE_AUTO_PURCHASE;
                var token_2 = JsonConvert.SerializeObject(data);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(token_2);
                var token = System.Convert.ToBase64String(plainTextBytes);
                var queryString = new StringContent("\"" + token + "\"", Encoding.UTF8, "text/html");
                var result_2 = await httpClient_2.PostAsync(apiQueueService + "?key=" + key, queryString);
                dynamic resultContent_2 = Newtonsoft.Json.Linq.JObject.Parse(result_2.Content.ReadAsStringAsync().Result);
                var status = (int)resultContent_2.Code;
                if (status != (int)ResponseType.SUCCESS)
                {
                    LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - UpdateToDBOld - " + data.AutoBuyMappingId + " \nError: " + JsonConvert.SerializeObject(resultContent_2));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS - AutomaticPurchaseController - UpdateToDBOld - Error: " + ex.ToString());

            }  
        }
    }
}
