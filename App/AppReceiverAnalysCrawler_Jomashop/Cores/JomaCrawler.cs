using AppCrawl_Joma_Receiver.Models;
using AppReceiverAnalysCrawler_Jomashop.Common;
using AppReceiverAnalysCrawler_Jomashop.Interfaces;
using AppReceiverAnalysCrawler_Jomashop.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Utilities;
using Utilities.Contants;

namespace AppReceiverAnalysCrawler_Jomashop.Cores
{
    public class JomaCrawler : IJomaCrawler
    {
        ChromeDriver _chromeDriver;
        private readonly string current_path = Directory.GetCurrentDirectory();
        public async Task<ProductViewModel> CrawlDetail(ChromeDriver driver, IConfiguration _configuration, QueueMessage record)
        {
            //CrawlResultMessage result = new CrawlResultMessage();
            var model = new ProductViewModel();
            model.page_not_found = true;
            model.product_code = null;
            model.product_name = null;
            string msg = null;
            try
            {
                Stopwatch time_step_excute = new Stopwatch();
                time_step_excute.Restart();
                //Chrome Driver:
                //ChromeDrvier truy cập URL
                _chromeDriver = driver;
                _chromeDriver.Navigate().GoToUrl(record.url);
                Thread.Sleep(Convert.ToInt32(_configuration["time_delay"]));
                time_step_excute.Stop();
                msg += "\nChrome Load Time: " + time_step_excute.ElapsedMilliseconds + " ms.";
                time_step_excute.Restart();
                //Get-actual-product-path
                var cache = driver.Url.ToLower().Split(".html")[0].Split("/");
                var product_path = cache[cache.Length - 1];
                //Closed Ads Pop-up
                if (LocalHelper.IsElementPresentByXpath(_chromeDriver, "//a[contains(@class,\"bx-close-link\") and@data-click=\"close\"]"))
                {
                    var close_button = _chromeDriver.FindElements(By.XPath("//a[contains(@class,\"bx-close-link\") and@data-click=\"close\"]"));
                    foreach (var e in close_button)
                    {
                        if (e.Displayed)
                        {
                            e.Click();
                            break;
                        }
                    }

                }
                //Check if Page not found:
                if (LocalHelper.IsElementPresentByXpath(_chromeDriver, "//div[contains(@class,\"text-404\")]"))
                {
                    msg += "\nPage Not Found.";
                }
                else
                {

                    // Get Local Storage:
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_chromeDriver;
                    var v1 = (String)js.ExecuteScript("return window.localStorage.getItem('apollo-cache-persist');");
                    //Thread.Sleep(Convert.ToInt32(_configuration["time_delay"]));
                    // Try again:
                    if (v1 == null || v1.Trim() == "")
                    {
                        Console.WriteLine("JSON get Null, try again 1 times. ");
                        _chromeDriver.Navigate().GoToUrl(record.url);
                        js = (IJavaScriptExecutor)_chromeDriver;
                        v1 = (String)js.ExecuteScript("return window.localStorage.getItem('apollo-cache-persist');");
                    }
                    if (v1 != null && v1.Trim() != "")
                    {
                        var local_storage = JsonConvert.DeserializeObject<JObject>(v1);

                        string str_json = "";
                        //-- Get Core Detail:
                        var var1 = JsonConvert.SerializeObject(local_storage["ProductInterface:" + product_path]);
                        JomaCoreDetailViewModel core_detail = JsonConvert.DeserializeObject<JomaCoreDetailViewModel>(var1);
                        str_json += "Core_Detail: " + var1;
                        //-- Get Final Price Detail:
                        // ProductInterface:gucci-ladies-interlocking-g-ace-sneakers-577145-a38v0-9062.price_range.minimum_price.final_price
                        var1 = JsonConvert.SerializeObject(local_storage["$ProductInterface:" + product_path + ".price_range.minimum_price.final_price"]);
                        JomaPriceViewModel final_price = JsonConvert.DeserializeObject<JomaPriceViewModel>(var1);
                        str_json += "final_price: " + var1;
                        //-- Get Price to Calucate Shipping fee:
                        // ProductInterface:gucci-ladies-interlocking-g-ace-sneakers-577145-a38v0-9062.price_range.minimum_price.final_price
                        var1 = JsonConvert.SerializeObject(local_storage["$ProductInterface:" + product_path + ".price_range.minimum_price.regular_price"]);
                        JomaPriceViewModel regular_price = JsonConvert.DeserializeObject<JomaPriceViewModel>(var1);
                        str_json += "final_price: " + var1;

                        //-- Discount:
                        //.minimum_price.discount_on_msrp
                        var1 = JsonConvert.SerializeObject(local_storage["$ProductInterface:" + product_path + ".price_range.minimum_price.discount_on_msrp"]);
                        JomaDiscountViewModel discount = JsonConvert.DeserializeObject<JomaDiscountViewModel>(var1);
                        str_json += "discount: " + var1;

                        //-- Thumbnail:
                        // $ProductInterface:gucci-ladies-interlocking-g-ace-sneakers-577145-a38v0-9062.image
                        var1 = JsonConvert.SerializeObject(local_storage["$ProductInterface:" + product_path + ".image"]);
                        JomaImageViewModel thumb = JsonConvert.DeserializeObject<JomaImageViewModel>(var1);
                        str_json += "thumb: " + var1;

                        //-- Review:
                        // $ProductInterface:gucci-ladies-interlocking-g-ace-sneakers-577145-a38v0-9062.yotpo
                        var1 = JsonConvert.SerializeObject(local_storage["$ProductInterface:" + product_path + ".yotpo"]);
                        JomaReviewerViewModel review = JsonConvert.DeserializeObject<JomaReviewerViewModel>(var1);
                        str_json += "review: " + var1;

                        if (core_detail == null || final_price == null || thumb == null || review == null)
                        {
                            msg += "\n JSON: " + str_json;
                            msg += "\n JsonConvert Data Return Null. ";
                            LogScreenshot(product_path);
                        }
                        else
                        {
                            //-- Bind Detail:
                            model.page_not_found = false;
                            model.product_map_id = -1;
                            model.product_code = core_detail.sku;
                            model.product_name = core_detail.name;
                            //-- Generate:
                            model.amount = 0;

                            //-- Get Price On show instead of get on json:
                            var web_element_price = driver.FindElementByXPath("//div[contains(@class,\"now-price\")]/span[not(@class)]");
                            if(web_element_price != null)
                            {
                                try
                                {
                                    string price_str = web_element_price.GetAttribute("innerHTML");
                                    if (price_str != null && price_str.Contains("$"))
                                    {
                                        model.amount = Convert.ToDouble(price_str.Trim().Replace(",", "").Replace("$", ""));
                                    }
                                } catch (Exception ex)
                                {
                                    Console.WriteLine("Parser Price From HTML with '" + web_element_price.GetAttribute("innerHTML") + "' error: " + ex.ToString());
                                }

                            }
                            //-- If parse Price: 
                            if (model.amount > 0)
                            {
                                if (regular_price.value < 100) model.shiping_fee = 5.99;
                                else model.shiping_fee = 0;
                                model.price = core_detail.msrp;
                                model.discount = Math.Round(((core_detail.msrp -model.amount) / core_detail.msrp)*100, 0);
                            }
                            else
                            {
                                if (regular_price.value < 100) model.shiping_fee = 5.99;
                                else model.shiping_fee = 0;
                                model.price = core_detail.msrp;
                                model.discount = discount.percent_off;
                                model.amount = final_price.value;
                                if (core_detail.price_promo_text != null && core_detail.price_promo_text.Trim().Contains("Sale") && core_detail.promotext_type != null && core_detail.promotext_type.Trim() != "")
                                {
                                    try
                                    {
                                        switch (core_detail.promotext_type.Trim())
                                        {
                                            case "percentage":
                                                {
                                                    var discount_percent = Convert.ToDouble(core_detail.promotext_value);
                                                    model.amount = Math.Round((model.amount * discount_percent / 100), 2) + model.shiping_fee;
                                                    model.amount_vnd = Math.Round(model.amount * model.rate, 0);
                                                    model.discount = Math.Round(100 - (model.amount / model.price * 100), 0);
                                                }
                                                break;
                                            default: break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Get Price by PromoCode error: " + ex.ToString());
                                    }
                                }
                            }
                            model.rate = await GetRate(_configuration["API:rate"]);
                            model.amount_vnd = Math.Round((model.amount+model.shiping_fee) * model.rate, 0);
                            model.image_thumb = GetIMGLinkWithoutCache(thumb.url);
                            model.label_id = 7;
                            model.label_name = LabelNameType.jomashop;
                           
                            //-- USexpress Shipping fee:
                            var product_fee_list = await GetUSExpressShippingFee(_configuration["API:ShippingFee"], _configuration["Key:API"], model.amount);
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
                            model.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"])* model.rate , 0);
                            model.list_product_fee.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]) * model.rate, 0);
                            model.product_save_price = model.price - model.amount;
                            //-- Image Product and Image Size Product
                            if (model.image_product == null) model.image_product = new List<string>();
                            if (model.image_size_product == null) model.image_size_product = new List<ImageSizeViewModel>();
                            for (int i = 0; i < 5; i++)
                            {
                                // ProductInterface:gucci-ladies-interlocking-g-ace-sneakers-577145-a38v0-9062.media_gallery.0.sizes.0
                                var1 = JsonConvert.SerializeObject(local_storage["ProductInterface:" + product_path + ".media_gallery." + i + ".sizes.0"]);
                                if (var1 == null || var1.Trim() == "" || var1.Trim().ToLower() == "null")
                                {
                                    break;
                                }
                                var img_obj = JsonConvert.DeserializeObject<JomaImageDetailViewModel>(var1);
                                var img_url = GetIMGLinkWithoutCache(img_obj.url).Split("?")[0];
                                model.image_product.Add(img_url + "?width=546&height=546");
                                model.image_size_product.Add(new ImageSizeViewModel()
                                {
                                    Thumb = img_url + "?width=70&height=70",
                                    Larger = img_url + "?width=546&height=546"
                                });
                            }
                            //-- Variant Name: 
                            if (model.variation_name == null) model.variation_name = new List<string>();
                            if (model.list_variations == null) model.list_variations = new List<VariationViewModel>();
                            if (core_detail.variants != null && core_detail.variants.Count > 0)
                                foreach (var variation in core_detail.variants)
                                {
                                    var1 = JsonConvert.SerializeObject(local_storage[variation.id]);
                                    JomaVariationLabelViewModel variation_detail = JsonConvert.DeserializeObject<JomaVariationLabelViewModel>(var1);
                                    var1 = JsonConvert.SerializeObject(local_storage[variation_detail.product.id]);
                                    var variations = JsonConvert.DeserializeObject<JomaVariantDetailViewModel>(var1);
                                    model.variation_name.Add(variations.name_wout_brand);
                                    var variant_item = new VariationViewModel();
                                    variant_item.asin = variations.id.ToString();
                                    var var2 = _chromeDriver.Url.Split("?product_id=");
                                    if (var2 != null && var2.Length > 1)
                                    {
                                        string id_in_url = _chromeDriver.Url.Split("?product_id=")[1];
                                        variant_item.selected = id_in_url == null || id_in_url.Trim() == "" || id_in_url.Trim() != variations.id.ToString() ? false : true;
                                    }
                                    else
                                    {
                                        variant_item.selected = false;
                                    }
                                    DataObjectViewModel dim = new DataObjectViewModel();
                                    dim["product_name"] = variations.name_wout_brand;
                                    dim["product_source_url"] = _chromeDriver.Url+ "?product_id=" + variations.id;
                                    dim["in_stock"] = variations.stock_status.Trim().ToUpper().Replace("_"," ");
                                    dim["product_variation_code"] = variations.model_id;
                                    dim["price"] = variations.msrp;
                                    var1 = JsonConvert.SerializeObject(local_storage["$SimpleProduct:"+variations.id+".price_range.minimum_price.final_price"]);
                                    var variations_final_price = JsonConvert.DeserializeObject<JomaPriceJSONModel>(var1);
                                    dim["amount"] = variations_final_price.value;
                                    dim["shiping_fee"] = variations_final_price.value<100 ? 5.99 :0 ;
                                    variant_item.dimensions = dim;
                                    model.list_variations.Add(variant_item);
                                }
                            model.create_date = DateTime.Now;
                            model.manufacturer = core_detail.manufacturer;
                            model.star = review.average_score;
                            model.reviews_count = review.reviews_count;

                            model.is_prime_eligible = false;
                            model.seller_id = "jomashop";
                            model.seller_name = core_detail.brand_name;
                            model.in_stock = core_detail.stock_status.Replace("_", " ");
                            model.link_product = CommonHelper.genLinkDetailProduct(LabelType.jomashop.ToString(), model.product_code, model.product_name);


                            model.product_type = ProductType.AUTO;
                            model.is_crawl_weight = false;
                            model.item_weight = "1 pound";
                            model.update_last = DateTime.Now;
                            model.product_ratings = review.reviews_count;
                            model.regex_step = 2;
                            // model.page_source_html = v1;
                            model.product_status = (int)ProductStatus.NORMAL_SELL;
                            //-- Product Specification:
                            model.product_specification = new Dictionary<string, string>();
                            

                        }
                    }
                    else
                    {
                        msg += "\nGet JSON From Local Storage Return Null. ";
                        LogScreenshot(product_path);
                    }
                }
                time_step_excute.Stop();
                msg += "\nParser Variable Time: " + time_step_excute.ElapsedMilliseconds + " ms.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //LogHelper.InsertLogTelegram("AppReceiverAnalysCrawler_Jomashop - CrawlDetail Error: " + ex.ToString());
                msg += "\n Error on excution: " + ex.ToString();
            }
            model.product_infomation_HTML = msg;
            //return result;
            return model;
        }
       
        private void LogScreenshot(string product_path)
        {
            var photo = _chromeDriver.GetScreenshot();
            string base_path = @Directory.GetCurrentDirectory() + @"\screenshot\";
            if (!Directory.Exists(base_path))
            {
                Directory.CreateDirectory(base_path);
            }
            string path = base_path + product_path.Trim().Replace(" ", "_") + "_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm") + ".png";
            photo.SaveAsFile(path);
            FileStream fsSource = new FileStream(path, FileMode.Open, FileAccess.Read);
            InputOnlineFile file = new InputOnlineFile(fsSource);
            LogHelper.InsertImageTelegramAsync(file,path);
        }
        private List<VariationViewModel> GetListVariation()
        {
            List<VariationViewModel> list = new List<VariationViewModel>();
            try
            {

            }
            catch (Exception)
            {

            }
            return list;
        }
        private async Task<double> GetRate(string api_url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = api_url;
                var result = await httpClient.GetAsync(apiPrefix);
                dynamic resultContent = result.Content.ReadAsStringAsync().Result;
                if (double.TryParse(resultContent, out double rateValue))
                {
                    return rateValue;
                }
            }
            catch (Exception)
            {

            }
            return 0;
        }
        private async Task<Dictionary<string,double>> GetUSExpressShippingFee(string url,string key,double price)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = url;

                string j_param = JsonConvert.SerializeObject(new {
                    price = price,
                    label_id = 7,
                    pound =1,
                    unit="pound"
                });
                string token = CommonHelper.Encode(j_param, key);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var rs_content = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.Content.ReadAsStringAsync().Result);
                if ((string)rs_content["status"] == ResponseType.SUCCESS.ToString())
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, double>>(JsonConvert.SerializeObject(rs_content["shipping_fee"]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetUSExpressShippingFee error" + ex.ToString());
            }
            return null;
        }
        public string GetIMGLinkWithoutCache(string img_url)
        {
            try
            {
                string new_url = "";
                if (img_url.Contains("cache/"))
                {
                    bool cache_id_after = false;
                    string[] url_pattern = img_url.Split("/");
                    for (int i = 0; i < url_pattern.Length; i++)
                    {
                        if (!cache_id_after)
                        {
                            if (url_pattern[i] == "cache")
                            {
                                cache_id_after = true;
                                continue;
                            }
                            else
                            {
                                new_url = new_url + url_pattern[i];
                                if (i < (url_pattern.Length - 1)) new_url = new_url + "/";
                            }
                        }
                        else
                        {
                            cache_id_after = false;
                            continue;
                        }
                    }
                    return new_url;
                }
            }
            catch (Exception)
            {

            }
            return img_url;
        }


    }
}
