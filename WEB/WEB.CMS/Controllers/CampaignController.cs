using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using App_Crawl_SearchList_Receiver.Models;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    public class CampaignController : Controller
    {
        private const int US_CATEGORY_ID = 90;
        private readonly ILabelRepository _LabelRepository;
        private readonly IPositionRepository _PositionRepository;
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly ICampaignAdsRepository _CampaignAdsRepository;
        private readonly IProductClassificationRepository _ProductClassificationRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IConfiguration _configuration;

        public CampaignController(IWebHostEnvironment hostEnvironment, IPositionRepository positionRepository,
               ILabelRepository labelRepository, IProductClassificationRepository productClassificationRepository,
               ICampaignAdsRepository campaignAdsRepository, IGroupProductRepository groupProductRepository, IConfiguration configuration)
        {
            _WebHostEnvironment = hostEnvironment;
            _LabelRepository = labelRepository;
            _PositionRepository = positionRepository;
            _CampaignAdsRepository = campaignAdsRepository;
            _GroupProductRepository = groupProductRepository;
            _ProductClassificationRepository = productClassificationRepository;
            _configuration = configuration;

        }

        public IActionResult Index()
        {
            ViewBag.ListLabel = _LabelRepository.GetListAll();
            ViewBag.ListCampaignAds = _CampaignAdsRepository.GetListAll();
            return View();
        }

        public IActionResult Add()
        {
            ViewBag.ListLabel = _LabelRepository.GetListAll();
            ViewBag.ListCampaignAds = _CampaignAdsRepository.GetListAll();
            return View();
        }

        public IActionResult Error(int Id)
        {
            var model = _ProductClassificationRepository.GetById(Id);
            return View(model.Result);
        }

        public IActionResult Confirm()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SearchData(string fromTime, string toTime, List<int> listLabelId, string listCampaignId,
            string strLink, int pageSize = 8, int currentPage = 1, int status = -1)
        {
            try
            {
                var model = _ProductClassificationRepository.GetPagingList(fromTime, toTime, currentPage, listLabelId, strLink,
                    listCampaignId, pageSize, status);

                model.ListData.AsParallel().ForAll(o =>
                {
                    var labelInfo = _LabelRepository.GetById(o.LabelId.Value);
                    var campaignInfo = _CampaignAdsRepository.GetById(o.CapaignId.Value);
                    var groupInfo = _GroupProductRepository.GetById(o.GroupIdChoice);
                    o.LabelName = labelInfo != null ? labelInfo.Result.StoreName : "";
                    o.GroupName = groupInfo != null ? groupInfo.Result.Name : "";
                    o.CampaignName = campaignInfo != null ? campaignInfo.Result.CampaignName : "";
                });
                return PartialView(model);
            }
            catch (Exception ex)
            {
                var model = new GenericViewModel<CampaignAdsViewModel>();
                LogHelper.InsertLogTelegram("GetAllGroup: " + ex.Message);
                return PartialView(model);
            }
        }

        public async Task<IActionResult> GetDetail(int Id)
        {
            var model = new ProductClassification();
            var productModel = new ProductClassificationViewModel();
            try
            {
                model = await _ProductClassificationRepository.GetById(Id);
                productModel.CapaignId = model.CapaignId;
                productModel.CreateTime = model.CreateTime;
                productModel.FromDate = model.FromDate;
                productModel.ToDate = model.ToDate;
                productModel.UserId = model.UserId;
                productModel.Status = model.Status;
                //productModel.ProductId = model.ProductId;
                productModel.Note = model.Note;
                productModel.LinkProductTarget = model.LinkProductTarget;
                productModel.Link = model.Link;
                productModel.LabelId = model.LabelId;
                productModel.Id = model.Id;
                productModel.GroupIdChoice = model.GroupIdChoice;
                var labelInfo = _LabelRepository.GetById(productModel.LabelId.Value);
                var campaignInfo = _CampaignAdsRepository.GetById(productModel.CapaignId.Value);
                var groupInfo = _GroupProductRepository.GetById(productModel.GroupIdChoice);
                productModel.LabelName = labelInfo != null ? labelInfo.Result.StoreName : "";
                productModel.GroupName = groupInfo != null ? groupInfo.Result.Name : "";
                productModel.CampaignName = campaignInfo != null ? campaignInfo.Result.CampaignName : "";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - CampaignController: " + ex.Message);
            }
            return View(productModel);
        }

        public string GetCampaignSuggestionList(string name)
        {
            try
            {
                var list = _CampaignAdsRepository.GetListAll();
                if (!string.IsNullOrEmpty(name))
                {
                    list = list.Where(s => StringHelpers.ConvertStringToNoSymbol(s.CampaignName.Trim().ToLower())
                                                   .Contains(StringHelpers.ConvertStringToNoSymbol(name.Trim().ToLower())))
                                                   .ToList();
                }
                var suggestionlist = list.Take(10).Select(s => new
                {
                    id = s.Id,
                    name = s.CampaignName
                }).ToList();
                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IActionResult Delete(int Id)
        {
            try
            {
                var rs = _ProductClassificationRepository.Delete(Id);
                return new JsonResult(new
                {
                    isSuccess = true,
                    message = "Xóa thành công."
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        #region Campaign Management
        public async Task<IActionResult> CampaignManagement()
        {
            ViewBag.CampaignList = await _CampaignAdsRepository.GetListAllAsync();
            return PartialView();
        }

        public async Task<string> GetJsonCampaignList()
        {
            var CampaignList = await _CampaignAdsRepository.GetListAllAsync();
            var dataResult = CampaignList.Select(s => new
            {
                id = s.Id,
                name = s.CampaignName,
                start_date = s.StartDate != null ? s.StartDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                end_date = s.EndDate != null ? s.EndDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                social_link = s.ScriptSocial
            });

            return JsonConvert.SerializeObject(dataResult);
        }

        public async Task<IActionResult> SaveCampaign(int Id, string Name)
        {
            try
            {
                var rs = await _CampaignAdsRepository.Upsert(Id, Name);
                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật dữ liệu thành công",
                        camp_id = rs
                    });
                }
                else if (rs == -2)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tên chiến dịch đã tồn tại. Vui lòng nhập lại"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật dữ liệu thất bại"
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

        public async Task<string> GetCategoryTreeViewOfCampaign(int campaignId)
        {
            var _ListGroupProduct = await _CampaignAdsRepository.GetListGroupProductIdByCampaignId(campaignId);
            return await _GroupProductRepository.GetListTreeViewCheckBox(US_CATEGORY_ID, -1, _ListGroupProduct, true);
        }

        public async Task<IActionResult> SaveCampaignGroupProduct(int campaignId, DateTime? startDate, DateTime? endDate, string socialLink, List<int> arrGroupProduct)
        {
            try
            {

                var model = await _CampaignAdsRepository.GetById(campaignId);
                model.StartDate = startDate;
                model.EndDate = endDate;
                model.ScriptSocial = socialLink;
                var rs = await _CampaignAdsRepository.UpdateData(model);

                if (rs < 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật chiến dịch thất bại",
                    });
                }

                rs = await _CampaignAdsRepository.MultipleInsertCampaignGroupProduct(campaignId, arrGroupProduct);

                if (rs > 0)
                {
                    if (arrGroupProduct != null && arrGroupProduct.Count > 0)
                    {
                        await ClearCampaignCache(campaignId);
                    }

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật dữ liệu thành công",
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật dữ liệu thất bại"
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

        public async Task<IActionResult> GetDetailCampaignGroupProduct(int campaignId, int groupProductId)
        {
            try
            {
                var rs = await _CampaignAdsRepository.DetailCampaignGroupProduct(campaignId, groupProductId);
                if (rs != null)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật dữ liệu thành công",
                        data = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Có lỗi trong quá trình kết nối"
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

        public async Task<IActionResult> SaveInfoCampaignGroupProduct(CampaignGroupProduct model)
        {
            try
            {
                var rs = await _CampaignAdsRepository.SaveInfoCampaignGroupProduct(model);
                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật dữ liệu thành công",
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật dữ liệu thất bại"
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

        public async Task ClearCampaignCache(int campaignId)
        {
            string token = string.Empty;
            try
            {
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_CLEAR_CACHE;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API;
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string> {
                    { "value", campaignId.ToString() },
                    { "cache_type", CacheType.CAMPAIGN_ID_ }
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                await httpClient.PostAsync(apiPrefix, content);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClearCampaignCache - " + ex.Message.ToString() + " Token:" + token);
                throw ex;
            }
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> DoCrawl(SLQueueItem item)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiQueueService = ReadFile.LoadConfig().API_CMS_URL
                   + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                switch (item.labelid)
                {
                    case 1:
                        {
                            string product_code; int _stt_code = 0; string _message = "Lỗi trong quá trình xử lý - Not Implemented.";
                            CommonHelper.CheckAsinByLink(item.linkdetail, out product_code);
                            if (product_code == null || product_code == "")
                            {
                                var j_param = new Dictionary<string, string>()
                                {
                                    {"data_push",JsonConvert.SerializeObject(item) },
                                    { "type",TaskQueueName.group_product_mapping}
                                };
                                var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                                var content = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("token", token),
                                });
                                var result_api = await httpClient.PostAsync(apiQueueService, content);
                                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result_api.Content.ReadAsStringAsync().Result);
                                var status = (string)resultContent.status;
                                var msg = (string)resultContent.msg;
                                if (status != Constants.Success)
                                {
                                    LogHelper.InsertLogTelegram("DoCrawlAsync - CampaignController. Push Queue '" + TaskQueueName.group_product_mapping + "'.Data: " + JsonConvert.SerializeObject(item) + ".\n Chi tiết: Status = " + status + ". Msg: " + msg);
                                    _stt_code = 1;
                                    _message = "Gửi yêu cầu Crawl thất bại. Chi tiết :  " + msg;
                                }
                                else
                                {
                                    _stt_code = 1;
                                    _message = "Gửi yêu cầu Crawl thành công";
                                }
                            }
                            else
                            {
                                SLProductItem product = new SLProductItem
                                {
                                    product_code = product_code,
                                    label_id = item.labelid,
                                    from_parent_url = null,
                                    group_id = item.groupProductid,
                                    url = item.linkdetail
                                };
                                var j_param = new Dictionary<string, string>()
                                {
                                    {"data_push",JsonConvert.SerializeObject(product) },
                                    { "type",TaskQueueName.group_product_mapping_detail}
                                };
                                var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                                var content = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("token", token),
                                });
                                var result_api = await httpClient.PostAsync(apiQueueService, content);
                                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result_api.Content.ReadAsStringAsync().Result);
                                var status = (string)resultContent.status;
                                var msg = (string)resultContent.msg;
                                if (status != Constants.Success)
                                {
                                    LogHelper.InsertLogTelegram("DoCrawlAsync - CampaignController. Push Queue '" + TaskQueueName.group_product_mapping_detail + "'.Data: " + JsonConvert.SerializeObject(product) + ".\n Chi tiết: Status = " + status + ". Msg: " + msg);
                                    _stt_code = 1;
                                    _message = "Gửi yêu cầu Crawl thất bại. Chi tiết :  " + msg;
                                }
                                else
                                {
                                    _stt_code = 1;
                                    _message = "Gửi yêu cầu Crawl Detail thành công";
                                }
                                _stt_code = 1;
                                _message = "Gửi yêu cầu Crawl Detail thành công";
                            }
                            return new JsonResult(new
                            {
                                Code = _stt_code,
                                Message = _message
                            });
                        }
                    default:
                        {
                            return new JsonResult(new
                            {
                                Code = 0,
                                Message = "Nhãn hàng đang được hoàn thiện, vui lòng sử dụng nhãn hàng khác."
                            });
                        }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DoCrawl - CampaignController: " + ex.ToString());
                return new JsonResult(new
                {
                    Code = 0,
                    Message = "Lỗi trong quá trình xử lý"
                });
            }

        }

    }
}