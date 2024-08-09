using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
using System.IO;
using Aspose;
using Aspose.Cells;
using Utilities;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;
using WEB.CMS.Customize;
using Nest;
using Utilities.Contants;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using WEB.CMS.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Entities.ConfigModels;
using App_Crawl_SearchList_Receiver.Models;
using Microsoft.Extensions.Configuration;
using Caching.RedisWorker;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class GroupProductController : Controller
    {
        private const int NEWS_CATEGORY_ID = 91;
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly ILabelRepository _LabelRepository;
        private readonly IPositionRepository _PositionRepository;
        private readonly IAllCodeRepository _AllCodeRepository;
        private readonly ICampaignAdsRepository _CampaignAdsRepository;
        private readonly IProductClassificationRepository _ProductClassificationRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly string _UrlStaticImage;
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;

        public GroupProductController(IGroupProductRepository groupProductRepository,
               IWebHostEnvironment hostEnvironment, IPositionRepository positionRepository,
               ILabelRepository labelRepository, IProductClassificationRepository productClassificationRepository,
               RedisConn redisService, ICampaignAdsRepository campaignAdsRepository, IAllCodeRepository allCodeRepository, IOptions<DomainConfig> domainConfig, IConfiguration configuration)
        {
            _GroupProductRepository = groupProductRepository;
            _WebHostEnvironment = hostEnvironment;
            _LabelRepository = labelRepository;
            _PositionRepository = positionRepository;
            _CampaignAdsRepository = campaignAdsRepository;
            _ProductClassificationRepository = productClassificationRepository;
            _AllCodeRepository = allCodeRepository;
            _UrlStaticImage = domainConfig.Value.ImageStatic;
            _configuration = configuration;
            _redisService = redisService;

        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Search(string Name, int Status = -1)
        {
            return await _GroupProductRepository.GetListTreeView(Name, Status);
        }

        /// <summary>
        /// Add Or Update GroupProduct
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">
        /// 0: Add child
        /// 1: Edit itseft
        /// </param>
        /// <returns></returns>
        public async Task<IActionResult> AddOrUpdate(int id, int type)
        {
            var model = new GroupProductDetailModel();
            try
            {
                if (type == 0)
                {
                    model = new GroupProductDetailModel()
                    {
                        Id = 0,
                        Status = 0,
                        OrderNo = 0,
                        ParentId = id
                    };
                }
                else
                {
                    var entity = await _GroupProductRepository.GetById(id);
                    model = new GroupProductDetailModel()
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        ImagePath = !string.IsNullOrEmpty(entity.ImagePath) ? _UrlStaticImage + entity.ImagePath : entity.ImagePath,
                        OrderNo = entity.OrderNo,
                        ParentId = entity.ParentId,
                        Status = entity.Status,
                        IsShowHeader = entity.IsShowHeader,
                        IsShowFooter = entity.IsShowFooter,
                        IsBrandBox = entity.IsBrandBox,
                        PositionId = entity.PositionId,
                        Description = entity.Description,
                        GroupProductStores = await _GroupProductRepository.GetGroupProductStoresByGroupProductId(id)
                    };
                }
            }
            catch
            {

            }

            ViewBag.PositionList = await _PositionRepository.GetAll();
            ViewBag.LabelList = _LabelRepository.GetListAll();
            return View(model);
        }

        /// <summary>
        /// public async Task<IActionResult> UpSert(IFormFile imageFile, string imageSize, GroupProductUpsertModel model)
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="imageSize"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpSert(GroupProductUpsertModel model)
        {
            try
            {
            

                var ListGroupProductStore = JsonConvert.DeserializeObject<List<GroupProductStore>>(model.GroupProductStores);

                var upsertModel = new GroupProduct()
                {
                    Id = model.Id,
                    Name = model.Name,
                    OrderNo = model.OrderNo,
                    ParentId = model.ParentId,
                    Description = model.Description,
                    PositionId = model.PositionId,
                    Status = model.Status,
                    IsShowHeader = model.IsShowHeader,
                    IsShowFooter = model.IsShowFooter,
                    IsBrandBox = model.IsBrandBox,
                    ImagePath = await UpLoadHelper.UploadBase64Src(model.ImageBase64, _UrlStaticImage),
                    LinkCount = (ListGroupProductStore != null && ListGroupProductStore.Count > 0) ? ListGroupProductStore.Count : 0
                };

                
                var rs = await _GroupProductRepository.UpSert(upsertModel);

                if (rs > 0)
                {
                    bool _IsHasLink = false;
                    var ListData = new List<GroupProductStore>();

                    if (ListGroupProductStore != null && ListGroupProductStore.Count > 0)
                    {
                        _IsHasLink = true;
                        ListData = ListGroupProductStore.Select(s => new GroupProductStore
                        {
                            Id = 0,
                            GroupProductId = rs,
                            LabelId = s.LabelId,
                            LinkStoreMenu = s.LinkStoreMenu,
                            CreatedOn = DateTime.Now
                        }).ToList();
                    }
                    await _GroupProductRepository.UpsertGroupProductStores(model.Id, ListData);

                    // clear cache category
                    await ClearCacheCategory(rs);

                    var rootParentId = await _GroupProductRepository.GetRootParentId(rs);
                    if (rootParentId == NEWS_CATEGORY_ID) await ClearCacheByKey(CacheType.MENU_CATEGORY_NEWS);

                    await ClearCacheByKey(CacheType.MENU_GROUP_PRODUCT);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        modelId = rs,
                        isHasLink = _IsHasLink
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tồn tại nhóm hàng cùng cấp"
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

        public async Task<IActionResult> GetGroupProductCrawledCount()
        {
            try
            {
                var rs = await _GroupProductRepository.GetListGroupProductCrawled();

                if (rs != null)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "succeed",
                        crawlOn = rs.Count(s => (s ?? 0) == 1),
                        crawlOff = rs.Count(s => (s ?? 0) == 0)
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lỗi tải dữ liệu",
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

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rootParentId = await _GroupProductRepository.GetRootParentId(id);
                var rs = await _GroupProductRepository.Delete(id);

                if (rs > 0)
                {
                    // clear cache category
                    await ClearCacheCategory(id);

                    // clear cache news menu
                    if (rootParentId == NEWS_CATEGORY_ID) await ClearCacheByKey(CacheType.MENU_NEWS);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thành công."
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Nhóm hàng đang được sử dụng. Bạn không thể xóa."
                    });
                }
                else if (rs == -2)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Nhóm hàng đang có cấp con. Bạn không thể xóa."
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thất bại."
                    });
                }
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

        /// <summary>
        /// Setup Auto Crawler
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">
        /// 1: true
        /// 0: false
        /// </param>
        /// <returns></returns>
        public async Task<IActionResult> SetupAutoCrawler(int id, int type)
        {
            try
            {
                var rs = await _GroupProductRepository.UpdateAutoCrawler(id, type);
                if (rs > 0)
                {
                    // clear cache category
                    await ClearCacheCategory(id);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật trạng thái tự động Crawl thành công."
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật trạng thái tự động Crawl thất bại."
                    });
                }
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

        public async Task<IActionResult> SetupAffiliateCategory(int cateId, int affId, int type)
        {
            try
            {
                var rs = await _GroupProductRepository.UpdateAffiliateCategory(cateId, affId, type);
                if (rs > 0)
                {
                    //// clear cache category
                    //await ClearCacheCategory(id);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật trạng thái Affiliate thành công."
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật trạng thái Affiliate thất bại."
                    });
                }
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

        [HttpPost]
        public string GetAffiliateList()
        {
            try
            {
                var listAllCode = _AllCodeRepository.GetListByType(AllCodeType.AFF_NAME);
                var listAffCate = listAllCode.Select(s => new
                {
                    id = s.CodeValue,
                    name = s.Description
                });

                return JsonConvert.SerializeObject(listAffCate);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAffiliateList: " + ex);
                return null;
            }
        }

        public async Task ClearCacheCategory(int categoryId)
        {
            string token = string.Empty;
            try
            {
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_SYNC_CATEGORY;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API;
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string> { { "category_id", categoryId.ToString() } };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                await httpClient.PostAsync(apiPrefix, content);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClearCacheCategory - " + ex.Message.ToString() + " Token:" + token);
            }
        }

        public IActionResult ProductCategory(bool isFromCampaign = false)
        {
            ViewBag.isFromCampaign = isFromCampaign;
            return View();
        }

        [HttpPost]
        [Obsolete]
        public IActionResult UploadExcel(IFormFile fileCrawl)
        {
            try
            {
                var listLink = new List<string>();
                if (fileCrawl == null)
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = new List<String>(),
                        Message = "Vui lòng chọn file."
                    });
                }
                if (!fileCrawl.FileName.Contains("xlsx") && !fileCrawl.FileName.Contains("xsl"))
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = new List<String>(),
                        Message = "File không đúng định dạng. Vui lòng nhập định dạng là file excel."
                    });
                }
                if (fileCrawl.Length > 10000000)
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = new List<String>(),
                        Message = "File bạn tải lên quá 10MB. Vui lòng nhập file có kích thước nhỏ hơn 10MB."
                    });
                }
                Workbook workbook = new Workbook(fileCrawl.OpenReadStream());
                var worksheet = workbook.Worksheets[0];
                var listLinkWrong = new List<string>();//list link khong hop le
                if (worksheet.Cells.Count > 0)
                {
                    var listLable = _LabelRepository.GetListAll();
                    //truong hop link trong file khong dung dinh dang
                    var list = worksheet.Cells;
                    for (int i = 1; i < list.Count; i++)
                    {
                        if (list[i].Value == null || string.IsNullOrEmpty(list[i].Value.ToString()))
                        {
                            continue;
                        }
                        //kiem tra link co nam trong cac nhan hang US ho tro khong?
                        var checkLink = listLable.FirstOrDefault(n => list[i].Value.ToString().ToLower().Contains(n.Domain.ToLower()));
                        if (checkLink != null)
                        {
                            listLink.Add(list[i].Value.ToString());
                        }
                        else
                        {
                            listLinkWrong.Add(list[i].Value.ToString());
                        }
                    }
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = listLink,
                        DataLinkWrong = listLinkWrong,
                        Message = "Bạn chưa nhập link vào file excel"
                    });
                }
                return new JsonResult(new
                {
                    Code = 1,
                    Data = listLink,
                    DataLinkWrong = listLinkWrong,
                    Message = "Thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UploadExcelAsync: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    DataLinkWrong = new List<String>(),
                    Message = "Lỗi khi gửi file lên server."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProductClassificationJson(List<string> listLink, int groupId, string fromTime,
            string toTime, int campaignId, string utm_campain, string utm_source, string utm_medium)
        {
            try
            {
                string EncryptApi = ReadFile.LoadConfig().KEY_ENCODE_TOKEN_PUT_QUEUE;
                HttpClient httpClient = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL +
                    ReadFile.LoadConfig().API_ADD_PRODUCT_CLASSIFICATION;
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var listLable = _LabelRepository.GetListAll();
                var fromDate = DateUtil.StringToDate(fromTime);
                var toDate = DateUtil.StringToDate(toTime).Value;
                if (fromDate > toDate)
                {
                    return new JsonResult(new
                    {
                        Data = -2,
                        Message = "Ngày bắt đầu hiệu lực phải nhỏ hơn ngày kết thúc hiệu lực"
                    });
                }
                foreach (var item in listLink)
                {
                    var productClassificationInfo = _ProductClassificationRepository.GetByLink(item);
                    ProductClassification productClassification = new ProductClassification();
                    productClassification.FromDate = fromDate;
                    productClassification.ToDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 00);
                    Uri uri = new Uri(item);
                    var lableInfo = listLable.FirstOrDefault(n => n.Domain.ToLower().Contains(uri.Host.ToLower()));
                    var labelType = lableInfo != null ? lableInfo.Id : 0;
                    productClassification.Link = item;
                    productClassification.GroupIdChoice = groupId;
                    productClassification.Status = (int)Utilities.Contants.Status.HOAT_DONG;
                    productClassification.CreateTime = DateTime.Now;
                    productClassification.CapaignId = campaignId;
                    productClassification.UtmPath = getUtmPath(utm_campain, utm_source, utm_medium);
                    productClassification.UserId = !string.IsNullOrEmpty(userId) ? int.Parse(userId) : 0;
                    productClassification.LabelId = lableInfo != null ? lableInfo.Id : 0;
                    productClassification.Id = productClassificationInfo != null && productClassificationInfo.Result != null
                        ? productClassificationInfo.Result.Id : 0;
                    if (productClassificationInfo.Result == null)
                    {
                        await _ProductClassificationRepository.CreateItem(productClassification);
                    }
                    else
                    {
                        productClassification.UpdateTime = DateTime.Now;
                        await _ProductClassificationRepository.UpdateItem(productClassification);
                    }
                    var j_param = new Dictionary<string, string> {
                        { "product_id", "-1" },
                        { "label_type",labelType.ToString()},
                        { "link_detail_product", item } };
                    string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);

                    var content = new FormUrlEncodedContent(new[]
                     {
                         new KeyValuePair<string, string>("token", token),
                     });
                    var result = await httpClient.PostAsync(apiPrefix, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    if (resultContent.status == "fail")
                    {
                        LogHelper.InsertLogTelegram("Loi put queue. Result: " + resultContent.status + ". Message: " + resultContent.msg);
                    }

                }
                int db_index = Convert.ToInt32(_configuration["Redis:Database:db_folder"]);
                string key = "GROUP_PRODUCT_" + groupId;
                _redisService.clear(key, db_index);
                return new JsonResult(new
                {
                    Data = 1,
                    Message = "Gửi yêu cầu thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddProductClassificationJson: " + ex);
                return new JsonResult(new
                {
                    Data = -1,
                    Message = "Lỗi khi gửi yêu cầu. Vui lòng liên hệ quản trị viên."
                }); ;
            }
        }

        public string getUtmPath(string utm_campain, string utm_source, string utm_medium)
        {
            string utmPath = "";
            //utm source
            if (!string.IsNullOrEmpty(utm_source) && string.IsNullOrEmpty(utm_medium)
                && string.IsNullOrEmpty(utm_campain))
            {
                utmPath = "?utm_source=" + utm_source;
            }
            if (!string.IsNullOrEmpty(utm_source) && !string.IsNullOrEmpty(utm_medium)
               && string.IsNullOrEmpty(utm_campain))
            {
                utmPath = "?utm_source=" + utm_source + "&utm_campain=";
            }
            if (!string.IsNullOrEmpty(utm_source) && !string.IsNullOrEmpty(utm_medium)
               && !string.IsNullOrEmpty(utm_campain))
            {
                utmPath = "?utm_source=" + utm_source + "&utm_campain=" + utm_campain +
                  "&utm_medium=" + utm_medium;
            }
            //utm utm_campain
            if (!string.IsNullOrEmpty(utm_campain) && string.IsNullOrEmpty(utm_medium)
               && string.IsNullOrEmpty(utm_source))
            {
                utmPath = "?utm_campain=" + utm_campain;
            }
            if (!string.IsNullOrEmpty(utm_campain) && !string.IsNullOrEmpty(utm_source)
               && string.IsNullOrEmpty(utm_medium))
            {
                utmPath = "?utm_source=" + utm_source + "&utm_campain=" + utm_campain;
            }
            if (!string.IsNullOrEmpty(utm_campain) && !string.IsNullOrEmpty(utm_source)
              && !string.IsNullOrEmpty(utm_medium))
            {
                utmPath = "?utm_source=" + utm_source + "&utm_campain=" + utm_campain +
                    "&utm_medium=" + utm_medium;
            }
            //utm medium
            if (!string.IsNullOrEmpty(utm_medium) && string.IsNullOrEmpty(utm_campain)
                && string.IsNullOrEmpty(utm_source))
            {
                utmPath = "?utm_medium=" + utm_medium;
            }
            if (!string.IsNullOrEmpty(utm_medium) && string.IsNullOrEmpty(utm_campain)
               && string.IsNullOrEmpty(utm_source))
            {
                utmPath = "?utm_campain=" + utm_campain + "&utm_medium=" + utm_medium;
            }
            if (!string.IsNullOrEmpty(utm_medium) && string.IsNullOrEmpty(utm_campain)
              && string.IsNullOrEmpty(utm_source))
            {
                utmPath = "?utm_source=" + utm_source + "&utm_campain=" + utm_campain +
                    "&utm_medium=" + utm_medium;
            }
            return utmPath;
        }

        public IActionResult AddCampaign()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCampaignJson(CampaignAds campaignAds, string fromTime, string toTime)
        {
            try
            {
                var fromDate = DateUtil.StringToDate(fromTime);
                var toDate = DateUtil.StringToDate(toTime).Value;
                campaignAds.StartDate = fromDate;
                campaignAds.Type = (int)Constants.CampaignType.Campaign_Product;
                campaignAds.EndDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 00);
                var rs = _CampaignAdsRepository.Create(campaignAds);
                if (rs.Result > -1)
                {
                    return new JsonResult(new
                    {
                        Code = 1,
                        Message = "Khởi tạo chiến dịch thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = 1,
                        Message = "Khởi tạo chiến dịch thất bại."
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddCamgpain - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    Message = "Lỗi khi gửi file lên server."
                });
            }
        }

        public IActionResult EditCampaign(int id)
        {
            var model = _CampaignAdsRepository.GetById(id);
            return View(model != null ? model.Result : null);
        }

        [HttpPost]
        public IActionResult EditCampaignJson(CampaignAds campaignAds, string fromTime, string toTime)
        {
            try
            {
                var model = _CampaignAdsRepository.GetById(campaignAds.Id).Result;
                var fromDate = DateUtil.StringToDate(fromTime);
                var toDate = DateUtil.StringToDate(toTime).Value;
                model.CampaignName = campaignAds.CampaignName;
                model.StartDate = fromDate;
                model.EndDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 00);
                var rs = _CampaignAdsRepository.Update(campaignAds);
                if (rs.Result > -1)
                {
                    return new JsonResult(new
                    {
                        Code = 1,
                        Message = "Cập nhật chiến dịch thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = -1,
                        Message = "Cập nhật chiến dịch thất bại."
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddCamgpain - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Message = "Lỗi khi gửi file lên server."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAllGroup()
        {
            try
            {
                var listGroup = await _GroupProductRepository.GetAll();
                var listLabel = _LabelRepository.GetListAll();
                return new JsonResult(new
                {
                    Data = listGroup,
                    DataLabel = listLabel
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllGroup: " + ex);
                return new JsonResult(new
                {
                    Data = new List<GroupProduct>()
                }); ;
            }
        }
        [HttpPost]
        public IActionResult GetAllCampaign()
        {
            try
            {
                var listCamgpain = _CampaignAdsRepository.GetListAll();
                return new JsonResult(new
                {
                    Data = listCamgpain
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllCampaign - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Data = new List<GroupProduct>()
                }); ;
            }
        }

        [HttpPost]
        public IActionResult GetAllPosition()
        {
            try
            {
                var listPosition = _PositionRepository.GetAll();
                return new JsonResult(new
                {
                    Code = 1,
                    Data = listPosition,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllPosition - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<Position>(),
                });
            }
        }

        public IActionResult AddOrUpdatePositionAsync(int id)
        {
            ViewBag.listPosition = _PositionRepository.GetListAll();
            return View();
        }

        [HttpPost]
        public IActionResult AddPositionJson(Position position)
        {
            try
            {
                var postionExists = _PositionRepository.GetByPositionName(position.PositionName);
                if (postionExists != null && postionExists.Result != null)
                {
                    return new JsonResult(new
                    {
                        Code = -2,
                        Message = "Tên kích thước đã tồn tại. Vui lòng nhập tên khác."
                    });
                }
                var rs = _PositionRepository.Create(position);
                if (rs.Result > -1)
                {
                    return new JsonResult(new
                    {
                        Code = 1,
                        Message = "Thêm mới kích thước thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = -1,
                        Message = "Thêm mới kích thước thất bại"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddPositionJson - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    Message = "Lỗi thêm mới kích thước."
                });
            }
        }

        public IActionResult UpdatePosition(int id)
        {
            try
            {
                var model = _PositionRepository.GetById(id);
                return View(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePosition - GrouProduct: " + ex);
                return View();
            }
        }

        [HttpPost]
        public IActionResult UpdatePositionJson(Position model)
        {
            try
            {
                var postionExists = _PositionRepository.GetByPositionName(model.PositionName);
                if (postionExists != null && postionExists.Result != null && postionExists.Result.Id != model.Id)
                {
                    return new JsonResult(new
                    {
                        Code = -2,
                        Message = "Tên kích thước đã tồn tại. Vui lòng nhập tên khác."
                    });
                }
                var orgInfo = _PositionRepository.GetById(model.Id).Result;
                orgInfo.Height = model.Height;
                orgInfo.Width = model.Width;
                orgInfo.PositionName = model.PositionName;
                var rs = _PositionRepository.Update(orgInfo);
                if (rs.Result > -1)
                {
                    return new JsonResult(new
                    {
                        Code = 1,
                        Message = "Cập nhật kích thước thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = -1,
                        Message = "Cập nhật kích thước thất bại"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePositionJson - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    Message = "Lỗi cập nhật kích thước."
                });
            }
        }

        public IActionResult ToolManual()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductManual(string ASIN, string json_product)
        {
            try
            {
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                HttpClient httpClient = new HttpClient();
                var apiPrefix = "http://dropshipping.x-tech.vn/apiUsexpress/createNewProductManual";
                string sContentEncode = "{'asin': '" + ASIN + "','json_product_data': '" + json_product + "'}";
                var token = Utilities.CommonHelper.Encode(sContentEncode, EncryptApi);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token)
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                if (resultContent.status == "success")
                {
                    string msg = (string)resultContent.msg;
                    return new JsonResult(new
                    {
                        Code = 0,
                        Message = msg
                    });
                }
                else
                {
                    string msg = (string)resultContent.msg;
                    return new JsonResult(new
                    {
                        Code = 1,
                        Message = msg
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePositionJson - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 1,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDetail(string ASIN, int data_type)
        {
            try
            {
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                HttpClient httpClient = new HttpClient();
                var apiPrefix = "http://dropshipping.x-tech.vn/apiUsexpress/getDetailProductManual";
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("asin", ASIN),
                     new KeyValuePair<string, string>("data_type", data_type +"")//0 la du lieu mau, 1 la chi tiet
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (resultContent.status == "success")
                {
                    var j_product = (string)resultContent.j_product_detail;
                    j_product = !string.IsNullOrEmpty(j_product) ? j_product : j_product;
                    return new JsonResult(new
                    {
                        Code = 0,
                        Detail = j_product,
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = 1,
                        Detail = "",
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePositionJson - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 1,
                    Detail = "",
                });
            }
        }

        [HttpPost]
        public IActionResult GetAllUtmSource()
        {
            try
            {
                var listPosition = _AllCodeRepository.GetListByType("UTM_SOURCE");
                return new JsonResult(new
                {
                    Code = 1,
                    Data = listPosition,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllUtmSource - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<Position>(),
                });
            }
        }

        [HttpPost]
        public IActionResult GetAllUtmMedium()
        {
            try
            {
                var listPosition = _AllCodeRepository.GetListByType("UTM_MEDIUM");
                return new JsonResult(new
                {
                    Code = 1,
                    Data = listPosition,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllUtmMedium - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<Position>(),
                });
            }
        }

        [HttpPost]
        public IActionResult GetAllUtmCampaign()
        {
            try
            {
                var listPosition = _AllCodeRepository.GetListByType("UTM_CAMPAIGN");
                return new JsonResult(new
                {
                    Code = 1,
                    Data = listPosition,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllUtmCampaign - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<Position>(),
                });
            }
        }

        public IActionResult AddAllCode(bool utm_source = false, bool utm_medium = false, bool utm_campaign = false)
        {
            ViewBag.utm_source = utm_source;
            ViewBag.utm_medium = utm_medium;
            ViewBag.utm_campaign = utm_campaign;
            return View();
        }

        [HttpPost]
        public IActionResult AddAllCodeJson(AllCode allCode)
        {
            try
            {
                var rs = _AllCodeRepository.Create(allCode);
                return new JsonResult(new
                {
                    Code = rs.Result,
                    Message = "Thêm mới thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAllCodeJson - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    Message = "Lỗi thêm mới."
                });
            }
        }

        public IActionResult EditAllCode(int id, bool utm_source = false, bool utm_medium = false, bool utm_campaign = false)
        {
            ViewBag.utm_source = utm_source;
            ViewBag.utm_medium = utm_medium;
            ViewBag.utm_campaign = utm_campaign;
            var allCode = _AllCodeRepository.GetById(id);
            return View(allCode.Result);
        }

        [HttpPost]
        public IActionResult EditAllCodeJson(AllCode allCode)
        {
            try
            {
                var rs = _AllCodeRepository.Update(allCode);
                return new JsonResult(new
                {
                    Code = rs.Result,
                    Message = "Cập nhật thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("EditAllCodeJson - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    Message = "Lỗi cập nhật."
                });
            }
        }

        [HttpPost]
        public async Task PushCrawlDataToQueue(List<string> listLink, int groupId, int campaignId)
        {
            try
            {
                string EncryptApi = ReadFile.LoadConfig().KEY_ENCODE_TOKEN_PUT_QUEUE;
                HttpClient httpClient = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                var listLable = _LabelRepository.GetListAll();
                foreach (var item in listLink)
                {
                    Uri uri = new Uri(item);
                    var lableInfo = listLable.FirstOrDefault(n => n.Domain.ToLower().Contains(uri.Host.ToLower()));
                    var labelType = lableInfo != null ? lableInfo.Id : 0;
                    var item_detail = new SLQueueItem()
                    {
                        groupProductid = groupId,
                        labelid = labelType,
                        linkdetail = item
                    };
                    var j_param = new
                    {
                        type = TaskQueueName.group_product_mapping,
                        data_push = JsonConvert.SerializeObject(item_detail)
                    };
                    string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), ReadFile.LoadConfig().EncryptApi);
                    var content = new FormUrlEncodedContent(new[]
                    {
                         new KeyValuePair<string, string>("token", token),
                    });
                    var result = await httpClient.PostAsync(apiPrefix, content);
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    if (resultContent.status != ResponseType.SUCCESS.ToString())
                    {
                        LogHelper.InsertLogTelegram("Loi put queue. Result: " + resultContent.status + ". Message: " + resultContent.msg);
                    }
                }
                await ClearCampaignCache(campaignId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS - GroupProduct - PushCrawlDataToQueue: " + ex);
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
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClearCampaignCache - " + ex.Message.ToString() + " Token:" + token);
                throw ex;
            }
        }

        public async Task ClearCacheByKey(string cacheKey)
        {
            string token = string.Empty;
            try
            {
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_CLEAR_CACHE_BY_KEY;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API;
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string> {
                    { "cache_key", cacheKey }
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClearCacheByKey - " + ex.Message.ToString() + " Token:" + token);
                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoCrawl(SLQueueItem item)
        {
            try
            {
                string product_code;

                switch (item.labelid)
                {
                    case 1:
                        {
                            CommonHelper.CheckAsinByLink(item.linkdetail, out product_code);
                            if (product_code == null)
                            {
                                return new JsonResult(new
                                {
                                    Code = 0,
                                    Message = "URL gửi lên không chính xác, vui lòng kiểm tra URL và thử lại."
                                });
                            }
                        }
                        break;
                    default:
                        {
                            return new JsonResult(new
                            {
                                Code = 0,
                                Message = "Nhãn hàng đang được hoàn thiện, vui lòng sử dụng nhãn hàng khác."
                            });
                        }

                }
                HttpClient httpClient = new HttpClient();
                var apiQueueService = ReadFile.LoadConfig().API_CMS_URL
                   + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;

                var j_param = new Dictionary<string, string>()
                    {
                        {"data_push", JsonConvert.SerializeObject(item) },
                        { "type",TaskQueueName.group_product_mapping}
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
                    LogHelper.InsertLogTelegram("ForceCrawlMapping - GroupProductController" +
                        "Gửi yêu cầu Crawl thất bại. Chi tiết: " + msg);
                    return new JsonResult(new
                    {
                        Code = 0,
                        Message = "Gửi yêu cầu Crawl thất bại " + msg
                    });
                }
                var db_index = Convert.ToInt32(_configuration["Redis:Database:db_folder"]);
                _redisService.clear("GROUP_PRODUCT_" + item.groupProductid, db_index);
                return new JsonResult(new
                {
                    Code = 1,
                    Message = "Gửi yêu cầu Crawl thành công."

                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DoCrawl - GroupProductController error:" + ex.ToString());
                return new JsonResult(new
                {
                    Code = 0,
                    Message = "Lỗi trong quá trình xử lý"
                });
            }

        }
    }
}