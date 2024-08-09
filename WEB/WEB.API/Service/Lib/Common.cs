using Caching.RedisWorker;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Service.Lib
{
    public partial class Common
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn _redisService;
        public Common(IConfiguration _configuration, RedisConn RedisService)
        {
            configuration = _configuration;
            _redisService = RedisService;
        }

        public string crawlRateVCB()
        {
            double rate_default = Convert.ToDouble(configuration["rate:rate_default"]);
            double percent_sell = Convert.ToDouble(configuration["rate:percent_sell"]);
            string url_vietcombank = configuration["rate:url_rate_vietcombank"];

            try
            {
                var doc1 = new XmlDocument();
                doc1.Load(url_vietcombank);
                XmlElement root = doc1.DocumentElement;
                XmlNodeList nodes = root.SelectNodes("/ExrateList/Exrate ");

                foreach (XmlNode node in nodes)
                {
                    string CurrencyCode = node.Attributes["CurrencyCode"].InnerText;
                    if (CurrencyCode.Trim().ToUpper() == "USD")
                    {
                        double Sell = Convert.ToDouble(node.Attributes["Sell"].InnerText);
                        string rate_response = (Sell + (Sell * (percent_sell)) / 100).ToString();

                        return rate_response;
                    }
                }
                return ((rate_default + (rate_default * percent_sell) / 100).ToString());

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getRateCurrent==> error:  " + ex.Message);
                return ((rate_default + (rate_default * percent_sell) / 100).ToString());
            }
        }

        public double getRateCache()
        {
            double rate_default = Convert.ToDouble(configuration["rate:rate_default"]);
            double percent_sell = Convert.ToDouble(configuration["rate:percent_sell"]);

            try
            {
                string cache_name = CacheType.RATE;
                string rate = (rate_default + (rate_default * percent_sell) / 100).ToString();
                string data_rate = string.Empty;
                var rate_result = _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                if (!string.IsNullOrEmpty(rate_result.Result))
                {
                    data_rate = rate_result.Result.ToString();
                    var date_cate = Convert.ToDateTime(data_rate.Split("_").Last());
                    var current_date = DateTime.Now.Date;
                    if (DateTime.Compare(date_cate, current_date) != 0)
                    {
                        var lib = new Service.Lib.Common(configuration, _redisService);
                        rate = crawlRateVCB().ToString();

                        data_rate = rate + "_" + DateTime.Now.Date;
                        _redisService.Set(cache_name, data_rate, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    }
                    else
                    {
                        rate = data_rate.Split("_").First();
                    }
                }
                else
                {
                    var lib = new Service.Lib.Common(configuration, _redisService);
                    rate = crawlRateVCB().ToString();

                    data_rate = rate + "_" + DateTime.Now.Date;
                    _redisService.Set(cache_name, data_rate, Convert.ToInt32(configuration["Redis:Database:db_common"]));

                }

                return Convert.ToDouble(rate);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getRateCache==> error:  " + ex.Message);
                return Convert.ToDouble((rate_default + (rate_default * percent_sell) / 100));
            }
        }

    }
}
