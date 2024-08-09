using App_Crawl_SearchList_Receiver.Models;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Common;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.Models.Product;
using LogHelper = Utilities.LogHelper;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ProductController : Controller
    {
        private const int USEXPRESS_CATEGORY_ID = 90;
        private readonly ILabelRepository _LabelRepository;
        private readonly ICommonRepository _CommonRepository;
        private readonly IProductRepository _ProductRepository;
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly RedisConn _RedisService;
        private readonly IConfiguration _Configuration;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly ILocationProductRepository _locationProductRepository;
        private const int US_CATEGORY_ID = 90;
        private readonly string _UrlStaticImage;
        private readonly ProductMongoAccess _productMongoAccess;
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;


        public ProductController(IConfiguration configuration, IAllCodeRepository allCodeRepository,
            ILabelRepository labelRepository, ICommonRepository commonRepository,
            IGroupProductRepository groupProductRepository, IProductRepository productRepository, ILocationProductRepository locationProductRepository, RedisConn redisService,
            IOptions<DomainConfig> domainConfig)
        {
            _allCodeRepository = allCodeRepository;
            _LabelRepository = labelRepository;
            _CommonRepository = commonRepository;
            _ProductRepository = productRepository;
            _GroupProductRepository = groupProductRepository;
            _RedisService = redisService;
            //_RedisService = new RedisConn(configuration);
            _RedisService.Connect();
            _Configuration = configuration;
            _locationProductRepository = locationProductRepository;
            _UrlStaticImage = configuration["DomainConfig:ImageStatic"];
            _productMongoAccess = new ProductMongoAccess(configuration);
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.LabelList = _LabelRepository.GetListAll();
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(USEXPRESS_CATEGORY_ID, -1);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductGrid(ProductFilterModel model)
        {
            var data = new GenericViewModel<ProductViewModel>();
            try
            {
                data = await _ProductRepository.GetProductPagingList(model);
            }
            catch
            {

            }

            ViewBag.SortField = model.SortField;
            ViewBag.SortType = model.SortType;
            return PartialView(data);
        }

        public string GetProductBoughtQuantity(string arrProductCode)
        {
            var data = _ProductRepository.GetListBoughtProductQuantity(arrProductCode);
            return JsonConvert.SerializeObject(data);
        }


        [HttpPost]
        public async Task<IActionResult> GetProductManualByASIN(string ASIN, int label_id, int redirect_from_cms = 0)
        {
            ProductViewModel model = null;
            int status = (int)ResponseType.FAILED;
            string msg = null;
            try
            {
                if (ASIN == null || ASIN == "")
                {
                    status = (int)ResponseType.FAILED;
                    msg = "ASIN không được để trống.";
                }
                else
                {
                    bool exists = false;
                    status = (int)ResponseType.FAILED;
                    msg = "Không tìm thấy thông tin của ProductID: " + ASIN;
                    //-- Get From MongoDB:
                    var model_mongo = await _productDetailMongoAccess.FindDetailByProductCode(ASIN);
                    if (model_mongo != null && model_mongo.product_detail.product_code != null && model_mongo.product_detail.product_code.Trim() != "")
                    {
                        model = model_mongo.product_detail;
                        status = (int)ResponseType.SUCCESS;
                        msg = "Lấy dữ liệu thành công";
                        exists = true;
                    }
                    //-- Get From ES:
                    if (exists == false)
                    {

                        IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                        model = _ESRepository.getProductDetailByCode("product", ASIN, label_id);
                        if (model != null && model.product_code != null && model.product_code.Trim() != "")
                        {
                            try
                            {
                                await SyncProductSpecification(model.product_specification);
                                status = (int)ResponseType.SUCCESS;
                                msg = "Lấy dữ liệu thành công";
                                exists = true;
                            }
                            catch (Exception)
                            {
                                status = (int)ResponseType.FAILED;
                                msg = "Dữ liệu đang được lưu không đúng dạng form.";
                            }
                        }
                    }
                    //-- Get From redis:
                    if (exists == false)
                    {
                        int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                        switch (label_id)
                        {
                            case (int)LabelType.amazon:
                                {
                                    db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                                }
                                break;
                            case (int)LabelType.jomashop:
                                {
                                    db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_jomashop"]);
                                }
                                break;
                            case (int)LabelType.costco:
                                {
                                    db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_costco"]);
                                }
                                break;
                        }
                        string cache_name = CacheHelper.cacheKeyProductDetail(ASIN, label_id);
                        var data_redis = await _RedisService.GetAsync(cache_name, db_index);
                        if (data_redis != null && data_redis.Trim() != "")
                        {
                            model = JsonConvert.DeserializeObject<ProductViewModel>(data_redis);
                            await SyncProductSpecification(model.product_specification);
                            status = (int)ResponseType.SUCCESS;
                            msg = "Lấy dữ liệu thành công";
                            exists = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetProductManualByASIN - ProductController:  " + ex + ". \nInput: " + ASIN + ", " + label_id + ", " + redirect_from_cms);
                status = (int)ResponseType.ERROR;
                msg = "Lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận kỹ thuật";

            }
            return Ok(new
            {
                status = status,
                msg = msg,
                model = model
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetGroupProductNameByID(int group_id)
        {
            string group_name = "";
            int status = (int)ResponseType.FAILED;
            try
            {
                group_name = await _GroupProductRepository.GetGroupProductNameAsync(group_id);
                status = (int)ResponseType.SUCCESS;
            }
            catch (Exception ex)
            {
                status = (int)ResponseType.FAILED;
                LogHelper.InsertLogTelegram("GetGroupProductNameByID - ProductController" + ex);
            }
            return Ok(new
            {
                status = status,
                data = group_name
            });

        }
        [HttpPost]
        public async Task<IActionResult> CreateProductManual(string json,string product_location)
        {
            int status = 0;
            string msg = "Not Implemented";
            string data = null;
            ProductViewModel model = null;
            List<LocationProduct> location_list = null;
            try
            {
                try
                {
                    model = JsonConvert.DeserializeObject<ProductViewModel>(json);
                    location_list = JsonConvert.DeserializeObject<List<LocationProduct>>(product_location);

                }
                catch (Exception ex)
                {

                }
                var regexItem = new Regex("^[a-zA-Z0-9 -_]*$");
                if (model == null || model.product_code == null || model.product_code.Trim() == "" || model.product_name == null|| model.product_name.Trim() == "" || model.price <=0
                    || model.item_weight == null || model.item_weight.Trim() == "" || model.item_weight.Contains("undefined") || model.label_id < 0 || model.image_product == null || model.image_product.Count < 1
                  /*  || location_list==null || location_list.Count<1*/)
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                    if (model == null || model.product_code == null) msg += "\nCấu trúc gửi lên không chính xác, vui lòng liên hệ IT.";
                    else
                    {
                        msg += "\n";
                        if (model.product_name == null || model.product_name.Trim() == "") msg += "Tên sản phẩm bị thiếu. ";
                        if (model.price <= 0) msg += "Giá sản phẩm phải lớn hơn 0. "; 
                        if (model.item_weight == null || model.item_weight.Trim() == "" || model.item_weight.Contains("undefined")) msg += "Cân nặng không chính xác. ";
                        if (model.label_id < 0) msg += "Sản phẩm không thuộc nhãn nào. ";
                        if (model.image_product == null || model.image_product.Count < 1) msg += "Ảnh sản phẩm bị thiếu. ";
                    }

                }
                else if (!regexItem.IsMatch(model.product_code))
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Mã sản phẩm không chính xác, vui lòng thử lại";

                }
                else
                {
                    //-- Thông tin chung:
                    int unit_weight_id = 1;
                    switch (model.item_weight.Split(" ")[1].Trim())
                    {
                        case "ounces":
                            unit_weight_id = 0;
                            break;
                        case "pounds":
                            unit_weight_id = 1;

                            break;
                        case "grams":
                            unit_weight_id = 2;

                            break;
                        case "kilograms":
                            unit_weight_id = 3;

                            break;
                        case "tonne":
                            unit_weight_id = 4;

                            break;
                        case "kiloton":
                            unit_weight_id = 5;
                            break;
                    }
                    model.update_last = DateTime.Now.AddDays(1);
                    if (model.create_date == null || model.create_date <= new DateTime(DateTime.MinValue.Ticks))
                    {
                        model.create_date = DateTime.Now;
                    }
                    model.is_crawl_weight = true;
                    model.regex_step = 2;
                    model.is_has_seller = false;
                    model.is_redirect_extension = false;
                    if (model.variation_name == null)
                    {
                        model.variation_name = new List<string>();
                        model.variation_name.Add("Default");
                    }
                    model.image_size_product = new List<ImageSizeViewModel>();
                    foreach (var image in model.image_product)
                    {
                        model.image_size_product.Add(new ImageSizeViewModel()
                        {
                            Larger = image,
                            Thumb = image
                        });
                    }
                    if (model.image_thumb == null || model.image_thumb.Trim() == "")
                    {
                        model.image_thumb = model.image_product[0];
                    }
                    if (model.product_infomation_HTML != null && model.product_infomation_HTML.Trim() != "")
                    {
                        var info_html = await StringHelpers.ReplaceEditorImage(model.product_infomation_HTML.Replace("<img src", "<img class=\" lazydetail\" src"), _UrlStaticImage);
                        model.product_infomation_HTML = info_html;
                    }
                    else
                    {
                        string product_infomation_HTML = "";
                        string template = "<img alt=\"" + model.product_name + "\" data-original=\"{img_link}\" class=\"lazydetail\" src=\"{img_link}\" style =\"display: inline;\">";
                        try
                        {
                            if (model.image_product != null && model.image_product.Count > 0)
                            {
                                foreach (var img in model.image_product)
                                {
                                    product_infomation_HTML += template.Replace("{img_link}", img);
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                        model.product_infomation_HTML = product_infomation_HTML;
                    }
                    model.seller_name= CommonHelper.RemoveSpecialCharacters(model.seller_name.Replace("\"", "").Replace("'", "").Replace("Author", " "));
                    //-- Chia label để fill model:
                    switch (model.label_id)
                    {
                        case (int)LabelType.amazon:
                            {
                                model.link_product = CommonHelper.genLinkDetailProduct(LabelNameType.amazon, model.product_code, CommonHelper.RemoveUnicode(CommonHelper.RemoveSpecialCharacters(model.product_name)).Trim());
                                model.label_name = "amazon";
                                model.keywork_search = "https://www.amazon.com/dp/" + model.product_code;
                                model.amount = model.price + model.shiping_fee;
                            }
                            break;
                        case (int)LabelType.costco:
                            {
                                model.link_product = CommonHelper.genLinkDetailProduct(LabelNameType.costco, model.product_code, CommonHelper.RemoveUnicode(CommonHelper.RemoveSpecialCharacters(model.product_name)).Trim());
                                model.label_name = "costco";
                                model.keywork_search = "";
                                model.image_thumb = model.image_product[0];

                                break;
                            }
                        default:
                            {

                            }
                            break;
                    }
                    switch (model.product_type)
                    {
                        case (int)ProductType.FIXED_AMOUNT_VND:
                            {
                                model.rate = 20000;
                                var fee_model = new ProductBuyerViewModel()
                                {
                                    IndustrySpecialType = 0,
                                    LabelId = 2,
                                    Pound = 1,
                                    Price = 0,
                                    RateCurrent = 20000,
                                    Unit = 1
                                };
                                var rs = await GetFeeFixed(fee_model, model.amount_vnd) as OkObjectResult;
                                var rs1 = rs.Value as dynamic;
                                var product_fee_list = JsonConvert.DeserializeObject<Dictionary<string, string>>(rs1.data);
                                if (model.list_product_fee == null) model.list_product_fee = new ProductFeeViewModel();
                                model.list_product_fee.shiping_fee = model.shiping_fee;
                                model.list_product_fee.list_product_fee = new Dictionary<string, double>();
                                model.list_product_fee.list_product_fee.Add("ITEM_WEIGHT", Convert.ToDouble(product_fee_list["ITEM_WEIGHT"]));
                                model.list_product_fee.list_product_fee.Add("FIRST_POUND_FEE", Convert.ToDouble(product_fee_list["FIRST_POUND_FEE"]));
                                model.list_product_fee.list_product_fee.Add("NEXT_POUND_FEE", Convert.ToDouble(product_fee_list["NEXT_POUND_FEE"]));
                                model.list_product_fee.list_product_fee.Add("LUXURY_FEE", Convert.ToDouble(product_fee_list["LUXURY_FEE"]));
                                model.list_product_fee.list_product_fee.Add("DISCOUNT_FIRST_FEE", Convert.ToDouble(product_fee_list["DISCOUNT_FIRST_FEE"]));
                                model.list_product_fee.list_product_fee.Add("TOTAL_SHIPPING_FEE", Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2));
                                model.list_product_fee.list_product_fee.Add("PRICE_LAST", Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]), 2));
                                model.list_product_fee.label_name = LabelNameType.GetLabelName(model.label_id);
                                model.list_product_fee.total_fee = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2);
                                var price = Math.Round((model.amount_vnd / model.rate) - Convert.ToDouble(product_fee_list["FIRST_POUND_FEE"]) - Convert.ToDouble(product_fee_list["NEXT_POUND_FEE"]), 2);
                                model.list_product_fee.price = price;
                                model.price = price;
                                model.list_product_fee.amount_vnd = model.amount_vnd;
                                model.rate = Math.Round((model.amount_vnd / Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]), 2)), 4);
                                model.item_weight = "1 pounds";
                                data = JsonConvert.SerializeObject(model.list_product_fee.list_product_fee);
                                if (model.product_save_price > 0)
                                {
                                    model.discount = Math.Round((model.product_save_price / (model.product_save_price + model.price)) * 100, 0);
                                }
                                else
                                {
                                    model.discount = 0;
                                }
                                //-- Push MongoDb
                                var exists_id = await _productDetailMongoAccess.FindIDByProductCode(model.product_code);
                                ProductMongoViewModel model_2 = new ProductMongoViewModel()
                                {
                                    product_detail = model
                                };
                                if (exists_id != null && exists_id.Trim() != "")
                                {
                                    model_2._id = exists_id;
                                    await _productDetailMongoAccess.UpdateAsync(model_2);
                                }
                                else
                                {
                                    model_2.GenID();
                                    await _productDetailMongoAccess.AddNewAsync(model_2);
                                }
                                //-- Product Location: 
                                var id = model_2._id;
                                if (location_list != null && location_list.Count > 0)
                                {
                                    //Delete Non Select Location:
                                    var exists_list = await _locationProductRepository.GetByProductCode(location_list[0].ProductCode);
                                    bool existence = false;
                                    foreach (var exists in exists_list)
                                    {
                                        existence = false;
                                        foreach (var location in location_list)
                                        {
                                            if (location.ProductCode == exists.ProductCode && location.GroupProductId == exists.GroupProductId)
                                            {
                                                existence = true;
                                                break;
                                            }
                                        }
                                        if (existence == false) await _locationProductRepository.DeleteAsync(exists);
                                    }
                                    string group_id = "";
                                    var is_first = true;
                                    //Add or Update
                                    foreach (var location in location_list)
                                    {
                                        //--Update Product Location:
                                        existence = false;
                                        foreach (var exists in exists_list)
                                        {
                                            if (location.ProductCode == exists.ProductCode && location.GroupProductId == exists.GroupProductId)
                                            {
                                                exists.UpdateLast = DateTime.Now;
                                                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                                                {
                                                    exists.UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                                                }
                                                exists.OrderNo = location.OrderNo;
                                                await _locationProductRepository.Update(exists);
                                                existence = true;
                                                break;
                                            }
                                        }
                                        if (existence == false)
                                        {
                                            if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                                            {
                                                location.UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                                            }
                                            location.CreateOn = DateTime.Now;
                                            location.UpdateLast = DateTime.Now;
                                            var location_id = await _locationProductRepository.Addnew(location);
                                        }
                                        //--Add To Queue field:
                                        if (is_first)
                                        {
                                            is_first = false;
                                        }
                                        else
                                        {
                                            group_id += ",";
                                        }
                                        group_id += location.GroupProductId;
                                        // _RedisService.clear("GROUP_PRODUCT_"+location.GroupProductId+"_"+ label_id, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                                        // _RedisService.clear("GROUP_PRODUCT_MANUAL_" + location.GroupProductId, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                                    }
                                    //-- Push Queue Offline:
                                    HttpClient httpClient_2 = new HttpClient();
                                    var apiQueueService = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                                    string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                                    var item = new
                                    {
                                        product_manual_key_id = id,
                                        group_id = group_id
                                    };
                                    var j_param = new Dictionary<string, string>()
                                    {
                                        {"data_push", JsonConvert.SerializeObject(item) },
                                        { "type",TaskQueueName.product_detail_manual_queue}
                                    };
                                    var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                                    var content = new FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("token", token),
                                    });
                                    var result_2 = await httpClient_2.PostAsync(apiQueueService, content);
                                    dynamic resultContent_2 = Newtonsoft.Json.Linq.JObject.Parse(result_2.Content.ReadAsStringAsync().Result);
                                    // msg_2 = (string)resultContent_2.msg;

                                }
                            } break;
                        default:
                            {
                                //Check rate và khởi tạo:
                                HttpClient httpClient = new HttpClient();
                                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_RATE_CURRENT;
                                var result = await httpClient.GetAsync(apiPrefix);
                                dynamic resultContent = result.Content.ReadAsStringAsync().Result;
                                if (double.TryParse(resultContent, out double rateValue)) model.rate = rateValue;
                                var product_fee_list = JsonConvert.DeserializeObject<Dictionary<string, string>>(await GetDetailProductCost(new ProductBuyerViewModel()
                                {
                                    IndustrySpecialType = model.industry_special_type,
                                    LabelId = model.label_id,
                                    Pound = Convert.ToDouble(model.item_weight.Split(" ")[0]),
                                    Price = model.amount <=0 ? model.price: model.amount,
                                    RateCurrent = model.rate,
                                    Unit = unit_weight_id
                                }));
                                if (model.list_product_fee == null) model.list_product_fee = new ProductFeeViewModel();
                                model.list_product_fee.shiping_fee = model.shiping_fee;
                                model.list_product_fee.list_product_fee = new Dictionary<string, double>();
                                model.list_product_fee.list_product_fee.Add("ITEM_WEIGHT", Convert.ToDouble(product_fee_list["ITEM_WEIGHT"]));
                                model.list_product_fee.list_product_fee.Add("FIRST_POUND_FEE", Convert.ToDouble(product_fee_list["FIRST_POUND_FEE"]));
                                model.list_product_fee.list_product_fee.Add("NEXT_POUND_FEE", Convert.ToDouble(product_fee_list["NEXT_POUND_FEE"]));
                                model.list_product_fee.list_product_fee.Add("LUXURY_FEE", Convert.ToDouble(product_fee_list["LUXURY_FEE"]));
                                model.list_product_fee.list_product_fee.Add("DISCOUNT_FIRST_FEE", Convert.ToDouble(product_fee_list["DISCOUNT_FIRST_FEE"]));
                                model.list_product_fee.list_product_fee.Add("TOTAL_SHIPPING_FEE", Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2));
                                model.list_product_fee.list_product_fee.Add("PRICE_LAST", Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]), 2));
                                model.list_product_fee.label_name = LabelNameType.GetLabelName(model.label_id);
                                model.list_product_fee.price = model.amount;
                                model.list_product_fee.total_fee = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2);
                                model.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_FEE"]), 0);
                                model.list_product_fee.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_FEE"]), 0);
                                data = JsonConvert.SerializeObject(model.list_product_fee.list_product_fee);
                                if (model.product_save_price > 0)
                                {
                                    model.discount = Math.Round((model.product_save_price / (model.product_save_price + model.price)) * 100, 0);
                                }
                                else
                                {
                                    model.discount = 0;
                                }
                                var json_obj_edit = JsonConvert.SerializeObject(model);
                                //-- Push ES
                                IESRepository<ProductViewModel> _ESRepository = new ESRepository<ProductViewModel>(_Configuration["DataBaseConfig:Elastic:Host"]);
                                var productObj = JsonConvert.DeserializeObject<ProductViewModel>(json_obj_edit);
                                var product_response = _ESRepository.DeleteProductByCode("product", productObj.product_code);
                                if (product_response)
                                {
                                    productObj.id = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                                    var id=  _ESRepository.UpSert(productObj, "product");

                                }

                                // -- Push Redis:
                                string cache_name = CacheHelper.cacheKeyProductDetail(model.product_code, model.label_id);
                                int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                                switch (model.label_id)
                                {
                                    case (int)LabelType.amazon:
                                        {
                                            db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                                        }
                                        break;
                                    case (int)LabelType.jomashop:
                                        {
                                            db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_jomashop"]);
                                        }
                                        break;
                                    case (int)LabelType.costco:
                                        {
                                            db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_costco"]);
                                        }
                                        break;
                                }
                                _RedisService.Set(cache_name, json_obj_edit, db_index);
                                _RedisService.Set(cache_name, json_obj_edit, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                            } break;
                    }
                    status = (int)ResponseType.SUCCESS;
                    msg = "Tạo mới / Cập nhật thông tin thành công";

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateProductManual - ProductController" + ex);

            }
            return Ok(new
            {
                status = status,
                msg = msg,
                data = data,
                model = model            
            });
        }

        private static string RemoveSpecialCharacters(string input)
        {
            Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return r.Replace(input, String.Empty);
        }

        [HttpPost]
        public IActionResult BlockProductManual(string product_code_list, int label_id, int target_status)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";

            bool current_status = false;
            try
            {
                if (product_code_list == null || product_code_list.Trim() == "" || label_id < 0 || target_status < 0 || target_status > 1)
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                }
                else
                {

                    IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                    string cache_name = "";
                    int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                    ProductViewModel model = null;
                    string str = "";
                    List<string> list = product_code_list.Trim().Replace(" ", "").Split(",").ToList();
                    if (list != null && list.Count > 0)
                    {
                        msg = "";
                        foreach (var product_code in list)
                        {
                            model = null;
                            cache_name = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                            switch (label_id)
                            {
                                case (int)LabelType.amazon:
                                    {
                                        db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                                    }
                                    break;
                                case (int)LabelType.jomashop:
                                    {
                                        db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_jomashop"]);
                                    }
                                    break;
                                case (int)LabelType.costco:
                                    {
                                        db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_costco"]);
                                    }
                                    break;
                            }
                            str = _RedisService.Get(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                            if (str == null || str.Trim() == "")
                            {
                                model = _ESRepository.getProductDetailByCode("product", product_code, label_id);
                            }
                            else
                            {
                                model = JsonConvert.DeserializeObject<ProductViewModel>(str);
                            }
                            if (model == null || model.product_code == null || model.product_code.Trim() == "")
                            {
                                status = (int)ResponseType.SUCCESS;
                                msg += product_code + "Not Exist. ";
                            }
                            else
                            {
                                if (target_status == 1)
                                {
                                    // model.page_not_found = true;
                                    model.product_status = (int)ProductStatus.NOT_FOUND;
                                    msg = "Blocked: ";
                                }
                                else
                                {
                                    // model.page_not_found = false;
                                    model.product_status = (int)ProductStatus.NORMAL_SELL;
                                    msg = "Unblocked: ";
                                }
                                var json_obj_edit = JsonConvert.SerializeObject(model);
                                _RedisService.Set(cache_name, json_obj_edit, db_index);
                                _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                                var productObj = JsonConvert.DeserializeObject<ProductViewModel>(json_obj_edit);
                                var product_response = _ESRepository.DeleteProductByCode("product", productObj.product_code);
                                if (product_response)
                                {
                                    productObj.id = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                                    _ESRepository.UpSert(productObj, "product");
                                }
                                status = (int)ResponseType.SUCCESS;
                                msg += model.product_code + " success. ";
                            }

                        }
                        if (msg.Contains("failed"))
                        {
                            status = (int)ResponseType.FAILED;
                        }
                        else
                        {
                            status = (int)ResponseType.SUCCESS;
                        }
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                    }


                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BlockProductManual - ProductController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Error on Excution";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
                current_status = current_status
            });
        }
        [HttpPost]
        public async Task<IActionResult> LockProductManual(string product_code_list, int label_id, int target_status)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";

            bool current_status = false;
            try
            {
                if (product_code_list == null || product_code_list.Trim() == "" || label_id < 0)
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                }
                else
                {
                    IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                    string cache_name = "";
                    int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                    ProductViewModel model = null;
                    string str = "";
                    List<string> list = product_code_list.Trim().Replace(" ", "").Split(",").ToList();
                    if (list != null && list.Count > 0)
                    {
                        msg = "";
                        foreach (var product_code in list)
                        {
                            model = null;
                            cache_name = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                            switch (label_id)
                            {
                                case (int)LabelType.amazon:
                                    {
                                        db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                                    }
                                    break;
                                case (int)LabelType.jomashop:
                                    {
                                        db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_jomashop"]);
                                    }
                                    break;
                                case (int)LabelType.costco:
                                    {
                                        db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_costco"]);
                                    }
                                    break;
                            }
                            str = _RedisService.Get(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                            if (str == null || str.Trim() == "")
                            {
                                model = _ESRepository.getProductDetailByCode("product", product_code, label_id);
                            }
                            else
                            {
                                model = JsonConvert.DeserializeObject<ProductViewModel>(str);
                            }
                            if (model == null || model.product_code == null || model.product_code.Trim() == "")
                            {
                                status = (int)ResponseType.SUCCESS;
                                msg += product_code + "Not Exist. ";
                            }
                            else
                            {

                                if (target_status == 1)
                                {
                                    model.product_status = (int)ProductStatus.NOT_AVAILABLE;
                                    msg = "Locked: ";
                                }
                                else
                                {
                                    model.product_status = (int)ProductStatus.NORMAL_SELL;
                                    msg = "Unlocked: ";
                                }
                                var json_obj_edit = JsonConvert.SerializeObject(model);
                                _RedisService.Set(cache_name, json_obj_edit, db_index);
                                _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                                var productObj = JsonConvert.DeserializeObject<ProductViewModel>(json_obj_edit);
                                var product_response = _ESRepository.DeleteProductByCode("product", productObj.product_code);
                                if (product_response)
                                {
                                    productObj.id = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                                    _ESRepository.UpSert(productObj, "product");
                                }
                                status = (int)ResponseType.SUCCESS;
                                msg += model.product_code + " success. ";
                            }
                        }
                        status = (int)ResponseType.SUCCESS;
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                    }


                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("LockProductManual - ProductController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Error on Excution";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
                current_status = current_status
            });
        }

        [HttpPost]
        public IActionResult DeleteProductOnES(string ASIN, int label_id)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
            try
            {
                if (ASIN == null || ASIN == "" || label_id < 1)
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                }
                else
                {
                    string cache_name = CacheHelper.cacheKeyProductDetail(ASIN, label_id);
                    //-- Delete in Redis:
                    _RedisService.clear(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                    IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                    // Build id identity:
                    // Delete in ES:
                    var product_response = _ESRepository.DeleteProductByCode("product", ASIN);
                    if (product_response)
                    {
                        status = (int)ResponseType.SUCCESS;
                        msg = "Xoá sản phẩm thành công";

                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        msg = "Xoá sản phẩm thất bại";
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteProductOnES - ProductController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Error on Excution";
            }
            return Ok(new
            {
                status = status,
                msg = msg,

            });
        }

        [HttpPost]
        public IActionResult DeleteProductOnRedis(string ASIN, int label_id)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
            try
            {
                if (ASIN == null || ASIN == "" || label_id < 1)
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                }
                else
                {
                    string cache_name = CacheHelper.cacheKeyProductDetail(ASIN, label_id);
                    //-- Delete in Redis:
                    _RedisService.clear(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                    status = (int)ResponseType.SUCCESS;
                    msg = "Xoá cache sản phẩm sản phẩm thành công";

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteProductOnRedis - ProductController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Error on Excution";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
            });
        }
        [HttpPost]
        public async Task<IActionResult> GetFeeFixed(ProductBuyerViewModel product, double amount_vnd = 0)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
            dynamic data = null;
            double rate = 20000;
            try
            {
                if (product.LabelId < 1)
                {

                }
                else
                {
                    switch (product.LabelId)
                    {
                        case 2:
                            {

                                if (amount_vnd > 0)
                                {
                                   
                                    var list_fee = new Dictionary<string, string>();
                                    /*
                                    product.Price = Math.Round(amount_vnd / rate, 2);
                                    product.Pound = 0.1;
                                    product.Unit = 1;
                                    list_fee = JsonConvert.DeserializeObject<Dictionary<string, string>>(await GetDetailProductCost(product));*/
                                    list_fee["ITEM_WEIGHT"] = "1";
                                    list_fee["FIRST_POUND_FEE"] = "0";
                                    list_fee["NEXT_POUND_FEE"] = "0"; 
                                    list_fee["LUXURY_FEE"] = "0"; 
                                    list_fee["DISCOUNT_FIRST_FEE"] = "0"; 
                                    list_fee["TOTAL_SHIPPING_FEE"] = "0";
                                    list_fee["RATE_CURRENT"] = "20000";
                                    list_fee["ORIGINAL_WEIGHT"] = "1 pounds";
                                    
                                    //-- Additional
                                    list_fee["TOTAL_FEE"] = amount_vnd.ToString();
                                    list_fee["PRICE_LAST"] = Math.Round(amount_vnd / rate, 2).ToString();
                                    list_fee["PRODUCT_PRICE"] = (Convert.ToDouble(list_fee["PRICE_LAST"]) - Convert.ToDouble(list_fee["FIRST_POUND_FEE"])).ToString();
                                    data = JsonConvert.SerializeObject(list_fee);
                                }
                                else
                                {
                                    data = await GetDetailProductCost(product);
                                }
                                break;
                            }
                        default:
                            {
                                data = await GetDetailProductCost(product);
                                break;
                            }
                    }
                    status = (int)ResponseType.SUCCESS;
                    msg = "Success";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPriceFromFixedAmountVND - ProductController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Error on Excution";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
                data = data
            });
        }

        public async Task<IActionResult> ProductCost()
        {
            double rate_current = 0;
            var _UserId = "";

            try
            {
                string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                HttpClient httpClient = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_RATE_CURRENT;
                var result = await httpClient.GetAsync(apiPrefix);
                rate_current = Convert.ToDouble(result.Content.ReadAsStringAsync().Result);
                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                {
                    _UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                }
            }
            catch
            {

            }
            ViewBag.UserId = _UserId;
            ViewBag.LabelList = _LabelRepository.GetListAll();
            ViewBag.IndustrySpecialList = await _CommonRepository.GetAllCodeByType(AllCodeType.INDUSTRY_SPECIAL_TYPE);
            ViewBag.WeightUnitList = await _CommonRepository.GetAllCodeByType(AllCodeType.WEIGHT_UNIT);
            ViewBag.RateCurent = rate_current;
            return PartialView();
        }

        public async Task<string> GetDetailProductCost(ProductBuyerViewModel product)
        {
            try
            {
                //HttpClient httpClient = new HttpClient();
                //var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_RATE_CURRENT;
                //var result = await httpClient.GetAsync(apiPrefix);
                //dynamic resultContent = result.Content.ReadAsStringAsync().Result;
                //if (double.TryParse(resultContent, out double rateValue)) product.RateCurrent = rateValue;

                string Original_Weight = string.Empty;
                switch (product.Unit)
                {
                    case 0:
                        Original_Weight = product.Pound + " ounces";
                        product.Pound = CommonHelper.convertToPound(product.Pound, "ounces");
                        break;
                    case 1:
                        Original_Weight = product.Pound + " pounds";
                        product.Pound = CommonHelper.convertToPound(product.Pound, "pounds");
                        break;
                    case 2:
                        Original_Weight = product.Pound + " grams";
                        product.Pound = CommonHelper.convertToPound(product.Pound, "grams");
                        break;
                    case 3:
                        Original_Weight = product.Pound + " kilograms";
                        product.Pound = CommonHelper.convertToPound(product.Pound, "kilograms");
                        break;
                    case 4:
                        Original_Weight = product.Pound + " tonne";
                        product.Pound = CommonHelper.convertToPound(product.Pound, "tonne");
                        break;
                    case 5:
                        Original_Weight = product.Pound + " kiloton";
                        product.Pound = CommonHelper.convertToPound(product.Pound, "kiloton");
                        break;
                }

                var data = await _ProductRepository.getShippingFee(product.LabelId, product);

                double totalShippingFee = Convert.ToDouble(data["TOTAL_SHIPPING_FEE"]);
                double totalFee = (totalShippingFee + product.Price + product.ShippingUSFee) * product.RateCurrent;
                if (!data.ContainsKey("PRICE_LAST"))
                {
                    data.Add("PRICE_LAST", totalShippingFee + product.Price + product.ShippingUSFee);
                }
                data.Add("RATE_CURRENT", product.RateCurrent);
                data.Add("PRODUCT_PRICE", product.Price);
                data.Add("TOTAL_FEE", totalFee);

                if (product.LabelId == 1)
                    data.Add("SHIPPING_US_FEE", product.ShippingUSFee);

                var obj_return = data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
                obj_return.Add("ORIGINAL_WEIGHT", Original_Weight);
                obj_return.Add("LABEL_ID", product.LabelId.ToString());

                return JsonConvert.SerializeObject(obj_return);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailProductCost - ProductController - with " + JsonConvert.SerializeObject(product) + " Error: " + ex.ToString());
            }
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> SyncRedisProduct(ProductSyncModel product)
        {
            try
            {
                var db_index = 1;
                string link_product = string.Empty;
                // Test : PRODUCT_DETAIL_B0019GW3G8_1
                var redis_key = CacheHelper.cacheKeyProductDetail(product.PRODUCT_CODE, product.LABEL_ID);
                var product_cache = await _RedisService.GetAsync(redis_key, db_index);
                var json_obj_edit = string.Empty;

                if (product_cache != null)
                {
                    dynamic objRedis = JsonConvert.DeserializeObject<dynamic>(product_cache);
                    objRedis.price = product.PRODUCT_PRICE;
                    objRedis.amount = product.PRODUCT_PRICE;
                    objRedis.amount_vnd = product.TOTAL_FEE;
                    objRedis.rate = product.RATE_CURRENT;
                    objRedis.product_type = 2;

                    objRedis.item_weight = product.ORIGINAL_WEIGHT;
                    objRedis.list_product_fee.price = product.PRODUCT_PRICE;
                    objRedis.list_product_fee.amount_vnd = product.TOTAL_FEE;

                    objRedis.list_product_fee.list_product_fee.ITEM_WEIGHT = product.ITEM_WEIGHT;
                    objRedis.list_product_fee.list_product_fee.FIRST_POUND_FEE = product.FIRST_POUND_FEE;
                    objRedis.list_product_fee.list_product_fee.NEXT_POUND_FEE = product.NEXT_POUND_FEE;
                    objRedis.list_product_fee.list_product_fee.LUXURY_FEE = product.LUXURY_FEE;
                    objRedis.list_product_fee.list_product_fee.TOTAL_SHIPPING_FEE = product.TOTAL_SHIPPING_FEE;
                    objRedis.list_product_fee.list_product_fee.PRICE_LAST = product.PRICE_LAST;
                    objRedis.list_product_fee.list_product_fee.SHIPPING_US_FEE = product.SHIPPING_US_FEE;


                    link_product = objRedis.link_product;
                    json_obj_edit = JsonConvert.SerializeObject(objRedis);
                }

                if (!string.IsNullOrEmpty(json_obj_edit))
                {
                    _RedisService.clear(redis_key, db_index);
                    _RedisService.Set(redis_key, json_obj_edit, DateTime.Now.AddHours(1), db_index);

                    var productObj = JsonConvert.DeserializeObject<ProductViewModel>(json_obj_edit);
                    IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                    var product_response = _ESRepository.DeleteProductByCode("product", productObj.product_code);
                    if (product_response)
                    {
                        productObj.id = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                        _ESRepository.UpSert(productObj, "product");
                    }
                    if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                    {
                        string j_action_log = "User " + HttpContext.User.FindFirst(ClaimTypes.Name).Value + " đã đồng bộ giá sản phẩm [ " + product.PRODUCT_CODE + " ] . Giá sản phẩm mới:  " + JsonConvert.SerializeObject(product);
                        LoggingActivity.AddLog(int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value), HttpContext.User.FindFirst(ClaimTypes.Name).Value, (int)LogActivityType.PRODUCT_DETAIL_CHANGE, j_action_log);
                    }

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật sản phẩm thành công",
                        product_link = link_product
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Hệ thống không tìm thấy sản phẩm"
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
        public async Task<IActionResult> SetupManualV2()
        {
            var product_code = Request.Query["product_code"].ToString();
            var label_id = Request.Query["label_id"].ToString();

            double _RateCurrent = 0;
            HttpClient httpClient = new HttpClient();
            var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_RATE_CURRENT;
            var result = await httpClient.GetAsync(apiPrefix);
            dynamic resultContent = result.Content.ReadAsStringAsync().Result;
            if (double.TryParse(resultContent, out double rateValue))
                _RateCurrent = rateValue;
            ViewBag.rateVietCom = _RateCurrent;
            ViewBag.LabelList = _LabelRepository.GetListAll();
            ViewBag.WeightUnitList = ViewBag.IndustrySpecialList = await _CommonRepository.GetAllCodeByType(AllCodeType.WEIGHT_UNIT);
            var industrySpecialList = await _CommonRepository.GetAllCodeByType(AllCodeType.INDUSTRY_SPECIAL_TYPE);
            industrySpecialList.Insert(0, new AllCode()
            {
                Type = AllCodeType.INDUSTRY_SPECIAL_TYPE,
                CodeValue = -1,
                CreateDate = DateTime.Now,
                UpdateTime = DateTime.Now,
                Description = "Không thuộc nhóm sản phẩm đặc thù",
                Id = -1,
                OrderNo = 0
            });
            ViewBag.IndustrySpecialList = industrySpecialList;
            ViewBag.product_code = product_code;
            ViewBag.label_id = label_id;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetSpecificationUnit()
        {
            int status = (int)ResponseType.FAILED;
            dynamic data = null;
            var msg = "Failed, Cannot load data";
            try
            {
                data = await _allCodeRepository.GetListSortByName(AllCodeType.PRODUCT_SPECIFICATION);
                status = (int)ResponseType.SUCCESS;
                msg = "Success.";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSpecificationUnit - ProductController - Error: " + ex.ToString());
                status = (int)ResponseType.ERROR;
                msg = ex.ToString();
            }
            return new JsonResult(new
            {
                status = status,
                data = data,
                msg = msg
            });
        }
        [HttpPost]
        public async Task<IActionResult> GetProductLocation(string product_code, int group_id=0)
        {
            int status = (int)ResponseType.FAILED;
            dynamic data = null;
            var msg = "Failed, Cannot load data";
            try
            {
                var list = await _locationProductRepository.GetByProductCode(product_code);
                List<LocationProductViewModel> list_fe = new List<LocationProductViewModel>();
                if (list == null || list.Count < 0)
                {
                    
                    msg = "Empty Data";
                }
                else
                {
                    foreach (var item in list)
                    {
                        string name = await _GroupProductRepository.GetGroupProductNameAsync(item.GroupProductId);
                        list_fe.Add(new LocationProductViewModel()
                        {
                            CreateOn = item.CreateOn,
                            GroupProductId = item.GroupProductId,
                            group_product_name = name,
                            LocationProductId = item.LocationProductId,
                            OrderNo = item.OrderNo,
                            ProductCode = item.ProductCode,
                            UpdateLast = item.UpdateLast,
                            UserId = item.UserId
                        });
                    }
                    status = (int)ResponseType.SUCCESS;
                    msg = "Success.";
                }
                if (list_fe.Count < 1 && group_id > 0)
                {
                    string name = await _GroupProductRepository.GetGroupProductNameAsync(group_id);
                    var group_item=new LocationProductViewModel()
                    {
                        CreateOn = DateTime.Now,
                        GroupProductId = group_id,
                        group_product_name = name,
                        OrderNo = 1,
                        ProductCode = product_code,
                        UpdateLast = DateTime.Now,
                        UserId=0
                    };
                    if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                    {
                        group_item.UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    }
                    list_fe.Add(group_item);
                }
                data = list_fe;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetProductLocation - ProductController - Error: " + ex.ToString());
                status = (int)ResponseType.ERROR;
                msg = ex.ToString();
            }
            return new JsonResult(new
            {
                status = status,
                data = data,
                msg = msg
            });
        }
        public async Task<IActionResult> AddProductLocation(string checked_list = null)
        {
            if (checked_list != null)
            {
                if (!checked_list.Contains("[")) checked_list = "[" + checked_list + "]";
                List<int> list = JsonConvert.DeserializeObject<List<int>>(checked_list);
                ViewBag.html = await _GroupProductRepository.GetListTreeViewCheckBox(US_CATEGORY_ID, -1, list, true);
            }
            else
            {
                ViewBag.html = await _GroupProductRepository.GetListTreeViewCheckBox(US_CATEGORY_ID, -1, null, true);
            }
            return View();
        }
        public IActionResult AddProductSpecificationType()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddNewProductSpecificationType(string type)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Failed";
            dynamic data = null;
            try
            {
                if (type == null || type.Trim() == "")
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Tên kiểu thông số không được bỏ trống";
                }
                else
                {
                    var detail = await _allCodeRepository.GetIDIfValueExists(AllCodeType.PRODUCT_SPECIFICATION, type);
                    if (detail != null)
                    {
                        status = (int)ResponseType.FAILED;
                        msg = "Kiểu thông số đã tồn tại, không thể thêm mới.";
                    }
                    else
                    {
                        var lastest_code = await _allCodeRepository.GetLastestCodeValueByType(AllCodeType.PRODUCT_SPECIFICATION);
                        var lastest_orderno = await _allCodeRepository.GetLastestOrderNoByType(AllCodeType.PRODUCT_SPECIFICATION);
                        var model = new AllCode()
                        {
                            Type = AllCodeType.PRODUCT_SPECIFICATION,
                            CodeValue = (short)(lastest_code + 1),
                            CreateDate = DateTime.Now,
                            Description = type,
                            OrderNo = (short)(lastest_orderno + 1),
                            UpdateTime = DateTime.Now
                        };
                        var id = await _allCodeRepository.Create(model);
                        if (id > 0)
                        {
                            status = (int)ResponseType.SUCCESS;
                            msg = "Tạo mới kiểu thông số sản phẩm thành công";
                            data = model;
                        }
                        else
                        {
                            msg = "Tạo mới kiểu thông số sản phẩm thất bại";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddProductSpecificationType - ProductController - Error: " + ex.ToString());
                status = (int)ResponseType.ERROR;
                msg = ex.ToString();
            }
            return new JsonResult(new
            {
                status = status,
                msg = msg,
                data = data
            });
        }
        public IActionResult ProductFee(string rate, string price, string shiping_fee, string amount_vnd, string FIRST_POUND_FEE, string NEXT_POUND_FEE, string LUXURY_FEE, string TOTAL_SHIPPING_FEE, string PRICE_LAST)
        {
            ViewBag.rate = rate;
            ViewBag.price = price;
            ViewBag.shiping_fee = shiping_fee;
            ViewBag.amount_vnd = amount_vnd;
            ViewBag.FIRST_POUND_FEE = FIRST_POUND_FEE;
            ViewBag.NEXT_POUND_FEE = NEXT_POUND_FEE;
            LUXURY_FEE = LUXURY_FEE == null || LUXURY_FEE.Trim() == "" ? "0" : LUXURY_FEE;
            ViewBag.LUXURY_FEE = LUXURY_FEE;
            ViewBag.TOTAL_SHIPPING_FEE = TOTAL_SHIPPING_FEE;
            ViewBag.PRICE_LAST = PRICE_LAST;
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> SummitProductImages(string json = null)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Data invalid.";
            dynamic data = null;
            try
            {
                if (json == null || json == "[]")
                {
                    status = (int)ResponseType.SUCCESS;
                    msg = "Đã cập nhật mới.";
                }
                else
                {
                    var url_list = new List<string>();
                    var list = JsonConvert.DeserializeObject<List<string>>(json);
                    string url = null;
                    foreach (var base64 in list)
                    {
                        if (base64 == null || base64.Trim() == "")
                        {

                        }
                        else if (base64.Contains("media-amazon.com") || base64.StartsWith("http://")|| base64.StartsWith("https://"))
                        {
                            url_list.Add(base64);
                        }
                        else
                        {
                            url = await UpLoadHelper.UploadBase64Src(base64, _UrlStaticImage);
                            // url = "NEWURL";
                            if (url != string.Empty)
                            {
                                if (!url.Contains(_UrlStaticImage)) url = _UrlStaticImage + url;
                                url_list.Add(url);
                            }
                        }
                    }
                    if (url_list.Count > 0)
                    {
                        data = JsonConvert.SerializeObject(url_list);
                        status = (int)ResponseType.SUCCESS;
                        msg = "Cập nhật ảnh sản phẩm thành công";
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        msg = "Bạn hãy chọn ít nhất 1 ảnh đại đại diện cho sản phẩm";
                    }

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SummitProductImages - ProductController - Error: " + ex.ToString());
                status = (int)ResponseType.ERROR;
                msg = ex.ToString();
            }
            return new JsonResult(new
            {
                status = status,
                msg = msg,
                data = data
            });

        }
        public IActionResult FormLockManual()
        {
            ViewBag.LabelList = _LabelRepository.GetListAll();
            return View();
        }

        public IActionResult ProductConfiguration()
        {
            ViewBag.LabelList = _LabelRepository.GetListAll();
            return View();
        }
        public async Task<IActionResult> ProductConfigurationTable(string keywords, int label_id = -1, int keyword_type = -1, int product_status = -2)
        {
            try
            {
                var label_list = _LabelRepository.GetListAll();
                var model = new List<ProductBlackListViewModel>();
                var data = await _productMongoAccess.GetByKeywords(keywords, label_id, keyword_type, product_status);
                if (data == null || data.Count < 1)
                {
                }
                else
                {
                    foreach (var v in data)
                    {
                        var keyword_type_str = "Từ khóa";
                        if (v.keyword_type == 0)
                        {
                            keyword_type_str = "Mã sản phẩm";
                        }
                        var statusname = "Mở khóa / Bỏ chặn";
                        switch (v.product_status)
                        {
                            case -1:
                                {
                                    statusname = "Chặn";
                                }
                                break;
                            case 1:
                                {
                                    statusname = "Khóa";
                                }
                                break;
                            default: break;
                        }
                        model.Add(new ProductBlackListViewModel()
                        {
                            _id = v._id,
                            create_date = v.create_date,
                            create_date_string = v.create_date.ToString("dd/MM/yyyy HH:mm:ss"),
                            keywords = v.keywords,
                            keyword_type = v.keyword_type,
                            label_id = v.label_id,
                            label_name = label_list.Where(i => i.Id == v.label_id).FirstOrDefault().StoreName,
                            product_status = v.product_status,
                            status_name = statusname,
                            keyword_type_str = keyword_type_str
                        });
                    }
                }
                return PartialView(model);
            }
            catch (Exception)
            {

            }
            return Content("");
        }
        [HttpPost]
        public async Task<IActionResult> SummitLockProduct(int product_status, string keywords, int keyword_type, int label_id)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Not Implemented";
            try
            {
                if (product_status < -1 || keywords == null || keywords.Trim() == "" || keyword_type < 0 || label_id < 1)
                {
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                }
                else if (product_status == (int)ProductStatus.NORMAL_SELL)
                {
                    return await DeleteLockProduct(product_status, keywords, keyword_type, label_id);
                }
                else
                {
                    var msg_create = "Added: ";
                    var msg_update = "Updated: ";
                    var msg_failed = "Failed: ";
                    var kw = keywords.Trim().Replace(" ", "").Split(",");
                    foreach (var item in kw)
                    {
                        var exsisting_model = await _productMongoAccess.Find(item, keyword_type, label_id);
                        if (exsisting_model == null || exsisting_model._id == null || exsisting_model._id.Trim() == "")
                        {
                            exsisting_model = new ProductBlackList()
                            {
                                product_status = product_status,
                                label_id = label_id,
                                keyword_type = keyword_type,
                                keywords = item,
                                create_date = DateTime.Now
                            };
                            exsisting_model.GenID();
                            var id = _productMongoAccess.AddNewAsync(exsisting_model);
                            if (id == null)
                            {
                                msg_failed += item + ". ";
                            }
                            else
                            {
                                msg_create += item + ". ";
                            }
                        }
                        else
                        {
                            exsisting_model.product_status = product_status;
                            exsisting_model.create_date = DateTime.Now;
                            var id = _productMongoAccess.UpdateAsync(exsisting_model);
                            if (id == null)
                            {
                                msg_failed += item + ". ";
                            }
                            else
                            {
                                msg_update += item + ". ";
                            }
                        }
                    }
                    msg = "";
                    if (!msg_create.Equals("Added: "))
                    {
                        msg += msg_create + "\n";
                    }
                    if (!msg_update.Equals("Updated: "))
                    {
                        msg += msg_update + "\n";
                    }
                    if (msg_failed.Equals("Failed: "))
                    {
                        status = (int)ResponseType.SUCCESS;
                    }
                    else
                    {
                        msg += msg_failed;
                        status = (int)ResponseType.FAILED;

                    }
                }
                _productMongoAccess.CacheData(_RedisService, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SummitLockProduct - ProductController - Error: " + ex.ToString());

            }
            return new JsonResult(new
            {
                status = status,
                msg = msg
            });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLockProduct(int product_status, string keywords, int keyword_type, int label_id)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Not Implemented";
            try
            {
                if (product_status < -1 || keywords == null || keywords.Trim() == "" || keyword_type < 0 || label_id < 1)
                {
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                }
                else
                {
                    var msg_del = "Deleted: ";
                    var msg_failed = "Failed: ";
                    var kw = keywords.Trim().Replace(" ", "").Split(",");
                    foreach (var item in kw)
                    {
                        var exsisting_model = await _productMongoAccess.Find(keywords, keyword_type, label_id);
                        if (exsisting_model == null || exsisting_model._id == null || exsisting_model._id.Trim() == "")
                        {
                            msg_failed += item + ". ";
                        }
                        else
                        {
                            var id = await _productMongoAccess.DeleteAsync(exsisting_model);
                            status = (int)ResponseType.SUCCESS;
                            msg = "Deleted: " + exsisting_model.keywords;
                            if (id == null)
                            {
                                msg_failed += item + ". ";
                            }
                            else
                            {
                                msg_del += item + ". ";
                            }
                        }

                    }
                    msg = "";
                    if (!msg_del.Equals("Deleted: "))
                    {
                        msg += msg_del + "\n";
                    }
                    if (msg_failed.Equals("Failed: "))
                    {
                        status = (int)ResponseType.SUCCESS;
                    }
                    else
                    {
                        status = (int)ResponseType.FAILED;
                        msg += msg_failed;
                    }

                }
                _productMongoAccess.CacheData(_RedisService, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteLockProduct - ProductController - Error: " + ex.ToString());

            }
            return new JsonResult(new
            {
                status = status,
                msg = msg
            });
        }
        private async Task SyncProductSpecification(Dictionary<string, string> product_specification)
        {
            try
            {

                if (product_specification != null && product_specification.Count > 0)
                    foreach (var a in product_specification)
                    {
                        var detail = await _allCodeRepository.GetIDIfValueExists(AllCodeType.PRODUCT_SPECIFICATION, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.Key.Trim().ToLower()));
                        if (detail != null)
                        {
                            continue;
                        }
                        else
                        {
                            var lastest_code = await _allCodeRepository.GetLastestCodeValueByType(AllCodeType.PRODUCT_SPECIFICATION);
                            var lastest_orderno = await _allCodeRepository.GetLastestOrderNoByType(AllCodeType.PRODUCT_SPECIFICATION);
                            var model = new AllCode()
                            {
                                Type = AllCodeType.PRODUCT_SPECIFICATION,
                                CodeValue = (short)(lastest_code + 1),
                                CreateDate = DateTime.Now,
                                Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.Key.Trim().ToLower()),
                                OrderNo = (short)(lastest_orderno + 1),
                                UpdateTime = DateTime.Now
                            };
                            var id = await _allCodeRepository.Create(model);
                        }
                    }
            }
            catch (Exception)
            {

            }
        }
        public IActionResult CrawlOfflinePopup()
        {
            ViewBag.LabelList = _LabelRepository.GetListAll();
            return View();
        }
        [HttpPost]
        public IActionResult CrawlASINOffline(string ASIN, int label_id, int group_id = 344)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Not Implemented";
            try
            {
                if (ASIN == null || ASIN.Trim() == "" || label_id < 1)
                {
                    msg = "Thông tin không chính xác, vui lòng kiểm tra lại";
                }
                else
                {
                    var list = ASIN.Split(",");
                    foreach (var product_code in list)
                    {
                        var obj = new
                        {
                            group_id = group_id,
                            label_id = label_id,
                            url = "https://www.amazon.com/dp/" + product_code,
                            product_code = product_code,
                            from_parent_url = "https://www.amazon.com"
                        };
                        string api_url = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                        HttpClient httpClient = new HttpClient();

                        string j_param = "{'data_push':'" + JsonConvert.SerializeObject(obj) + "','type':'group_product_mapping_detail'}";
                        string token = CommonHelper.Encode(j_param, ReadFile.LoadConfig().EncryptApi);
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("token", token),
                        });
                        var result = httpClient.PostAsync(api_url, content);
                    }
                }
                status = (int)ResponseType.SUCCESS;
                msg = "Gửi yêu cầu thành công";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CrawlASINOffline - ProductController - Error: " + ex.ToString());
                msg = "Error on Excution";
            }
            return new JsonResult(new
            {
                status = status,
                msg = msg
            });
        }

        [HttpPost]
        public IActionResult SyncProductMongoDB()
        {
            SyncMongoDB();
            return Ok(new
            {
                status = (int)ResponseType.SUCCESS,
                msg = "Gửi yêu cầu thành công"
            });

        }
        [HttpPost]
        public IActionResult SyncFixedPrice()
        {
            UpdateFixedPrice();
            return Ok(new
            {
                status = (int)ResponseType.SUCCESS,
                msg = "Gửi yêu cầu thành công"
            });

        }
        private async Task SyncMongoDB()
        {
            try
            {
                // ESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                /*
                 var product_list = await _productDetailMongoAccess.GetCoreDetail();
                 foreach(var product in product_list)
                 {
                     //-- Push Queue Offline:
                     HttpClient httpClient_2 = new HttpClient();
                     var apiQueueService = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                     string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                     var item = new
                     {
                         product_manual_key_id = product._id,
                         group_id = "396"
                     };
                     var j_param = new Dictionary<string, string>()
                         {
                             {"data_push", JsonConvert.SerializeObject(item) },
                             { "type",TaskQueueName.product_detail_manual_queue}
                         };
                     var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                     var content = new FormUrlEncodedContent(new[]
                     {
                             new KeyValuePair<string, string>("token", token),
                         });
                     var result_2 = await httpClient_2.PostAsync(apiQueueService, content);
                     dynamic resultContent_2 = Newtonsoft.Json.Linq.JObject.Parse(result_2.Content.ReadAsStringAsync().Result);
                     var msg = resultContent_2.msg;
                 }
                
                ESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                 var list = _ESRepository.GetListProductManual();
                 if (list != null && list.Count > 0)
                     foreach (var product in list)
                     {
                         ProductMongoViewModel model = new ProductMongoViewModel()
                         {
                             product_detail = product
                         };
                         var id = await _productDetailMongoAccess.FindIDByProductCode(product.product_code);
                         if (id != null && id.Trim() != "")
                         {
                             model._id = id;
                             await _productDetailMongoAccess.UpdateAsync(model);
                         }
                         else
                         {
                             model.GenID();
                             await _productDetailMongoAccess.AddNewAsync(model);
                         }
                     }
                
                var list_id = await _productDetailMongoAccess.GetCoreDetail();*/
                ESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                var list_id = await _productDetailMongoAccess.GetDetailByLabelID(2);
                
                foreach (var detail in list_id)
                {
                    detail.link_product = CommonHelper.genLinkDetailProduct(detail.label_name, detail.product_code, CommonHelper.RemoveUnicode(detail.product_name).Trim());
                    var delete_complete = _ESRepository.DeleteProductByCode("product", detail.product_code);
                    if (delete_complete)
                    {
                        _ESRepository.UpSert(detail, "product");
                    }
                    var detail_json = JsonConvert.SerializeObject(detail);
                    // -- Push Redis:
                    string cache_name = CacheHelper.cacheKeyProductDetail(detail.product_code, detail.label_id);
                    int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                    switch (detail.label_id)
                    {
                        case (int)LabelType.amazon:
                            {
                                db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                            }
                            break;
                        case (int)LabelType.jomashop:
                            {
                                db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_jomashop"]);
                            }
                            break;
                        case (int)LabelType.costco:
                            {
                                db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_costco"]);
                            }
                            break;
                    }
                    _RedisService.Set(cache_name, detail_json, db_index);
                    _RedisService.Set(cache_name, detail_json, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                    //var location_product = await _locationProductRepository.GetByProductCode(detail.product_code);
                    /*
                    string group_id = "";
                    if (location_product != null && location_product.Count > 0)
                    {
                       var is_first = true;
                       foreach(var location in location_product)
                       {
                            if (is_first)
                            {
                                is_first = false;
                            }
                            else
                            {
                                group_id += ",";
                            }
                            group_id += location.GroupProductId;
                       }
                    }

                    //-- Push Queue Offline:
                    HttpClient httpClient_2 = new HttpClient();
                    var apiQueueService = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_PUSH_DATA_TO_QUEUE;
                    string EncryptApi = ReadFile.LoadConfig().EncryptApi;
                    var item = new
                    {
                        product_manual_key_id = detail._id,
                        group_id = group_id
                    };
                    var j_param = new Dictionary<string, string>()
                        {
                            {"data_push", JsonConvert.SerializeObject(item) },
                            { "type",TaskQueueName.product_detail_manual_queue}
                        };
                    var token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                    var content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("token", token),
                        });
                    var result_2 = await httpClient_2.PostAsync(apiQueueService, content);
                    dynamic resultContent_2 = Newtonsoft.Json.Linq.JObject.Parse(result_2.Content.ReadAsStringAsync().Result);
                    var msg = resultContent_2.msg;
                    count++;
                    */
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS/SyncMongoDB - ProductController - Error: " + ex.ToString());
            }
        }
        private async Task UpdateFixedPrice()
        {
            try
            {
                ESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                var list_id = await _productDetailMongoAccess.GetFullDetailByLabelID(2);
                foreach(var model_mongo in list_id)
                {
                    var model = model_mongo.product_detail;
                    if (model.product_type == (int)ProductType.FIXED_AMOUNT_VND)
                    {
                        model.rate = 20000;
                        var fee_model = new ProductBuyerViewModel()
                        {
                            IndustrySpecialType = 0,
                            LabelId = 2,
                            Pound = 1,
                            Price = 0,
                            RateCurrent = 20000,
                            Unit = 1
                        };
                        var rs = await GetFeeFixed(fee_model, model.amount_vnd) as OkObjectResult;
                        var rs1 = rs.Value as dynamic;
                        var product_fee_list = JsonConvert.DeserializeObject<Dictionary<string, string>>(rs1.data);
                        if (model.list_product_fee == null) model.list_product_fee = new ProductFeeViewModel();
                        model.list_product_fee.shiping_fee = model.shiping_fee;
                        model.list_product_fee.list_product_fee = new Dictionary<string, double>();
                        model.list_product_fee.list_product_fee.Add("ITEM_WEIGHT", Convert.ToDouble(product_fee_list["ITEM_WEIGHT"]));
                        model.list_product_fee.list_product_fee.Add("FIRST_POUND_FEE", Convert.ToDouble(product_fee_list["FIRST_POUND_FEE"]));
                        model.list_product_fee.list_product_fee.Add("NEXT_POUND_FEE", Convert.ToDouble(product_fee_list["NEXT_POUND_FEE"]));
                        model.list_product_fee.list_product_fee.Add("LUXURY_FEE", Convert.ToDouble(product_fee_list["LUXURY_FEE"]));
                        model.list_product_fee.list_product_fee.Add("DISCOUNT_FIRST_FEE", Convert.ToDouble(product_fee_list["DISCOUNT_FIRST_FEE"]));
                        model.list_product_fee.list_product_fee.Add("TOTAL_SHIPPING_FEE", Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2));
                        model.list_product_fee.list_product_fee.Add("PRICE_LAST", Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]), 2));
                        model.list_product_fee.label_name = LabelNameType.GetLabelName(model.label_id);
                        model.list_product_fee.total_fee = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2);
                        var price = Math.Round((model.amount_vnd / model.rate) - Convert.ToDouble(product_fee_list["FIRST_POUND_FEE"]) - Convert.ToDouble(product_fee_list["NEXT_POUND_FEE"]), 2);
                        model.list_product_fee.price = price;
                        model.price = price;
                        model.list_product_fee.amount_vnd = model.amount_vnd;
                        model.rate = Math.Round((model.amount_vnd / Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]), 2)), 4);
                        model.item_weight = "1 pounds";

                        //-- Push ES:
                        var delete_complete = _ESRepository.DeleteProductByCode("product", model.product_code);
                        if (delete_complete)
                        {
                            _ESRepository.UpSert(model, "product");
                        }
                        // -- Push Redis:
                        string cache_name = CacheHelper.cacheKeyProductDetail(model.product_code, model.label_id);
                        int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                        switch (model.label_id)
                        {
                            case (int)LabelType.amazon:
                                {
                                    db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                                }
                                break;
                            case (int)LabelType.jomashop:
                                {
                                    db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_jomashop"]);
                                }
                                break;
                            case (int)LabelType.costco:
                                {
                                    db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_costco"]);
                                }
                                break;
                        }
                        _RedisService.Set(cache_name, JsonConvert.SerializeObject(model), db_index);
                        _RedisService.Set(cache_name, JsonConvert.SerializeObject(model), Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                        //--Pushback Mongo:
                        model_mongo.product_detail = model;
                        var update = await _productDetailMongoAccess.UpdateAsync(model_mongo);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS/SyncMongoDB - ProductController - Error: " + ex.ToString());
            }
        }
        /*
[HttpPost]
public async Task<IActionResult> CheckOut(string json)
{
    int status = 0;
    string msg = "Not Implemented";
    dynamic data = null;
    ProductViewModel model = null;

    try
    {
        try
        {
            model = JsonConvert.DeserializeObject<ProductViewModel>(json);
        }
        catch (Exception)
        {

        }
        var regexItem = new Regex("^[a-zA-Z0-9 -_]*$");
        if (model == null || model.product_code == null || model.product_name == null || model.product_code == "" || model.product_name == "" || model.price <= 0 || model.rate <= 0
             || model.item_weight == null || model.item_weight == "" || model.label_id < 0 || model.image_thumb == null | model.image_thumb == "")
        {
            status = (int)ResponseType.FAILED;
            msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
        }
        else if (!regexItem.IsMatch(model.product_code))
        {
            status = (int)ResponseType.FAILED;
            msg = "Mã sản phẩm không chính xác, vui lòng thử lại";

        }
        else
        {
            switch (model.label_id)
            {
                case (int)LabelType.amazon:
                    {
                        model.link_product = CommonHelper.genLinkDetailProduct(LabelNameType.amazon, model.product_code, RemoveSpecialCharacters(model.product_name).Trim());
                        model.label_name = "amazon";
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
            int unit_weight_id = 1;
            switch (model.item_weight.Split(" ")[1].Trim())
            {
                case "ounces":
                    unit_weight_id = 0;
                    break;
                case "pounds":
                    unit_weight_id = 1;

                    break;
                case "grams":
                    unit_weight_id = 2;

                    break;
                case "kilograms":
                    unit_weight_id = 3;

                    break;
                case "tonne":
                    unit_weight_id = 4;

                    break;
                case "kiloton":
                    unit_weight_id = 5;
                    break;
            }
            model.update_last = DateTime.Now.AddDays(7);
            model.is_crawl_weight = true;
            model.regex_step = 2;
            model.is_has_seller = false;
            model.is_redirect_extension = false;
            model.amount = model.price + model.shiping_fee;
            var product_fee_list = JsonConvert.DeserializeObject<Dictionary<string, string>>(await GetDetailProductCost(new ProductBuyerViewModel()
            {
                IndustrySpecialType = model.industry_special_type,
                LabelId = model.label_id,
                Pound = Convert.ToDouble(model.item_weight.Split(" ")[0]),
                Price = model.amount,
                RateCurrent = model.rate,
                Unit = unit_weight_id
            }));
            if (model.image_product == null)
            {
                model.image_product = new List<string>();
                model.image_product.Add(model.image_thumb);
            }
            if (model.variation_name == null)
            {
                model.variation_name = new List<string>();
                model.variation_name.Add("Default");
            }
            model.keywork_search = "https://www.amazon.com/dp/" + model.product_code;
            if (model.image_size_product == null || model.image_size_product.Count < 1)
            {
                model.image_size_product = new List<ImageSizeViewModel>();
                model.image_size_product.Add(new ImageSizeViewModel()
                {
                    Larger = model.image_thumb,
                    Thumb = model.image_thumb
                });
            }

            if (model.list_product_fee == null) model.list_product_fee = new ProductFeeViewModel();
            model.list_product_fee.shiping_fee = model.shiping_fee;
            model.list_product_fee.list_product_fee = new Dictionary<string, double>();
            model.list_product_fee.list_product_fee.Add("ITEM_WEIGHT", Convert.ToDouble(product_fee_list["ITEM_WEIGHT"]));
            model.list_product_fee.list_product_fee.Add("FIRST_POUND_FEE", Convert.ToDouble(product_fee_list["FIRST_POUND_FEE"]));
            model.list_product_fee.list_product_fee.Add("NEXT_POUND_FEE", Convert.ToDouble(product_fee_list["NEXT_POUND_FEE"]));
            model.list_product_fee.list_product_fee.Add("LUXURY_FEE", Convert.ToDouble(product_fee_list["LUXURY_FEE"]));
            model.list_product_fee.list_product_fee.Add("DISCOUNT_FIRST_FEE", Convert.ToDouble(product_fee_list["DISCOUNT_FIRST_FEE"]));
            model.list_product_fee.list_product_fee.Add("TOTAL_SHIPPING_FEE", Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2));
            model.list_product_fee.list_product_fee.Add("PRICE_LAST", Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]), 2));

            model.list_product_fee.label_name = LabelNameType.GetLabelName(model.label_id);
            model.list_product_fee.price = model.amount;
            model.list_product_fee.total_fee = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2);
            model.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_FEE"]), 0);
            model.list_product_fee.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_FEE"]), 0);
            data = JsonConvert.SerializeObject(model.list_product_fee.list_product_fee);
            status = (int)ResponseType.SUCCESS;
            msg = "Success";
        }
    }
    catch (Exception ex)
    {
        LogHelper.InsertLogTelegram("CheckOut - ProductController" + ex);

    }
    return Ok(new
    {
        status = status,
        msg = msg,
        data = data,
        model = model
    });

}
*/
        /*
       [HttpPost]
       public IActionResult CreateProductManualFromJSON(string ASIN, string json_product)
       {
           int status = (int)ResponseType.FAILED;
           string msg = "Data gửi lên không chính xác, vui lòng kiểm tra lại";
           dynamic data = null;
           dynamic model = null;
           try
           {
               if (ASIN == null || ASIN == "" || json_product == null || json_product == "")
               {

               }
               else
               {
                   ProductViewModel product_detail = null;
                   try
                   {
                       product_detail = JsonConvert.DeserializeObject<ProductViewModel>(json_product);

                   }
                   catch (Exception)
                   {
                       status = (int)ResponseType.FAILED;
                       msg = "Data gửi lên không chính xác, vui lòng kiểm tra lại";
                   }
                   var regexItem = new Regex("^[a-zA-Z0-9 -_]*$");
                   if (product_detail == null || product_detail.product_code == null || product_detail.product_name == null || product_detail.product_code == "" || product_detail.product_name == "" || product_detail.price <= 0 || product_detail.rate <= 0
                       || product_detail.item_weight == null || product_detail.item_weight == "" || product_detail.label_id < 0 || product_detail.image_thumb == null | product_detail.image_thumb == "")
                   {
                       status = (int)ResponseType.FAILED;
                       msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                   }
                   else if (!regexItem.IsMatch(product_detail.product_code))
                   {
                       status = (int)ResponseType.FAILED;
                       msg = "Mã sản phẩm không chính xác, vui lòng thử lại";

                   }
                   else
                   {
                       //-- Set Product_type to 1: Manual set:
                       product_detail.product_type = ProductType.MANUAL_EDIT;

                       string cache_name = CacheHelper.cacheKeyProductDetail(product_detail.product_code, product_detail.label_id);
                       _RedisService.Set(cache_name, JsonConvert.SerializeObject(product_detail), DateTime.Now.AddHours(1), Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));
                       //-- Push ES
                       IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                       // Build id identity:
                       var product_response = _ESRepository.DeleteProductByCode("product", product_detail.product_code);
                       if (product_response) // delete thành công thì mới dc add new
                       {
                           long unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                           product_detail.id = unixTime;

                           var result_push_es = _ESRepository.UpSert(product_detail, "product");
                           if (result_push_es > 0)
                           {
                               status = (int)ResponseType.SUCCESS;
                               msg = "Tạo mới / Cập nhật dữ liệu thành công";
                               data = JsonConvert.SerializeObject(product_detail.list_product_fee.list_product_fee);
                               model = product_detail;
                           }
                           else
                           {
                               LogHelper.InsertLogTelegram("CreateProductManualFromJSON - ProductController - [" + product_detail.product_code + "] Push Data to ES Error to: " + _Configuration["DataBaseConfig:Elastic:Host"]);
                               status = (int)ResponseType.FAILED;
                               msg = "Tạo mới / Cập nhật thất bại. [" + product_detail.product_code + "] Push Data Error";
                           }

                       }
                       else
                       {
                           LogHelper.InsertLogTelegram("CreateProductManualFromJSON - ProductController - [" + product_detail.product_code + "] Push Data to ES Error to: " + _Configuration["DataBaseConfig:Elastic:Host"]);
                           status = (int)ResponseType.FAILED;
                           msg = "Tạo mới / Cập nhật thất bại. [" + product_detail.product_code + "] Delete Data Error";
                       }
                   }

               }
           }
           catch (Exception ex)
           {
               LogHelper.InsertLogTelegram("CreateProductManualFromJSON - ProductController - Excution: " + ex);
               status = (int)ResponseType.ERROR;
               msg = "Error on Excution";
           }
           return Ok(new
           {
               status = status,
               msg = msg,
               data = data,
               model = model
           });
       }
       */
        /*
public IActionResult ProductCategory()
{
    return View();
}


[HttpPost]
public IActionResult GetListLink(List<String> listLink)
{
    try
    {
        return new JsonResult(new
        {
            message = "Gửi dữ liệu thành công."
        });
    }
    catch (Exception ex)
    {
        return new JsonResult(new
        {
            message = "Lỗi khi gửi file lên server."
        });
    }
}

[HttpPost]
public IActionResult GetMenu()
{
    try
    {
        return new JsonResult(new
        {
            message = "Gửi dữ liệu thành công."
        });
    }
    catch (Exception ex)
    {
        return new JsonResult(new
        {
            message = "Lỗi khi gửi file lên server."
        });
    }
}

public async Task<IActionResult> SetupManual()
{
    var product_code = Request.Query["product_code"].ToString();
    var label_id = Request.Query["label_id"].ToString();
    var group_id = Request.Query["group_id"].ToString();

    double _RateCurrent = 0;
    HttpClient httpClient = new HttpClient();
    var apiPrefix = ReadFile.LoadConfig().API_CMS_URL + ReadFile.LoadConfig().API_RATE_CURRENT;
    var result = await httpClient.GetAsync(apiPrefix);
    dynamic resultContent = result.Content.ReadAsStringAsync().Result;
    if (double.TryParse(resultContent, out double rateValue))
        _RateCurrent = rateValue;
    ViewBag.rateVietCom = Math.Round(_RateCurrent, 2);
    ViewBag.IndustrySpecialList = await _CommonRepository.GetAllCodeByType(AllCodeType.INDUSTRY_SPECIAL_TYPE);
    ViewBag.product_code = product_code;
    ViewBag.label_id = label_id;
    return View();
}
  */
    }
}