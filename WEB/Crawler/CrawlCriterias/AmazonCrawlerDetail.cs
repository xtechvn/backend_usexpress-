using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crawler.Interfaces;
using Utilities.Contants;
using Utilities;

namespace Crawler.CrawlCriterias
{
    public class AmazonCrawlerDetail : ICrawlerByProduct
    {
        string product_code;
        int scrapy_by;
        string tele_token;
        string tele_group_id;
        public AmazonCrawlerDetail(string _product_code, int _scrapy_by, string _tele_token, string _tele_group_id)
        {
            product_code = _product_code;
            scrapy_by = _scrapy_by;
            tele_token = _tele_token;
            tele_group_id = _tele_group_id;
        }

        public ProductViewModel CrawlProductDetail()
        {
            try
            {


                var product_detail = new ProductViewModel();
                switch (scrapy_by)
                {
                    case (int)ScrapyType.CrawBy.QUEUE_RPC:

                        break;
                    case (int)ScrapyType.CrawBy.QUEUE_WORK:

                        break;
                    case (int)ScrapyType.CrawBy.HTTP_REQUEST:

                        break;
                    case (int)ScrapyType.CrawBy.SELELIUM:

                        break;
                    case (int)ScrapyType.CrawBy.FADO:

                        break;
                }

                return product_detail;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(tele_token, tele_group_id, "CrawlProductDetail Product_Code = "+ product_code + ", scrapy_by = "+ scrapy_by + " error: " + ex.ToString());
                return null;
            }
        }


    }
}
