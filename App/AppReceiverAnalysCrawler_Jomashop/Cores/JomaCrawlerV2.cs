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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace AppReceiverAnalysCrawler_Jomashop.Cores
{
    public class JomaCrawlerV2 : IJomaCrawler
    {
        ChromeDriver _chromeDriver;
        private readonly string current_path = Directory.GetCurrentDirectory();

        public Task<ProductViewModel> CrawlDetail(ChromeDriver driver, IConfiguration _configuration, QueueMessage record)
        {
            return null;
        }

        public async Task<CrawlMethodOutput> CrawlDetailV2(ChromeDriver driver, IConfiguration _configuration, QueueMessage record)
        {
            var result = new CrawlMethodOutput()
            {
                status = (int)MethodOutputStatusCode.Failed,
                message = "",
                product = null
            };
            var model = new ProductViewModel();
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
                result.message += "\nChrome Load Time: " + time_step_excute.ElapsedMilliseconds + " ms";
                time_step_excute.Restart();
                //-- Closed Ads Pop-up
                IWebElement ads_banner = null;
                if (LocalHelper.IsElementPresent(_chromeDriver, By.XPath(_configuration["Xpath:AdsBanner"]), out ads_banner))
                {
                    try
                    {
                        ads_banner.Click();
                        result.message += "\nClosed Ads Banner";
                        Thread.Sleep(1000);
                    }
                    catch { }
                }
                //-- Check if Page not found:
                ads_banner = null;
                if (LocalHelper.IsElementPresent(_chromeDriver, By.XPath(_configuration["Xpath:PageNotFound"]), out ads_banner))
                {
                    model.page_not_found = true;
                    result.message += "\nPage not found Detected";
                    return result;
                }
                //-- Check if Sold out:
                ads_banner = null;
                if (LocalHelper.IsElementPresent(_chromeDriver, By.XPath(_configuration["Xpath:PageNotFound"]), out ads_banner))
                {
                    result.message += "\nSold out";
                    model.page_not_found = true;
                }
                //---- Get Core Detail:
                //------Get-actual-product-path
                var cache = driver.Url.ToLower().Split(".html")[0].Split("/");
                var cache_product = "ProductInterface:" + cache[cache.Length - 1];
                int count = 0;
                string var1 = null;
                JObject local_storage = null;
                JomaCoreDetailV2 core_detail = null;
                IJavaScriptExecutor js = (IJavaScriptExecutor)_chromeDriver;
                //-- To ignore cache not present in first time:
                do
                {
                    try
                    {

                        //-- Get Local Storage:
                        var v1 = (String)js.ExecuteScript("return window.localStorage.getItem('apollo-cache-persist');");
                        if (v1 == null && v1.Trim() == "")
                        {
                            await Task.Delay(100);
                            count++;
                            continue;
                        }
                        //-- Parse
                        local_storage = JsonConvert.DeserializeObject<JObject>(v1);
                        //-- Get JSON
                        var1 = JsonConvert.SerializeObject(local_storage[cache_product]);
                        if (var1 == null)
                        {
                            local_storage = null;
                            var1 = null;
                            count++;
                            await Task.Delay(100);
                            continue;
                        }
                        core_detail = JsonConvert.DeserializeObject<JomaCoreDetailV2>(var1);
                        if (core_detail != null && core_detail.brand_name != null && core_detail.brand_name.Trim() != "")
                        {
                            break;
                        }
                        else
                        {
                            local_storage = null;
                            var1 = null;
                            core_detail = null;
                            count++;
                            await Task.Delay(100);
                        }
                    }
                    catch { await Task.Delay(100); }
                    
                } while (count < 15);
                if (core_detail == null || core_detail.brand_name == null || core_detail.brand_name.Trim() == "")
                {
                    result.message += "\nCannot Convert JSON Name \'" + cache_product + "\' from \'apollo-cache-persist\'";
                    return result;
                }
                //-- Get Detail:
                //---- Label:
                model.label_id = (int)LabelType.jomashop;
                model.label_name = LabelNameType.jomashop;
                //---- description:
                model.product_infomation = new List<string>();
                model.product_infomation.Add(core_detail.description.html);
                //---- image_thumb:
                model.image_thumb = LocalHelper.GetIMGLinkWithoutCache(core_detail.image.url);

                //---- image_product,image_size_product:
                model.image_product = new List<string>();
                model.image_size_product = new List<ImageSizeViewModel>();
                if (core_detail.media_gallery != null && core_detail.media_gallery.Count > 0)
                {
                    foreach (var gallery in core_detail.media_gallery)
                    {
                        model.image_product.Add(gallery.url_nocache);
                        model.image_size_product.Add(new ImageSizeViewModel()
                        {
                            Larger = gallery.url_nocache + "?width=546&height=546",
                            Thumb = gallery.url_nocache + "?width=70&height=70",
                        });
                    }
                }
                //---- Name:
                model.product_name = core_detail.name_wout_brand;
                //---- Product Code:
                model.product_code = core_detail.model_id;
                //------Change Product Code to full URL path + variation id (if have):
                // model.product_code = CommonHelper.GetProductCodeFromURLv2(record.url);

                //---- shiping_fee:
                model.shiping_fee = 0;
                try
                {
                    if (core_detail.is_shipping_free_message.ToLower().Contains("$"))
                    {
                        model.shiping_fee = Convert.ToDouble(core_detail.is_shipping_free_message.Replace("Shipped", "").Replace("$", ""));
                    }
                }
                catch { }

                //---- Price:
                model.price = Math.Round(core_detail.price_range.minimum_price.msrp_price.value + model.shiping_fee, 2);
                model.amount = 0;
                try
                {
                    if (LocalHelper.IsElementPresentByXpath(driver, _configuration["Xpath:Price"]))
                    {
                        var element = driver.FindElement(By.XPath(_configuration["Xpath:Price"]));
                        model.amount = Convert.ToDouble(element.GetAttribute("innerHTML").Trim().Replace("$", "").Replace(",", ""));
                        model.discount = Math.Round((model.price - model.amount) / model.price *100, 0);
                        model.product_save_price = Math.Round(model.price-model.amount, 2);

                    }
                }
                catch { }

                if (model.amount <= 0)
                {
                    model.amount = Math.Round(core_detail.price_range.minimum_price.final_price.value + model.shiping_fee, 2);
                    model.discount = Math.Round(core_detail.price_range.minimum_price.discount_on_msrp.percent_off, 0);
                    model.product_save_price = Math.Round(core_detail.price_range.minimum_price.discount_on_msrp.amount_off, 2);
                }

                model.rate = await GetRate(_configuration["API:rate"]);

                //---- USexpress Shipping fee:
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
                model.list_product_fee.price = Math.Round(model.amount, 2);
                model.list_product_fee.total_fee = Math.Round(Convert.ToDouble(product_fee_list["TOTAL_SHIPPING_FEE"]), 2);
                model.list_product_fee.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]) * model.rate, 0);
                model.amount_vnd = Math.Round(Convert.ToDouble(product_fee_list["PRICE_LAST"]) * model.rate, 0);
                model.price_vnd = Math.Round(model.price * model.rate, 0);

                //---- USexpress Link:
                model.link_product = CommonHelper.genLinkDetailProductv2(LabelType.jomashop.ToString(), record.url);
                //---- Review:
                model.star = core_detail.yotpo.average_score;
                model.reviews_count = core_detail.yotpo.reviews_count;
                //-- Variants:
                model.variation_name = new List<string>();
                model.list_variations = new List<VariationViewModel>();
                if (core_detail.variants != null && core_detail.variants.Count > 0)
                {
                    foreach (var variant in core_detail.variants)
                    {
                        string ref_name = variant.product.__ref;
                        var var2 = JsonConvert.SerializeObject(local_storage["" + ref_name]);
                        JomaV2Variant variant_detail = JsonConvert.DeserializeObject<JomaV2Variant>(var2);
                        model.variation_name.Add(variant_detail.name_wout_brand);
                        var variant_item = new VariationViewModel();
                        variant_item.asin = variant_detail.id.ToString();
                        var var3 = _chromeDriver.Url.Split("?product_id=");
                        if (var3 != null && var3.Length > 1)
                        {
                            string id_in_url = _chromeDriver.Url.Split("?product_id=")[1];
                            variant_item.selected = id_in_url == null || id_in_url.Trim() == "" || id_in_url.Trim() != variant_detail.id.ToString() ? false : true;
                        }
                        else
                        {
                            variant_item.selected = false;
                        }
                        DataObjectViewModel dim = new DataObjectViewModel();
                        dim["product_name"] = variant_detail.name_wout_brand;
                        dim["product_source_url"] = _chromeDriver.Url + "?product_id=" + variant_detail.id;
                        dim["in_stock"] = variant_detail.stock_status.Trim().ToUpper().Replace("_", " ");
                        dim["product_variation_code"] = variant_detail.model_id;
                        dim["price"] = variant_detail.msrp;
                        //---- shiping_fee:
                        dim["shiping_fee"] = 0;
                        try
                        {
                            if (core_detail.is_shipping_free_message.ToLower().Contains("$"))
                            {
                                dim["shiping_fee"] = Convert.ToDouble(variant_detail.is_shipping_free_message.Replace("Shipped", "").Replace("$", ""));
                            }
                            dim["amount"] = variant_detail.price_range.minimum_price.final_price.value + (double)dim["shiping_fee"];
                        }
                        catch
                        {
                            dim["amount"] = variant_detail.price_range.minimum_price.final_price.value;
                        }
                        variant_item.dimensions = dim;
                        model.list_variations.Add(variant_item);
                    }
                }
                //-- Other Information:
                model.create_date = DateTime.Now;
                model.is_prime_eligible = false;
                model.seller_id = "Jomashop";
                model.seller_name = core_detail.brand_name;
                model.in_stock = core_detail.stock_status.Replace("_", " ");
                model.product_type = ProductType.AUTO;
                model.is_crawl_weight = false;
                model.item_weight = "1 pound";
                model.update_last = DateTime.Now;
                model.regex_step = 2;
                model.product_status = (int)ProductStatus.NORMAL_SELL;

                //-- All Passed, return Success:
                result.status = (int)MethodOutputStatusCode.Success;
                result.message += "\nCompleted";
                result.product = model;
            }
            catch (Exception ex)
            {
                result.status = (int)MethodOutputStatusCode.ErrorOnExcution;
                result.message += "\n" + ex.ToString();
            }
            return result;
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
        private async Task<Dictionary<string, double>> GetUSExpressShippingFee(string url, string key, double price)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = url;

                string j_param = JsonConvert.SerializeObject(new
                {
                    price = price,
                    label_id = 7,
                    pound = 1,
                    unit = "pound"
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
    }
}
