using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ViewModels;
using Entities.ViewModels.Carts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;

namespace WEB.UI.Controllers.Product
{
    public partial class ProductGroup
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public ProductGroup(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
        }
        // get list sản phẩm từ cache
        public async Task<List<ProductViewModel>> getProductListByCacheName(int grp_id, int db_index, int skip, int take)
        {
            try
            {
                var group_product = new List<ProductViewModel>();
                string cache_name = CacheType.GROUP_PRODUCT + grp_id;
                var j_product_detail = await redisService.GetAsync(cache_name, db_index);

                if (!string.IsNullOrEmpty(j_product_detail))
                {
                    group_product = JsonConvert.DeserializeObject<List<ProductViewModel>>(j_product_detail);
                    group_product = group_product.Where(x => x.label_id > 0).ToList();
                    return group_product.Skip(skip).Take(take).ToList();
                }
                else
                {
                    // Đọc ra từ ES                    
                    string INDEX_ES_PRODUCT = configuration["DataBaseConfig:Elastic:index_product_search"];
                    string ES_HOST = configuration["DataBaseConfig:Elastic:Host"];
                    var ESRepository = new ESRepository<object>(ES_HOST);
                    var lst_grp_id = new List<int>();
                    lst_grp_id.Add(grp_id);

                    var result_product = await ESRepository.getProductListByGroupProductId(INDEX_ES_PRODUCT, lst_grp_id, skip, take);
                    if (result_product != null)
                    {
                        // set cache trong 15 phut sẽ tự động lấy lại từ ES
                        redisService.Set(cache_name, JsonConvert.SerializeObject(result_product.obj_lst_product_result), DateTime.Now.AddMinutes(15), db_index);
                        return result_product.obj_lst_product_result; // gán data từ es về
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getProductListByCacheName _msg error = " + ex.ToString());
                return null;
            }
        }

        // get list sp từ ES


    }
}
