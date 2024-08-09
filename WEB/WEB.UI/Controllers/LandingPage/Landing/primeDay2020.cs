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

namespace WEB.UI.Controllers.LandingPage
{
    public partial class primeDay2020
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public string FOLDER_KEY { get; set; }
        public primeDay2020(IConfiguration _configuration, RedisConn _redisService, string _FOLDER_KEY)
        {
            configuration = _configuration;
            redisService = _redisService;
            FOLDER_KEY = _FOLDER_KEY;
        }

        public async Task<List<ProductViewModel>> getProductByCategoryId()
        {
            try
            {
                var group_product = new List<ProductViewModel>();
                var j_product_detail = await redisService.GetAsync(FOLDER_KEY, Convert.ToInt32(configuration["Redis:Database:db_folder"]));

                if (!string.IsNullOrEmpty(j_product_detail))
                {
                    group_product = JsonConvert.DeserializeObject<List<ProductViewModel>>(j_product_detail);
                    return group_product;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getProductByCategoryId _msg error = Chưa có dữ liệu nào được nạp vào cache " + FOLDER_KEY);
                }
                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getProductByCategoryId _msg error = " + ex.ToString());
                return null;
            }
        }
    }
}
