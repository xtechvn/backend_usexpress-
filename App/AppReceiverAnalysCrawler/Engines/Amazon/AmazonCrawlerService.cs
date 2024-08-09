using AppReceiverAnalysCrawler.Common;
using AppReceiverAnalysCrawler.Interfaces;

using Crawler.ScraperLib.Amazon;
using CsQuery.Utility;
using Entities.Models;
using Entities.ViewModels;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;
using Utilities;
using Utilities.Contants;
using Utilities.UtilitiesNumb;

namespace AppReceiverAnalysCrawler.Engines.Amazon
{
    public class AmazonCrawlerService : IAmazonCrawlerService
    {
        string startupPath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\netcoreapp3.1\", @"\");

        public static string KEY_CONNECT_API_USEXPRESS = ConfigurationManager.AppSettings["KEY_CONNECT_API_USEXPRESS"];
        public static string URL_API_USEXPRESS = ConfigurationManager.AppSettings["API_USEXPRESS"];
        public ProductViewModel crawlerProductAmazon(ChromeDriver browers, string url_page, string asin, int group_product_id)
        {
            var amz_detail = new ProductViewModel();
            try
            {
                string page_source_html = browers.PageSource;
                // bool is_range_price = false;

                #region Kiếm tra trang sản phẩm có tồn tại sản phẩm không
                if (page_source_html.IndexOf("Page Not Found") >= 0)
                {
                    if (page_source_html.IndexOf("Dogs of Amazon") == -1)
                    {
                        // LogHelper.InsertLogTelegram("[Page Not Found] crawlerProductAmazon: asin: " + asin + ": khong tim thay san pham nay !");
                    }
                    amz_detail.page_not_found = true;
                    return amz_detail;
                }
                #endregion

                #region Nếu có Captcha thì ByPass Captcha
                if (ParserAmz.HaveCapCha(page_source_html))
                {
                    // vượt captcha: vuot xong page se tu dong chuyen ve trang detail
                    if (executeCaptcha(browers))
                    {
                        Thread.Sleep(1500);
                        page_source_html = browers.PageSource;
                    }
                    else
                    {
                        //LogHelper.InsertLogTelegram("crawlerProductAmazon error: analys captcha error");
                        return null;
                    }
                }
                #endregion

                //Lấy ra tỷ giá trong ngày
                double rate = CommonHelper.getRateCurrent(URL_API_USEXPRESS + "/api/ServicePublic/rate.json");

                // Regex các thành phần mặt trang--> Crawl qua app
                amz_detail = ParserAmz.RegexElementPage(page_source_html, asin, url_page, rate);

                // Regex các thành phần mặt trang--> Chuyển đổi cơ chế load API
                //amz_detail = this.RegexElementPage(page_source_html, asin, url_page); 

                // Tính phí mua hộ
                amz_detail.list_product_fee = getFeeForProduct(amz_detail.price + amz_detail.shiping_fee, amz_detail.item_weight, amz_detail.product_code, amz_detail.rate, amz_detail.shiping_fee);

                // Giá về tay
                amz_detail.amount_vnd = amz_detail.list_product_fee != null ? amz_detail.list_product_fee.amount_vnd : 0;

                // Giá giảm
                if (amz_detail.amount_vnd > 0)
                {
                    amz_detail.discount = Math.Round(((amz_detail.product_save_price / (amz_detail.price + amz_detail.product_save_price)) * 100));
                    amz_detail.price_vnd = Math.Round(amz_detail.amount_vnd / (1 - amz_detail.discount / 100));
                }

                amz_detail.group_product_id = group_product_id;
                return amz_detail;
            }
            catch (Exception ex)
            {
                // LogHelper.InsertLogTelegram("crawlerProductAmazon asin= " + asin + " error" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Crawl thêm sản phẩm. các phần tiếp theo như sellerlist, mô tả sản phẩm...
        /// </summary>
        /// <param name="browers"></param>
        /// <param name="amz_detail"></param>
        /// <param name="page_source"></param>
        /// <returns></returns>
        public ProductViewModel crawlProductMoreAmazon(ChromeDriver browers, ProductViewModel amz_detail, string page_source)
        {
            try
            {
                #region CRAWL tiếp phần cân nặng sau khi trang load ra xong

                // Weight
                bool is_crawl_weight;
                amz_detail.item_weight = ParserAmz.getItemWeight(page_source, amz_detail.product_code, out is_crawl_weight);
                amz_detail.is_crawl_weight = true; // Chỉ cho đọc lại 1 lần. Trường hợp ko ra thì tăng sleep

                // product fee
                amz_detail.list_product_fee = getFeeForProduct(amz_detail.price + amz_detail.shiping_fee, amz_detail.item_weight, amz_detail.product_code, amz_detail.rate, amz_detail.shiping_fee);

                // Tính giá về tay
                amz_detail.amount_vnd = amz_detail.list_product_fee != null ? amz_detail.list_product_fee.amount_vnd : 0;


                // Giá giảm
                if (amz_detail.amount_vnd > 0)
                {
                    amz_detail.price_vnd = (amz_detail.product_save_price * amz_detail.rate + amz_detail.amount_vnd);
                    amz_detail.discount = Math.Round(((amz_detail.product_save_price / (amz_detail.price + amz_detail.product_save_price)) * 100));
                }

                #endregion

                return amz_detail;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("crawlProductMoreAmazon error" + ex.ToString());
                return null;
            }
        }

        // By pass captcha using SE
        private bool executeCaptcha(ChromeDriver browers)
        {
            try
            {
                var link_image_captcha = browers.FindElementByXPath("/html/body/div/div[1]/div[3]/div/div/form/div[1]/div/div/div[1]/img").GetAttribute("src");

                var captcha_text = CommonHelper.convertImageToText(link_image_captcha);

                browers.FindElementByXPath("/html/body/div/div[1]/div[3]/div/div/form/div[1]/div/div/div[2]/input").SendKeys(captcha_text);
                //  browers.FindElementByXPath("/html/body/div/div[1]/div[3]/div/div/form/div[2]/div/span/span/button").Click();

                return true;
            }
            catch (Exception ex)
            {
                // LogHelper.InsertLogTelegram("checkCaptcha error" + ex.ToString());
                return false;
            }
        }
        public List<SellerListViewModel> getPriceBySellers(ChromeDriver browers, string page_source_html)
        {
            try
            {
                //triger                

                //if (in_stock.IndexOf("Available from these sellers") >= 0)
                //{

                //browers.FindElement(By.XPath("/html/body/div[2]/div[2]/div[8]/div[5]/div[4]/div[45]/div[2]/span/a")).Click();

                //TimeSpan interval = new TimeSpan(0, 0, 6);
                //var wait = new WebDriverWait(browers, interval);               

                //   browers.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(120);

                // get dom sellerlist
                string dom_sellerlist = string.Empty;
                //var el_sellerlist = browers.FindElement(By.Id(""));

                //var js = (IJavaScriptExecutor)browers;
                //js.ExecuteScript("$('#olp-upd-new .a-link-normal').click();");
                browers.FindElement(By.CssSelector("#olp-upd-new > span > a")).Click();
                //dom_sellerlist = el_sellerlist != null ? el_sellerlist.GetAttribute("innerHTML") : "";

                // Bóc dữ liệu từ DOM
                return ParserAmz.getPriceBySellerList(dom_sellerlist);
                //}
                //else
                //{
                //    // LogHelper.InsertLogTelegram("getPriceBySellers error: khong tim thay seller nao");
                //    return null;
                //}
            }
            catch (Exception ex)
            {
                // LogHelper.InsertLogTelegram("getPriceBySellers error" + ex.ToString());
                return null;
            }
        }

        public static ProductFeeViewModel getFeeForProduct(double price, string weight, string asin, double rate, double shiping_fee)
        {
            try
            {
                string[] weight_value = weight.Split(" ");
                double round_weight = Convert.ToDouble(weight_value[0]);

                var param_push = new Dictionary<string, string>
                {
                    { "label_id",((int)LabelType.amazon).ToString()},
                    { "price",price.ToString() },
                    { "pound",Utilities.CommonHelper.convertToPound(round_weight, weight_value[1]).ToString() },
                    { "unit",weight_value[1].ToString()},
                    { "industry_special","-1"} // chua detect nganh hang voi
                };

                string token = CommonHelper.Encode(JsonConvert.SerializeObject(param_push), KEY_CONNECT_API_USEXPRESS);
                var data = new RequestData(token, URL_API_USEXPRESS + "/api/ServicePublic/tracking-shippingfee.json");
                var response_api = data.CreateHttpRequest();

                var json_data = JArray.Parse("[" + response_api + "]");
                string status = json_data[0]["status"].ToString();
                if (status == ResponseType.SUCCESS.ToString())
                {
                    string response_fee = json_data[0]["shipping_fee"].ToString();
                    var list_fee = JsonConvert.DeserializeObject<Dictionary<string, double>>(response_fee);

                    var product_fee = new ProductFeeViewModel
                    {
                        label_name = LabelType.amazon.ToString(),
                        price = price,
                        amount_vnd = price == 0 ? 0 : Convert.ToDouble(list_fee[FeeBuyType.PRICE_LAST.ToString()]) * rate,
                        list_product_fee = list_fee,
                        shiping_fee = shiping_fee
                    };
                    return product_fee;
                }
                else
                {
                    string response_error_api = json_data[0]["msg"].ToString();
                    //LogHelper.InsertLogTelegram("getFeeForProduct asin= " + asin + " error response api" + response_error_api);
                    return null;
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("getFeeForProduct asin= " + asin + " error" + ex.ToString());
                return null;
            }
        }
        private ProductViewModel RegexElementPage(string page_source_html, string asin, string url_page)
        {
            try
            {
                if (string.IsNullOrEmpty(page_source_html) && string.IsNullOrEmpty(asin))
                {
                    LogHelper.InsertLogTelegram("[App_crawl_amz_detail] RegexElementPage error with param empty");
                    return null;
                }
                var param_push = new Dictionary<string, string>
                {
                    { "page_source_html",page_source_html},
                    { "asin",asin },
                    { "url_page",url_page }
                };

                string token = CommonHelper.Encode(JsonConvert.SerializeObject(param_push), KEY_CONNECT_API_USEXPRESS);
                var data = new RequestData(token, URL_API_USEXPRESS + "/api/AmzProduct/detail.json");
                var response_api = data.CreateHttpRequest();

                var json_data = JArray.Parse("[" + response_api + "]");
                string status = json_data[0]["status"].ToString();
                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string product_detail = json_data[0]["data"].ToString();

                    return JsonConvert.DeserializeObject<ProductViewModel>(product_detail);
                }
                else
                {
                    LogHelper.InsertLogTelegram("[BOT]-RegexElementPage response error from api  with: response_api = " + response_api + " --- token= " + token);
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegexElementPage error with asin= " + asin + " error" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// price: giá sản phẩm trên mặt trang
        /// weight: trọng lượng sản phẩm
        /// </summary>
        /// <param name="price"></param>
        /// <param name="weight"></param>

        /// <returns></returns>


        //public double calculatorAmountLast(double rate, Dictionary<string, double> product_fee, string asin)
        //{
        //    try
        //    {
        //        double price = Convert.ToDouble(product_fee[FeeBuyType.PRICE_LAST.ToString()]);
        //        return price * rate;
        //    }
        //    catch (Exception ex)
        //    {

        //        LogHelper.InsertLogTelegram("calculatorAmountLast error with asin= " + asin + " error" + ex.ToString());
        //        return 0;
        //    }
        //}
    }
}
