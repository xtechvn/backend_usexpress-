using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.UI.Controllers.Order;
using WEB.UI.Service;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers.GroupProduct
{
    // [Route("[controller]")]
    public class GroupProductController : Controller
    {
        private readonly IConfiguration Configuration;
        public readonly IGroupProductRepository GroupProductRepository;
        private readonly RedisConn redisService;
        public GroupProductController(IGroupProductRepository _groupProductRepository, IConfiguration _Configuration, RedisConn _redisService)
        {
            GroupProductRepository = _groupProductRepository;
            Configuration = _Configuration;
            redisService = _redisService;

        }

        /// <summary>
        /// gr_type: cat: là 1 nhóm sản phẩm | camp: nhiều nhóm sản phẩm
        /// </summary>
        /// <param name="gr_type"></param>
        /// <param name="path"></param>
        /// <returns></returns>


        [Route("{path}-cat")]
        [Route("{path}-cat/p{cur_page}")]
        [Route("{path}-all/{camp_id}")]
        [Route("{path}-all/{camp_id}/p{cur_page}")]
        [HttpGet]
        //[HttpGet]
        public async Task<IActionResult> getListProductInGroup(string path, [DefaultValue(0)] int camp_id, [DefaultValue(1)] int cur_page)
        {
            try
            {
                int skip = cur_page == 1 ? 0 : cur_page * 10;
                int take = 20; // 20 sp tren 1 page

                var lst_group_id = new List<int>();
                string group_product_name = string.Empty;
                // get group info                
                switch (path)
                {
                    case "camp":
                        #region Get list id theo chien dich

                        string cache_key = CacheType.CAMPAIGN_ID_ + camp_id;
                        var j_data = await redisService.GetAsync(cache_key, Convert.ToInt32(Configuration["Redis:Database:db_common"]));
                        if (!string.IsNullOrEmpty(j_data))
                        {
                            var JsonParent = JArray.Parse("[" + j_data + "]");

                            var camp_detail = JsonConvert.DeserializeObject<List<ViewModels.GroupProductViewModel>>(JsonParent[0]["data"].ToString()).ToList(); // hieu la 1 camp_id
                            string s_camp_group_id = string.Join(",", camp_detail.Select(x => x.id));

                            lst_group_id = Array.ConvertAll(s_camp_group_id.Split(","), s => (int.Parse(s))).ToList(); // hieu la 1 camp_id
                        }
                        else
                        {
                            return Redirect("/Error/2");
                        }
                        #endregion
                        break;
                    default:
                        var gr_service = new GroupProductService(Configuration, redisService);
                        var group_detail = await gr_service.getGroupProductDetailByPath(path);
                        if (group_detail != null)
                        {
                            group_product_name = group_detail.name;
                            lst_group_id.Add(group_detail.id);
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "fe-getListProductInGroup not found link [path = " + path + "]");
                        }
                        break;
                }

                // Lấy ra nhóm sp thuộc chuyên mục
                if (lst_group_id.Count > 0)
                {

                    // get ES                    
                    string ES_HOST = Configuration["DataBaseConfig:Elastic:Host"];
                    var ESRepository = new ESRepository<object>(ES_HOST);
                    var result_gr = await ESRepository.getProductListByGroupProductId(Configuration["DataBaseConfig:Elastic:index_product_search"], lst_group_id, skip, take, "all");
                    if (result_gr != null)
                    {
                        var gr_list = result_gr.obj_lst_product_result.Select(o =>
                            new ProductListViewModel
                            {
                                image_url = o.image_thumb,
                                url = o.link_product,
                                product_name = o.product_name,
                                star = o.star,
                                reviews_count = o.reviews_count,
                                url_store = o.keywork_search,
                                amount = o.amount_vnd
                            }).ToList();

                        //phan trang
                        var pagination_model = new PaginationEntitiesViewModel
                        {
                            base_url = "/" + (path == "camp" ? (path + "-all/" + camp_id + "/p{cur_page}") : (path + "-cat/p{cur_page}")),
                            cur_page = cur_page,
                            total_item_store = result_gr.total_item_store,
                            per_page = take
                        };

                        // page san pham dau tien
                        var model = new GroupProductEntitiesViewModel
                        {
                            Pagination = pagination_model,
                            obj_lst_product_result = gr_list,
                            group_product_name = group_product_name
                        };
                        return View("GroupListProduct", model);
                    }
                    else
                    {
                        return Redirect("/Error/2");
                    }
                }
                else
                {
                    return Redirect("/Error/2");
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getListProductInGroup [path = " + path + "] error: " + ex.ToString());
                return Redirect("/Error/2");
            }
        }

        /// <summary>
        /// Box nhóm hàng trang chủ
        /// </summary>
        /// <returns></returns>
        [Route("group-product/choice.json")]
        [HttpPost]
        public async Task<IActionResult> getProductGroupChoice()
        {
            try
            {
                var gr_service = new GroupProductService(Configuration, redisService);
                var group_choice = await gr_service.GetFeaturedGroupProduct();
                return Json(new { status = (int)ResponseType.SUCCESS, gr = await this.RenderViewToStringAsync("PartialView/Menu/BestGroupProduct", group_choice) });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getProductGroupChoice  error: " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR });
            }
        }

        /// <summary>
        /// lấy ra danh sách sp theo chuyên mục
        /// </summary>
        /// <param name="product_code"></param>
        /// <returns></returns>
        [HttpPost("/group_product/render-product-by-group-id.json")]
        public async Task<IActionResult> getProductByGroupId(int group_product_id, int skip, int take, int label_id, string partial_view)
        {
            try
            {
                var arr_product_code = new List<string>();
                int db_index = Convert.ToInt32(Configuration["Redis:Database:db_common"]);
                string CACHE_GROUP_PRODUCT_COSTCO = CacheType.GROUP_PRODUCT + group_product_id + "_" + label_id;

                // Truyền group_id = - 1 thì sẽ get ra all sp trong mục theo skip take
                if (group_product_id > 0)
                {
                    var gr_service = new GroupProductService(Configuration, redisService);
                    var product_list = await gr_service.getProductCodeByGroupId(group_product_id);

                    if (product_list == null) return Redirect("/Error/2");


                    if (product_list.Count > 0)
                    {
                        arr_product_code = product_list.Select(s => (string)s.product_code).ToList();
                    }

                    var j_product_list_detail = await redisService.GetAsync(CACHE_GROUP_PRODUCT_COSTCO, db_index);
                    if (!string.IsNullOrEmpty(j_product_list_detail) && j_product_list_detail != "null")
                    {
                        // Đọc từ Redis 
                        var data_cache = JsonConvert.DeserializeObject<List<ProductViewModel>>(j_product_list_detail);
                        return Json(new { status = (int)ResponseType.SUCCESS, total_item_store = data_cache.Count, data = await this.RenderViewToStringAsync("PartialView/" + partial_view.Replace("-", "/"), data_cache) });
                    }
                }

                // get product detail by product code list                    
                string ES_HOST = Configuration["DataBaseConfig:Elastic:Host"];
                var ESRepository = new ESRepository<object>(ES_HOST);
                var result_gr = await ESRepository.getProductDetailByProductCodeList(Configuration["DataBaseConfig:Elastic:index_product_search"], arr_product_code, skip, take, label_id);

                if (result_gr != null)
                {
                    var gr_list = result_gr.obj_lst_product_result.Select(o =>
                        new ProductViewModel
                        {
                            product_code=o.product_code,
                            image_thumb = o.image_thumb,
                            link_product = o.link_product,
                            product_name = o.product_name,
                            star = o.star,
                            reviews_count = o.reviews_count,
                            amount_vnd = o.amount_vnd
                        }).ToList();

                    if (group_product_id > 0)
                    {
                        redisService.Set(CACHE_GROUP_PRODUCT_COSTCO, JsonConvert.SerializeObject(gr_list), db_index);
                    }

                    return Json(new { status = (int)ResponseType.SUCCESS, total_item_store = result_gr.total_item_store, data = await this.RenderViewToStringAsync("PartialView/" + partial_view.Replace("-", "/"), gr_list) });
                }
                else
                {
                    return Redirect("/Error/2");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "[FE] getProductByGroupId error: " + ex.ToString() + " group_product_id =" + group_product_id);
                return Json(new { status = (int)ResponseType.ERROR });
            }
        }

    }
}