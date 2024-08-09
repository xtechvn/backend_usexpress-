using App_AutomaticPurchase_AMZ.Lib;
using App_AutomaticPurchase_AMZ.Model;
using App_AutomaticPurchase_AMZ.Repositories;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace App_AutomaticPurchase_AMZ
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IAutoPurchaseAmz _autoPurchase;
        private readonly IUsExAPI _usExAPI;
        private readonly IUSExOldAPI _uSExOldAPI;
        private readonly IConfiguration _configuration;
        private ChromeDriver _driver;
        private string app_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\net6.0", "");

        private string user_name = null, password = null;
        private USOLDToken token = null;
        private string buy_log = "";
        public Worker(ILogger<Worker> logger, IAutoPurchaseAmz autoPurchase, IUsExAPI usExAPI, IUSExOldAPI uSExOldAPI, IConfiguration configuration)
        {
            _logger = logger;
            _autoPurchase = autoPurchase;
            _usExAPI = usExAPI;
            _uSExOldAPI = uSExOldAPI;
            _configuration = configuration;
        }
        #region Worker Base:
        //-- Worker Base:
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            user_name = _configuration["Login:Username"];
            password = _configuration["Login:Password"];
            string token_url = _configuration["API_OLD:Domain"] + _configuration["API_OLD:API_GetToken"];
            var gettoken = _uSExOldAPI.GetToken(token_url, _configuration["API_OLD:User_name"], _configuration["API_OLD:Password"]).Result;
            if (gettoken.status_code == (int)MethodOutputStatusCode.Success)
            {
                token = new USOLDToken()
                {
                    token = gettoken.message,
                    exprire_date = DateTime.UtcNow.AddDays(7)
                };
                _logger.LogInformation("OLD API Token: {0} ", token.token);
            }
            else
            {
                _logger.LogInformation("Cannot Get Token From OLD API ...");
            }

            var is_logged = ChromeInitilization().Result;
            if (is_logged)
            {
                return base.StartAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Closing Service ...");
                return base.StopAsync(cancellationToken);
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("Amazon Logged Account: " + user_name);
            while (!stoppingToken.IsCancellationRequested)
            {
                //-- Excuting Service
                await ExcuteService(stoppingToken);
                //await ExcuteTest(stoppingToken);
                _logger.LogInformation("Excute Completed: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ". Wait 30 seconds.");
                await Task.Delay(30 * 1000, stoppingToken);
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
        #endregion
        /// <summary>
        /// Test
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ExcuteTest(CancellationToken cancellationToken)
        {
            List<AutomaticPurchaseAmz> carts_new = new List<AutomaticPurchaseAmz>();
            var item = new AutomaticPurchaseAmz()
            {
                Amount = 11.05,
                CreateDate = DateTime.Now,
                OrderCode = "UAM-2D08086",
                OrderMappingId = "16644",
                PurchaseUrl = "https://www.amazon.com/dp/0875421229?smid=&psc=1",
                ProductCode = "0875421229",
                Quanity = 1,
                UpdateLast = DateTime.Now,
                PurchaseStatus = 0,
                OrderId = 18536,
                Id = 29

            };
            var autopurchase_items = new MethodOutput();
            autopurchase_items.status_code = (int)MethodOutputStatusCode.Success;
            autopurchase_items.data = JsonConvert.SerializeObject(carts_new);
            var final_item = item;
            var direct_buy_item = final_item;
            var direct_buy_success = true;
            final_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseSuccess;
            final_item.OrderDetailUrl = "https://www.amazon.com/gp/your-account/order-details/ref=ppx_yo_dt_b_order_details_o00?ie=UTF8&orderID=112-3606236-0855434";
            final_item.OrderedSuccessUrl = "https://www.amazon.com/gp/your-account/order-details/ref=ppx_yo_dt_b_order_details_o00?ie=UTF8&orderID=112-3606236-0855434";
            final_item.PurchaseMessage = "App_AutomaticPurchase_AMZ - Order Success";
            final_item.ManualNote = "App_AutomaticPurchase_AMZ - Order Success";
            final_item.UpdateLast = DateTime.Now;
            //await SendEmailAutomaticBuyExcuteComplete(direct_buy_item, final_item, false, direct_buy_success);

        }
        /// <summary>
        /// Main Program
        /// </summary>
        /// <returns></returns>
        private async Task ExcuteService(CancellationToken cancellationToken)
        {
            try
            {
                string get_list_url = _configuration["API_OLD:Domain"].Trim() + _configuration["API_OLD:API_GetCart"].Trim();
                string update_toNew_URL = _configuration["API_NEW:Domain"].Trim() + _configuration["API_NEW:AddNewItem"].Trim();
                string get_newdb_buy_list_URL = _configuration["API_NEW:Domain"].Trim() + _configuration["API_NEW:API_Get_AutoBuyList"].Trim();
                string offer_url = _configuration["Amazon:Product_OfferListingURL"].Trim();

                 var autopurchase_items = await _uSExOldAPI.GetAmazonCart(update_toNew_URL,get_list_url,token);

                if (autopurchase_items.status_code == (int)MethodOutputStatusCode.Success)
                {
                    List<AutomaticPurchaseAmz> excute_list = JsonConvert.DeserializeObject<List<AutomaticPurchaseAmz>>(autopurchase_items.data);
                    if (excute_list != null && excute_list.Count > 0)
                    {
                        _logger.LogInformation("Amazon Item Count: " + excute_list.Count);
                        _logger.LogInformation("OrderID Buy: " + JsonConvert.SerializeObject(excute_list.Select(x => x.Id)));

                        foreach (var item in excute_list)
                        {
                            
                            //-- If wrong item, skip:
                            if (item.PurchaseStatus != (int)AutomaticPurchaseStatus.New && item.PurchaseStatus != (int)AutomaticPurchaseStatus.ErrorOnExcution)
                            {
                                _logger.LogInformation("Skip PurchaseID: " + item.Id + ". PurchaseStatus: " + item.PurchaseStatus);
                                FileHelper.WriteLogToFile("Skip PurchaseID: " + item.Id + ". PurchaseStatus: " + item.PurchaseStatus, item.OrderCode, item.ProductCode);
                                LogHelper.InsertLogTelegram("Skip PurchaseID: " + item.Id + ". PurchaseStatus: " + item.PurchaseStatus+" - "+ item.OrderCode + " - " + item.ProductCode);
                                  continue;
                               
                            }
                            /*
                            //-- Check if already Excuted or File not found (Local):
                            if (FileHelper.CheckIfExcuted(item.OrderCode, item.ProductCode, item.Amount,item.Quanity) == 0 || FileHelper.CheckIfExcuted(item.OrderCode, item.ProductCode, item.Amount, item.Quanity)==2)
                            {
                                LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - Already Excuted Order (Local File): " + JsonConvert.SerializeObject(item));
                                //  continue;
                                //-- Stop Service to ignore duplicate buy:
                                await StopAsync(cancellationToken);
                                return;
                            }
                            else
                            {
                                FileHelper.WriteOrderExcutedToFile(item.OrderCode, item.ProductCode, item.Amount, item.Quanity);
                            }*/
                            FileHelper.WriteOrderExcutedToFile(item.OrderCode, item.ProductCode, item.Amount, item.Quanity);

                            //-- Check if already Excuted or File not found (DB NEW):
                            var check_indbnew = await _usExAPI.CheckIfPurchased(item, _configuration["API_NEW:Domain"].Trim() + _configuration["API_NEW:CheckIfPurchased"].Trim(),64, _configuration["API_NEW:API_Key"].Trim());
                            _logger.LogInformation(check_indbnew.message);
                            FileHelper.WriteLogToFile(check_indbnew.message, item.OrderCode, item.ProductCode);
                            if (check_indbnew.status_code == (int)MethodOutputStatusCode.Success)
                            {
                                FileHelper.WriteOrderExcutedToFile(item.OrderCode, item.ProductCode, item.Amount, item.Quanity);
                                LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - CheckIfPurchased: " + check_indbnew.message);
                                //  continue;
                                //-- Stop Service to ignore duplicate buy:
                                await StopAsync(cancellationToken);
                                return;
                            }
                            //-- Recorrect:
                            item.PurchaseUrl = item.PurchaseUrl.Trim().Replace(item.ProductCode.Trim(), item.ProductCode.Trim().ToUpper());
                            item.ProductCode = item.ProductCode.ToUpper();
                            FileHelper.WriteLogToFile("Recorrect Product", item.OrderCode, item.ProductCode);

                            //-- Detect if Buy Direct Success:
                            bool direct_buy_success = false;
                           
                            //-- Automatic Buy Phase 1, Direct Buy:
                            AutomaticPurchaseAmz direct_buy_item = await AutomaticBuyItem(item);
                            AutomaticPurchaseAmz final_item = direct_buy_item;
                            //-- Check if buy success
                            switch (direct_buy_item.PurchaseStatus)
                            {
                                case (int)AutomaticPurchaseStatus.PurchaseSuccess:
                                    {
                                        _logger.LogInformation("Purchased Success with URL: " + item.PurchaseUrl);
                                        _logger.LogInformation(item.PurchaseMessage);
                                        direct_buy_success = true;
                                    }
                                    break;
                                default:
                                    {
                                        _logger.LogInformation("Error On Excution while buy URL: " + direct_buy_item.PurchaseUrl);
                                        _logger.LogInformation(direct_buy_item.PurchaseMessage);
                                    }
                                    break;
                            }

                            //-- If cannot Buy Directly, select from OfferListing
                            if (!direct_buy_success)
                            {
                                var offer_url_item = offer_url.Replace("(&product_code&)", item.ProductCode.Trim());
                                _logger.LogInformation("Buy from Offer Listing - URL: " + offer_url_item);
                                var offer_item = await AutomaticBuyItemFromOfferListing(item, offer_url_item);
                                final_item = offer_item;
                                switch (final_item.PurchaseStatus)
                                {
                                    case (int)AutomaticPurchaseStatus.PurchaseSuccess:
                                        {
                                            _logger.LogInformation("Purchased  from Offer Listing Success with URL: " + final_item.PurchaseUrl);
                                            _logger.LogInformation(final_item.PurchaseMessage);
                                        }
                                        break;
                                    default:
                                        {
                                            _logger.LogInformation(" from Offer Listing FAILED with URL: " + final_item.PurchaseUrl);
                                            _logger.LogInformation(final_item.PurchaseMessage);
                                            final_item.PurchaseMessage = direct_buy_item.PurchaseMessage + offer_item.PurchaseMessage;
                                            //-- Log Telegram:
                                            LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - AutomaticBuyItemFromOfferListing with AutoPurchaseID: " + final_item.Id + "  Purchase URL: " + final_item.PurchaseUrl + "  \nError" + final_item.PurchaseMessage);

                                        }
                                        break;
                                }
                            }

                            
                            //-- Update DB Old:
                            string update_old_url = _configuration["API_OLD:Domain"] + _configuration["API_OLD:API_UpdateCart"];
                            string api_old_key = _configuration["API_OLD:API_Key"];
                            CheckToken();
                            var update_db_old = await _uSExOldAPI.UpdateAmazonCart(final_item, token, update_old_url, api_old_key);
                            _logger.LogInformation(update_db_old.message);

                            //-- Update DB + History:
                            string update_new_url = _configuration["API_NEW:Domain"] + _configuration["API_NEW:API_UpdatePurchaseDetail"];
                            string api_new_key = _configuration["API_NEW:API_Key"];
                            int user_excution = Convert.ToInt32(_configuration["Login:UserExcution"]);
                            var update_db_new = await _usExAPI.UpdatePurchaseDetail(final_item, update_new_url, final_item.PurchaseMessage, user_excution, api_new_key);
                            _logger.LogInformation(update_db_new.message);
                            if (update_db_new.status_code != (int)MethodOutputStatusCode.Success)
                            {
                                LogHelper.InsertLogTelegram("AutomaticBuyItem - UpdatePurchaseDetail with AutoPurchaseID: " + final_item.Id + "  Purchase URL: " + final_item.PurchaseUrl + "  \nError" + update_db_new.message);
                            }
                            /*
                            //-- Send Email To Customer Care if Buy Failed:
                               await SendEmailAutomaticBuyExcuteComplete(direct_buy_item, final_item, false, direct_buy_success);
                            */
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Cannot convert Data or Data Null: \n" + autopurchase_items.message);
                    }
                }
                else
                {
                    _logger.LogInformation("Cannot Get Auto-Purchase Item: " + autopurchase_items.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error While Excute: " + ex.ToString());
            }
            return;
        }
        /// <summary>
        /// Mua tự động item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<AutomaticPurchaseAmz> AutomaticBuyItem(AutomaticPurchaseAmz item)
        {
            AutomaticPurchaseAmz updated_item = item;
            FileHelper.WriteLogToFile("AutomaticBuyItem", item.OrderCode, item.ProductCode);
            try
            {
                //-- Screenshot List:
                Dictionary<string, string> screenshot_autobuy = new Dictionary<string, string>();
                string screenshot_item = null;

                //--Go to URL:
                _driver.Navigate().GoToUrl(item.PurchaseUrl);
                await Task.Delay(3 * 1000);
                FileHelper.WriteLogToFile("Go to URL "+item.PurchaseUrl, item.OrderCode, item.ProductCode);

                //-- Take Screenshot:
                screenshot_item = await TakeScreenshot(updated_item.OrderCode, "Product Detail Page");
                if (screenshot_item != null)
                {
                    screenshot_autobuy.Add("Product Detail Page", screenshot_item);
                }

                //--  Check IF ProductAvailable:
                var product_available = _autoPurchase.CheckProductAvailable(_driver);
                _logger.LogInformation(product_available.message);
                FileHelper.WriteLogToFile(product_available.message, item.OrderCode, item.ProductCode);

                //---- If out of allow price range, Buy From OfferListing
                if (product_available.status_code != (int)MethodOutputStatusCode.Success)
                {
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseFailure;
                    updated_item.PurchaseMessage = product_available.message;
                    updated_item.UpdateLast = DateTime.Now;
                    return updated_item;
                }

                //-- Switch to New arcodition instead of monthly delivering
                IConfigurationSection myArraySection = _configuration.GetSection("Xpath:BuyNewOption");
                var itemArray = myArraySection.AsEnumerable();
                var list = itemArray.Where(x => x.Value != null && x.Value.Trim() != "").Select(item => item.Value).ToList();
                IWebElement buy_new_option = null;
                var new_arcodition_output = _autoPurchase.CheckBuyNewOption(_driver, list, out buy_new_option);
                FileHelper.WriteLogToFile(new_arcodition_output.message, item.OrderCode, item.ProductCode);
                if (new_arcodition_output.status_code == (int)MethodOutputStatusCode.Success && buy_new_option != null)
                {
                    _logger.LogInformation(new_arcodition_output.message);
                    ClickElementAndWait(buy_new_option, 2);
                }

                //-- Check AddtoCart Button:
                myArraySection = _configuration.GetSection("Xpath:AddToCart");
                itemArray = myArraySection.AsEnumerable();
                list = itemArray.Where(x => x.Value != null && x.Value.Trim() != "").Select(item => item.Value).ToList();
                IWebElement add_to_cart_btn = null;
                var addtocart_output = _autoPurchase.CheckAddToCartButtonAvailable(_driver, list, out add_to_cart_btn);
                _logger.LogInformation(addtocart_output.message);
                FileHelper.WriteLogToFile(addtocart_output.message, item.OrderCode, item.ProductCode);
                if (addtocart_output.status_code == (int)MethodOutputStatusCode.Success && add_to_cart_btn != null)
                {
                }
                else
                {
                    //---- Btn not found, return to buy from offerlisting:
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.ErrorOnExcution;
                    updated_item.PurchaseMessage = ((string)addtocart_output.data) == null ? addtocart_output.message : (string)addtocart_output.data;
                    updated_item.UpdateLast = DateTime.Now;
                    return updated_item;
                }

                //-- Check Seller:
                list = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(_configuration["Xpath:AddToCart"]));
                var checkseller_output = _autoPurchase.CheckSeller(_driver, item.ProductCode);
                _logger.LogInformation(checkseller_output.message);
                FileHelper.WriteLogToFile(checkseller_output.message, item.OrderCode, item.ProductCode);

                if (checkseller_output.status_code == (int)MethodOutputStatusCode.Success)
                {
                    if (item.PurchasedSellerName == null || item.PurchasedSellerName.Trim() == "" || (item.PurchasedSellerName != null && item.PurchasedSellerName.Trim() == (string?)checkseller_output.data))
                    {
                        _logger.LogInformation("Check SellerID - Correct SellerID.");

                    }
                    else
                    {
                        //-- Seller Not Correct,log 
                        _logger.LogInformation("Check SellerID - SellerID not correct - Current Buy from SellerID: {0}", (string?)checkseller_output.data);
                        item.ManualNote += "SellerID-" + (string?)checkseller_output.data;
                    }
                }

                //-- Check Deal and Discount, click if available (Old Function):
                Dictionary<string, string> checkdeal_output_dictionary = new Dictionary<string, string>();
                checkdeal_output_dictionary.Add("Coupon_1", _configuration["Xpath:DealAndCoupon:Coupon_1"]);
                var checkdeal_output = _autoPurchase.CheckDealOrDiscountAvailable(_driver, checkdeal_output_dictionary);
                _logger.LogInformation(checkdeal_output.message);
                FileHelper.WriteLogToFile(checkdeal_output.message, item.OrderCode, item.ProductCode);

                //-- Select Quanity (Old Function, still working):

                var checkquanity_output = _autoPurchase.SelectQuanity(_driver, item.Quanity, list);
                _logger.LogInformation(checkquanity_output.message);
                FileHelper.WriteLogToFile(checkquanity_output.message, item.OrderCode, item.ProductCode);

                if (checkquanity_output.status_code != (int)MethodOutputStatusCode.Success)
                {
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseFailure;
                    updated_item.PurchaseMessage = ((string)addtocart_output.data) == null ? addtocart_output.message : (string)addtocart_output.data;
                    updated_item.UpdateLast = DateTime.Now;
                    return updated_item;
                }

                //--  Check Price:
                myArraySection = _configuration.GetSection("Xpath:Price");
                itemArray = myArraySection.AsEnumerable();
                List<string> price_xpath = itemArray.Where(x => x.Value != null && x.Value.Trim() != "").Select(item => item.Value).ToList();
                Dictionary<string, string> shipping_fee_xpath = new Dictionary<string, string>();
                checkdeal_output_dictionary.Add("Xpath", _configuration["Xpath:Shipping_fee:Xpath"]);
                checkdeal_output_dictionary.Add("Field", _configuration["Xpath:Shipping_fee:Field"]);
                var checkprice_output = _autoPurchase.CheckPrice(_driver, price_xpath, item.Amount, shipping_fee_xpath);
                _logger.LogInformation(checkprice_output.message);
                FileHelper.WriteLogToFile(checkprice_output.message, item.OrderCode, item.ProductCode);

                //---- If out of allow price range, Buy From OfferListing
                if (checkprice_output.status_code != (int)MethodOutputStatusCode.Success)
                {
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseFailure;
                    updated_item.PurchaseMessage = ((string)addtocart_output.data) == null ? addtocart_output.message : (string)addtocart_output.data;
                    updated_item.UpdateLast = DateTime.Now;
                    return updated_item;
                }

                //-- Click Add To Cart Button:
                _logger.LogInformation("Click Add To Cart Button");
                ClickElementAndWait(add_to_cart_btn, 2);
                FileHelper.WriteLogToFile("Click Add To Cart Button", item.OrderCode, item.ProductCode);

                //-- Check and Close Popup After Click Add To Cart:
                List<string> xpath_skip_btn = new List<string>();
                xpath_skip_btn.Add(_configuration["Xpath:AddedTocartNoAdditionalButton"]);
                var closepopup_1 = _autoPurchase.CloseAdsPopupAfterClickAddToCart(_driver, xpath_skip_btn);
                _logger.LogInformation(closepopup_1.message);
                FileHelper.WriteLogToFile(closepopup_1.message, item.OrderCode, item.ProductCode);

                //-- Take Screenshot:
                screenshot_item = await TakeScreenshot(updated_item.OrderCode, updated_item.ProductCode + " Added To Cart");
                if (screenshot_item != null)
                {
                    screenshot_autobuy.Add(updated_item.ProductCode + " Added To Cart", screenshot_item);
                }
                //-- Checkout:
                updated_item.Screenshot = JsonConvert.SerializeObject(screenshot_autobuy);
                updated_item.ManualNote = "AutomaticBuyItem - Order Success";
                updated_item = await AutomaticBuyItem_Checkout(updated_item, screenshot_autobuy);

            }
            catch (Exception ex)
            {
                _logger.LogInformation("AutomaticBuyItem: " + JsonConvert.SerializeObject(item) + "\nError: \n" + ex.ToString());
                updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.ErrorOnExcution;
                updated_item.PurchaseMessage = ex.ToString();
                updated_item.UpdateLast = DateTime.Now;
            }

            //-- Return:
            return updated_item;
        }
        /// <summary>
        /// Check Offer Listing và buy item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task<AutomaticPurchaseAmz> AutomaticBuyItemFromOfferListing(AutomaticPurchaseAmz item, string offer_url)
        {
            AutomaticPurchaseAmz updated_item = item;
            Dictionary<string, string> screenshot_offers = new Dictionary<string, string>();
            string screenshot_item = null;
            try
            {
                //--Go to URL:
                _driver.Navigate().GoToUrl(offer_url);
                await Task.Delay(3 * 1000);

                //-- Take Screenshot OfferListing:
                screenshot_item = await TakeScreenshot(updated_item.OrderCode, updated_item.ProductCode + "Offer Listings");
                if (screenshot_item != null)
                {
                    screenshot_offers.Add(updated_item.ProductCode + "Offer Listings", screenshot_item);
                }

                //-- Gen Xpath:
                Dictionary<string, string> offers_dictionary = new Dictionary<string, string>();
                offers_dictionary.Add("PinnedOffer", _configuration["Xpath:OffersListing:PinnedOffer"]);
                offers_dictionary.Add("OtherOfferList", _configuration["Xpath:OffersListing:OtherOfferList"]);
                offers_dictionary.Add("OfferHeadingText", _configuration["Xpath:OffersListing:OfferHeadingText"]);
                offers_dictionary.Add("OfferSeller", _configuration["Xpath:OffersListing:OfferSeller"]);
                offers_dictionary.Add("OfferISPrimeDetect", _configuration["Xpath:OffersListing:OfferISPrimeDetect"]);
                offers_dictionary.Add("OfferCheckPrice", _configuration["Xpath:OffersListing:OfferCheckPrice"]);
                offers_dictionary.Add("OfferAddToCart", _configuration["Xpath:OffersListing:OfferAddToCart"]);
                offers_dictionary.Add("OfferListingOnPage", _configuration["Xpath:OffersListing:OfferListingOnPage"]);
                offers_dictionary.Add("OfferCheckShippingFee", _configuration["Xpath:OffersListing:OfferCheckShippingFee"]);

                bool added_to_cart = false;

                //-- Check OfferListing
                //---- Scroll to last item in offer to take more item:
                var load_more_offers = _autoPurchase.LoadMoreOffers(_driver, offers_dictionary["OtherOfferList"]);
                FileHelper.WriteLogToFile(load_more_offers.message, item.OrderCode, item.ProductCode);
                _logger.LogInformation(load_more_offers.message);

                //---- Check Pinned Offer:
                var check_offer = _autoPurchase.CheckOfferListing(_driver, offers_dictionary["PinnedOffer"]);
                _logger.LogInformation(check_offer.message);
                FileHelper.WriteLogToFile(check_offer.message, item.OrderCode, item.ProductCode);

                if (check_offer.status_code == (int)MethodOutputStatusCode.Success)
                {
                    //------ Pinned Offer Found, Check offer detail:
                    var pinned_offer_element = _driver.FindElement(By.XPath(offers_dictionary["PinnedOffer"]));
                    var pinned_offer = _autoPurchase.OfferListing_OfferCheck(_driver, pinned_offer_element, offers_dictionary, item);
                    _logger.LogInformation(pinned_offer.message);
                    FileHelper.WriteLogToFile(pinned_offer.message, item.OrderCode, item.ProductCode);

                    if (pinned_offer.status_code == (int)MethodOutputStatusCode.Success)
                    {
                        //-------- In range, Add To Cart:
                        var add_to_cart_offer_pinned = _autoPurchase.OfferListing_AddToCart(_driver, pinned_offer_element, offers_dictionary);
                        _logger.LogInformation(add_to_cart_offer_pinned.message);
                        FileHelper.WriteLogToFile(add_to_cart_offer_pinned.message, item.OrderCode, item.ProductCode);
                        if (add_to_cart_offer_pinned.status_code == (int)MethodOutputStatusCode.Success)
                        {
                            //---------- Take Screenshot Add to Cart:
                            screenshot_item = await TakeScreenshot(updated_item.OrderCode, updated_item.ProductCode + "Added Pinned Offer to Cart");
                            if (screenshot_item != null)
                            {
                                screenshot_offers.Add("Added Pinned Offer to Cart", screenshot_item);
                            }
                            //---------- Added, Set true to checkout:
                            added_to_cart = true;
                        }
                    }
                }

                //---- If pinned not added, check normal offer:
                if (!added_to_cart)
                {
                    List<OfferListingAvailable> other_offer_list = new List<OfferListingAvailable>();
                    var normal_offer = _autoPurchase.CheckOfferListing(_driver, offers_dictionary["OtherOfferList"]);
                    _logger.LogInformation(normal_offer.message);
                    if (normal_offer.status_code == (int)MethodOutputStatusCode.Success)
                    {
                        var offer_list = _driver.FindElements(By.XPath(offers_dictionary["OtherOfferList"]));
                        if (offer_list.Count > 0)
                        {
                            foreach (var offer_item in offer_list)
                            {
                                var offer_check = _autoPurchase.OfferListing_OfferCheck(_driver, offer_item, offers_dictionary, item);
                                _logger.LogInformation(offer_check.message);
                                FileHelper.WriteLogToFile(offer_check.message, item.OrderCode, item.ProductCode);
                                if (offer_check.status_code == (int)MethodOutputStatusCode.Success)
                                {
                                    //---------- Added to List:
                                    other_offer_list.Add((OfferListingAvailable)offer_check.data);
                                    FileHelper.WriteLogToFile("Added: Normal Offer to Offer List", item.OrderCode, item.ProductCode);

                                }
                            }
                        }
                        //------ Add to Offer Item in List accepted. prioritize : Prime + new > new:
                        var prime_list = other_offer_list.Where(x => x.is_prime == true).ToList();
                        if (prime_list != null && prime_list.Count > 0)
                        {
                            foreach (var prime_item in prime_list)
                            {
                                //-------- In range, Add To Cart:
                                var add_to_cart_offer_pinned = _autoPurchase.OfferListing_AddToCart(_driver, prime_item.offer, offers_dictionary);
                                _logger.LogInformation(add_to_cart_offer_pinned.message);
                                FileHelper.WriteLogToFile(add_to_cart_offer_pinned.message.Trim() + " to Offer List", item.OrderCode, item.ProductCode);

                                if (add_to_cart_offer_pinned.status_code == (int)MethodOutputStatusCode.Success)
                                {
                                    //---------- Take Screenshot Add to Cart:
                                    screenshot_item = await TakeScreenshot(updated_item.OrderCode, updated_item.ProductCode + "Added  Prime new Offer to Cart");
                                    if (screenshot_item != null)
                                    {
                                        screenshot_offers.Add("Added  Prime new Offer to Cart", screenshot_item);
                                    }
                                    //---------- Added, Set true to checkout:
                                    added_to_cart = true;
                                    break;
                                }
                            }
                        }
                        //------ If no prime found, check new:
                        var new_list = other_offer_list.Where(x => x.is_prime == false).ToList();
                        if (!added_to_cart && new_list != null && new_list.Count > 0)
                        {
                            foreach (var normal_item in new_list)
                            {
                                //-------- In range, Add To Cart:
                                var normal_item_offer = _autoPurchase.OfferListing_AddToCart(_driver, normal_item.offer, offers_dictionary);
                                _logger.LogInformation(normal_item_offer.message);
                                FileHelper.WriteLogToFile(normal_item_offer.message.Trim() + " to Offer List", item.OrderCode, item.ProductCode);
                                if (normal_item_offer.status_code == (int)MethodOutputStatusCode.Success)
                                {
                                    //---------- Take Screenshot Add to Cart:
                                    screenshot_item = await TakeScreenshot(updated_item.OrderCode, updated_item.ProductCode + "Added New Offer to Cart");
                                    if (screenshot_item != null)
                                    {
                                        //---------- Add or Replace:
                                        try
                                        {
                                            screenshot_offers.Add("Added New Offer to Cart", screenshot_item);
                                        }
                                        catch 
                                        {
                                            screenshot_offers["Added New Offer to Cart"]=screenshot_item;
                                        }
                                    }
                                    //---------- Added, Set true to checkout:
                                    added_to_cart = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                //-- Check and Close Popup After Click Add To Cart:
                List<string> xpath_skip_btn = new List<string>();
                xpath_skip_btn.Add(_configuration["Xpath:AddedTocartNoAdditionalButton"]);
                var closepopup_1 = _autoPurchase.CloseAdsPopupAfterClickAddToCart(_driver, xpath_skip_btn);
                _logger.LogInformation(closepopup_1.message);
                FileHelper.WriteLogToFile(closepopup_1.message.Trim() + " to Offer List", item.OrderCode, item.ProductCode);

                //-- Additional Offer Listing from URL in page:
                if (!added_to_cart)
                {

                }
                //---- If still nothing added, return failed:
                if (!added_to_cart)
                {
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseFailure;
                    updated_item.PurchaseMessage += ". AutomaticBuyItemFromOfferListing - Không có Offer nào đạt điều kiện để đặt mua sản phẩm.";
                    updated_item.UpdateLast = DateTime.Now;
                    FileHelper.WriteLogToFile(updated_item.PurchaseMessage.Trim() + " to Offer List", item.OrderCode, item.ProductCode);
                    return updated_item;
                }
                else
                {

                    updated_item.ManualNote = "AutomaticBuyItemFromOfferListing - Order Success";
                    updated_item.Screenshot = JsonConvert.SerializeObject(screenshot_offers);
                    //----- Checkout:
                    updated_item = await AutomaticBuyItem_Checkout(updated_item, screenshot_offers);
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation("AutomaticBuyItemFromOfferListing: " + JsonConvert.SerializeObject(item) + "\nError: \n" + ex.ToString());
                updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.ErrorOnExcution;
                updated_item.PurchaseMessage = ex.ToString();
                updated_item.UpdateLast = DateTime.Now;
            }
            return updated_item;
        }
        /// <summary>
        /// Checkout tu dong
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<AutomaticPurchaseAmz> AutomaticBuyItem_Checkout(AutomaticPurchaseAmz item, Dictionary<string, string> exists_img_uploaded)
        {
            var updated_item = item;
            var screenshots_checkout = exists_img_uploaded;
            string screenshot_item = null;
            try
            {
              
                //-- Go To Cart URL:
                _driver.Navigate().GoToUrl(_configuration["Amazon:Cart_URL"]);

                FileHelper.WriteLogToFile(_configuration["Amazon:Cart_URL"], item.OrderCode, item.ProductCode);

                //-- Take Screenshot Cart:
                screenshot_item = await TakeScreenshot(updated_item.OrderCode, "Cart Page");
                if (screenshot_item != null)
                {
                    screenshots_checkout.Add("Cart Page", screenshot_item);
                }

                //--Generate Xpath use in Cart:
                Dictionary<string, string> cart_item_xpath = new Dictionary<string, string>();
                cart_item_xpath.Add("CartHeader", _configuration["Xpath:Cart:CartHeader"]);
                cart_item_xpath.Add("CartList", _configuration["Xpath:Cart:CartList"]);
                cart_item_xpath.Add("CartItemSelectQuantity", _configuration["Xpath:Cart:CartItemSelectQuantity"]);
                cart_item_xpath.Add("CartItemDelete", _configuration["Xpath:Cart:CartItemDelete"]);
                cart_item_xpath.Add("CheckoutURLDetectPart", _configuration["Xpath:Cart:CheckoutURLDetectPart"]);
                cart_item_xpath.Add("SkipPOCase1", _configuration["Xpath:Cart:SkipPOCase1"]);
                cart_item_xpath.Add("CartItemByASIN", _configuration["Xpath:Cart:CartItemByASIN"]);
                cart_item_xpath.Add("cart_price_1", _configuration["Xpath:Cart:cart_price_1"]);

                //-- Remove Non-In-Order Product:
                var remove_product_cart_output = _autoPurchase.CartRemoveNonOrderItems(_driver, cart_item_xpath, updated_item);
                _logger.LogInformation(remove_product_cart_output.message);
                FileHelper.WriteLogToFile(remove_product_cart_output.message, item.OrderCode, item.ProductCode);

                if (remove_product_cart_output.status_code != (int)MethodOutputStatusCode.Success)
                {
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.ErrorOnExcution;
                    updated_item.PurchaseMessage = remove_product_cart_output.message;
                    updated_item.UpdateLast = DateTime.Now;
                    return updated_item;
                }

                //-- Go To Cart URL again:
                _driver.Navigate().GoToUrl(_configuration["Amazon:Cart_URL"]);
                FileHelper.WriteLogToFile(_configuration["Amazon:Cart_URL"], item.OrderCode, item.ProductCode);

                //-- Re-check Product in Cart:
                var recheckcart_output = _autoPurchase.ReCheckProductInCart(_driver, cart_item_xpath, updated_item);
                _logger.LogInformation(recheckcart_output.message);
                FileHelper.WriteLogToFile(recheckcart_output.message, item.OrderCode, item.ProductCode);

                if (recheckcart_output.status_code != (int)MethodOutputStatusCode.Success)
                {
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseFailure;
                    updated_item.PurchaseMessage = recheckcart_output.message;
                    updated_item.UpdateLast = DateTime.Now;
                    return updated_item;
                }

                //-- Take Screenshot Recorrect:
                screenshot_item = await TakeScreenshot(updated_item.OrderCode, "Cart Page Recorrect Item");
                if (screenshot_item != null)
                {
                    screenshots_checkout.Add("Cart Page Recorrect Item", screenshot_item);
                }

                //-- Check Coupon in Cart (Old Function):
                var cartcheckcoupon_output = _autoPurchase.CartCheckCoupon(_driver, updated_item);
                _logger.LogInformation(cartcheckcoupon_output.message);


                //-- Process To Checkout:
                Dictionary<string, string> checkout_dict = new Dictionary<string, string>();
                checkout_dict.Add("Login_Username", user_name);
                checkout_dict.Add("Login_Password", password);

                checkout_dict.Add("CheckoutButton", _configuration["Xpath:Checkout:CheckoutButton"]);
                checkout_dict.Add("CheckoutURLDetectPart", _configuration["Xpath:Checkout:CheckoutURLDetectPart"]);
                checkout_dict.Add("PO_FormInput", _configuration["Xpath:Checkout:PO_FormInput"]);
                checkout_dict.Add("PO_URLDetect", _configuration["Xpath:Checkout:PO_URLDetect"]);

                checkout_dict.Add("PO_Input", _configuration["Xpath:Checkout:PO_Input"]);
                checkout_dict.Add("PO_SubmitBtn", _configuration["Xpath:Checkout:PO_SubmitBtn"]);
                checkout_dict.Add("PO_SkipCase1", _configuration["Xpath:Checkout:PO_SkipCase1"]);

                checkout_dict.Add("PO_SkipCase2", _configuration["Xpath:Checkout:PO_SkipCase2"]);
                checkout_dict.Add("ShippingAddress_URLDetect", _configuration["Xpath:Checkout:ShippingAddress_URLDetect"]);
                checkout_dict.Add("ShippingAddress_ContinuesBtn", _configuration["Xpath:Checkout:ShippingAddress_ContinuesBtn"]);

                checkout_dict.Add("ShippingAddress_ContinuesBtn_2", _configuration["Xpath:Checkout:ShippingAddress_ContinuesBtn_2"]);
                checkout_dict.Add("ShippingOption_URLDetect", _configuration["Xpath:Checkout:ShippingOption_URLDetect"]);
                checkout_dict.Add("PaymentMethod_URLDetect", _configuration["Xpath:Checkout:PaymentMethod_URLDetect"]);

                checkout_dict.Add("PaymentMethod_ContinuesBtn", _configuration["Xpath:Checkout:PaymentMethod_ContinuesBtn"]);
                checkout_dict.Add("Checkout_ProblemElementDetect", _configuration["Xpath:Checkout:Checkout_ProblemElementDetect"]);
                checkout_dict.Add("Checkout_ProblemTextDetect", _configuration["Xpath:Checkout:Checkout_ProblemTextDetect"]);

                checkout_dict.Add("Checkout_ProblemErrorDetail", _configuration["Xpath:Checkout:Checkout_ProblemErrorDetail"]);
                checkout_dict.Add("Checkout_ErorURLDetect", _configuration["Xpath:Checkout:Checkout_ErorURLDetect"]);
                checkout_dict.Add("PlaceYourOrder_Btn", _configuration["Xpath:Checkout:PlaceYourOrder_Btn"]);
                checkout_dict.Add("ForcePlaceOrder_Btn", _configuration["Xpath:Checkout:ForcePlaceOrder_Btn"]);

                var checkout_output = _autoPurchase.ProccessCheckOut(_driver, checkout_dict, updated_item);
                _logger.LogInformation(checkout_output.message);
                if (checkout_output.status_code != (int)MethodOutputStatusCode.Success)
                {
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseFailure;
                    updated_item.PurchaseMessage = checkout_output.message;
                    updated_item.UpdateLast = DateTime.Now;
                    return updated_item;
                }

                //-- Add Screenshot Checkout:
                var order_img_path = checkout_output.error_img_path;
                if (order_img_path != string.Empty)
                {
                    var upload_image = await _usExAPI.UploadImage(order_img_path, _configuration["API_NEW:UploadImageDomain"]);
                    if (upload_image.status_code == (int)MethodOutputStatusCode.Success)
                    {
                        screenshots_checkout.Add("Process Checkout", (string)upload_image.data);
                    }
                    else
                    {
                        _logger.LogInformation(upload_image.message);
                        FileHelper.WriteLogToFile(upload_image.message, item.OrderCode, item.ProductCode);

                    }
                }

                //-- Order Success, Gather Order Infomation:
                Dictionary<string, string> dictionary_success = new Dictionary<string, string>();
                dictionary_success.Add("URLDetect", _configuration["Xpath:PurchaseSuccess:URLDetect"]);
                dictionary_success.Add("DirectToOrderURL", _configuration["Xpath:PurchaseSuccess:DirectToOrderURL"]);
                dictionary_success.Add("OrderList_Elements", _configuration["Xpath:PurchaseSuccess:OrderList_Elements"]);
                dictionary_success.Add("OrderListItem_OrderURL", _configuration["Xpath:PurchaseSuccess:OrderListItem_OrderURL"]);
                dictionary_success.Add("OrderListItem_OrderedProductURL", _configuration["Xpath:PurchaseSuccess:OrderListItem_OrderedProductURL"]);
                dictionary_success.Add("Order_SellerID", _configuration["Xpath:PurchaseSuccess:Order_SellerID"]);
                dictionary_success.Add("Order_ExpectedDeliveryDates", _configuration["Xpath:PurchaseSuccess:Order_ExpectedDeliveryDates"]);
                //-- Take Screenshot Order Success:
                screenshot_item = await TakeScreenshot(updated_item.OrderCode, "Order Success");
                if (screenshot_item != null)
                {
                    screenshots_checkout.Add("Order Success", screenshot_item);
                }

                var gather_information = _autoPurchase.PurchasedSuccess(_driver, dictionary_success, updated_item);
                _logger.LogInformation(gather_information.message);
                if (gather_information.status_code != (int)MethodOutputStatusCode.Success)
                {

                    //-- Failed Still Return Success but note error while gather info to manual note:
                    updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseSuccess;
                    updated_item.PurchaseMessage = "Bot mua tự động thành công. ASIN: " + item.ProductCode + ". Số lượng: " + item.Quanity + ". Ngày Order: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    updated_item.ManualNote = "Lỗi khi lấy thông tin về đơn hàng đã đặt: " + gather_information.message;
                    updated_item.UpdateLast = DateTime.Now;
                    FileHelper.WriteLogToFile(updated_item.ManualNote, item.OrderCode, item.ProductCode);

                    return updated_item;
                }
                //-- Take Screenshot Your Order Page:
                screenshot_item = await TakeScreenshot(updated_item.OrderCode, "Your Order Page");
                if (screenshot_item != null)
                {
                    screenshots_checkout.Add("Your Order Page", screenshot_item);
                }
                Dictionary<string, string> order_success_detail = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(gather_information.data));
                //-- Update Success Detail:
                updated_item.DeliveryStatus = (int)OrderDeliveryStatus.OrderPlaced;
                updated_item.PurchasedSellerName = order_success_detail["SellerIDOrdered"].Trim();
                updated_item.PurchasedOrderId = order_success_detail["OrderDetailID"];
                updated_item.OrderDetailUrl = order_success_detail["OrderDetailURL"];
                updated_item.OrderedSuccessUrl = order_success_detail["SuccessURL"];

                //-- Return Detail:
                updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.PurchaseSuccess;
                //updated_item.ManualNote = "AutomaticBuyItem - Order Success";
                updated_item.PurchaseMessage = "Bot mua tự động thành công. ";
                updated_item.UpdateLast = DateTime.Now;
                updated_item.Screenshot = JsonConvert.SerializeObject(screenshots_checkout);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("AutomaticBuyItemFromOfferListing: " + JsonConvert.SerializeObject(item) + "\nError: \n" + ex.ToString());
                updated_item.PurchaseStatus = (int)AutomaticPurchaseStatus.ErrorOnExcution;
                updated_item.PurchaseMessage = ex.ToString();
                updated_item.UpdateLast = DateTime.Now;
            }
            return updated_item;
        }
        private async Task SendEmailAutomaticBuyExcuteComplete(AutomaticPurchaseAmz autobuy_direct_item, AutomaticPurchaseAmz autobuy_fromoffer_item, bool is_purchased_success, bool is_purchase_direct)
        {
            try
            {
                string title = "thất bại";
                if (is_purchased_success)
                {
                    title = "thành công";
                }
                string email_subject = "USEx_AutomaticPurchase_AMZ - Mua " + title + " [" + autobuy_direct_item.ProductCode + "] - [" + autobuy_direct_item.OrderCode + "]";
                //string email_subject = "USEx_AutomaticPurchase_AMZ - Test 2";

                string from_email = _configuration["EmailOperation:BuyFailed_FromEmail"];
                string to_email = _configuration["EmailOperation:BuyFailed_ToEmail"];
                string template = File.ReadAllText(Directory.GetCurrentDirectory() + @"\Templates\AutomaticBuyFailed.html");
                string body = template.Replace("${AutoPurchaseID}", autobuy_direct_item.Id.ToString())
                    .Replace("${order_id}", autobuy_direct_item.OrderId.ToString())
                    .Replace("${PurchaseURL}", autobuy_direct_item.PurchaseUrl)
                    .Replace("${ASIN}", autobuy_direct_item.ProductCode)
                    .Replace("${Code}", autobuy_direct_item.OrderCode)
                    .Replace("${direct_buy_msg}", autobuy_direct_item.PurchaseMessage)
                    .Replace("${CreateDate}", autobuy_direct_item.CreateDate.ToString("dd/MM/yyyy HH:mm:ss"))
                    .Replace("${offer_listing_msg}", autobuy_fromoffer_item.PurchaseMessage);
                //--- Img attachment:
                string img_template = "<tr><td style=\"text-align: justify;width:30%\">{Key}</td><td><img src=\"{Value}\"/></td></tr>";
                string email_img_tr = "";
                string email_img_tr_2 = "";

                try
                {
                    var img_src = JsonConvert.DeserializeObject<Dictionary<string, string>>(autobuy_direct_item.Screenshot);
                    if (img_src.Count > 0)
                    {
                        foreach (var img in img_src)
                        {
                            email_img_tr += img_template.Replace("{Key}", img.Key).Replace("{Value}", img.Value);
                        }
                    }
                    if (is_purchased_success)
                    {
                        body = body.Replace("${buy_success_URL}", autobuy_direct_item.OrderedSuccessUrl.Trim());
                        body = body.Replace("${buy_success_order_detail_url}", autobuy_direct_item.OrderDetailUrl.Trim());
                    }
                    else
                    {
                        body = body.Replace("${buy_success_URL}", "");
                        body = body.Replace("${buy_success_order_detail_url}", "");
                    }
                }
                catch
                {

                }
                if (!is_purchase_direct)
                {
                    try
                    {
                        var img_src_2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(autobuy_fromoffer_item.Screenshot);
                        if (img_src_2.Count > 0)
                        {
                            foreach (var img in img_src_2)
                            {
                                email_img_tr_2 += img_template.Replace("{Key}", img.Key).Replace("{Value}", img.Value);
                            }
                        }
                    }
                    catch
                    {

                    }
                    if (is_purchased_success)
                    {
                        body = body.Replace("${buy_success_URL}", autobuy_fromoffer_item.OrderedSuccessUrl.Trim());
                        body = body.Replace("${buy_success_order_detail_url}", autobuy_fromoffer_item.OrderDetailUrl.Trim());
                    }
                    else
                    {
                        body = body.Replace("${buy_success_URL}", "");
                        body = body.Replace("${buy_success_order_detail_url}", "");
                    }
                }
                body = body.Replace("${IMG_source}", email_img_tr);
                body = body.Replace("${IMG_source_2}", email_img_tr_2);
                body = body.Replace("${title}", title.ToUpper());

                //-- Post to API Send Email:
                Dictionary<string, string> email = new Dictionary<string, string>();
                email.Add("FromEmail", from_email);
                email.Add("ToEmail", to_email);
                email.Add("Subject", email_subject);
                email.Add("Body", body);
                string email_post = _configuration["API_OLD:Domain"] + _configuration["API_OLD:API_SendEmail"];
                if (CheckToken())
                {
                    var send_email_operation = await _uSExOldAPI.SendEmailAPI(email_post, token, email);
                    _logger.LogInformation(send_email_operation.message);
                    if (send_email_operation.status_code != (int)MethodOutputStatusCode.Success)
                    {
                        LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - SendEmailNotification - Cannot SendEmail " + send_email_operation.message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("SendEmailNotification:" + "\nError: \n" + ex.ToString());
                LogHelper.InsertLogTelegram("App_AutomaticPurchase_AMZ - SendEmailNotification: " + ex.ToString());

            }
        }

        private bool CheckToken()
        {
            if (token.token != null || token.token.Trim() == "" || token.exprire_date < DateTime.UtcNow)
            {
                string token_url = _configuration["API_OLD:Domain"] + _configuration["API_OLD:API_GetToken"];
                var gettoken = _uSExOldAPI.GetToken(token_url, _configuration["API_OLD:User_name"], _configuration["API_OLD:Password"]).Result;
                if (gettoken.status_code == (int)MethodOutputStatusCode.Success)
                {
                    token = new USOLDToken()
                    {
                        token = gettoken.message,
                        exprire_date = DateTime.UtcNow.AddDays(7)
                    };
                    _logger.LogInformation("Worker - CheckToken - OLD API Token: {0} ", token.token);

                }
                else
                {
                    _logger.LogInformation("Worker - CheckToken - Cannot Get Token From OLD API ...");
                    LogHelper.InsertLogTelegram("Worker - CheckToken - Cannot Get Token From OLD API: " + gettoken.message);

                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Khởi tạo ChromeDriver + Login
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ChromeInitilization()
        {
            try
            {
                //-- Setting Option:
                var chrome_option = new ChromeOptions();
                /*
                {
                    BinaryLocation = @"C:\Program Files\Google\Chrome Beta\Application\chrome.exe",
                };*/
                chrome_option.AddArgument("--start-maximized");
                chrome_option.AddArgument("--log-level=3"); //Start Silently.
                chrome_option.AddArgument("--disable-remote-fonts");
                chrome_option.AddArgument("--disable-extensions");
                chrome_option.AddArgument("--disable-remote-fonts");

                //-- Setting ChromeDriver:
                _driver = new ChromeDriver(app_path, chrome_option, new TimeSpan(0, 0, 120));
                _driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 5);
                var homeUrl = new Uri("https://www.amazon.com/");
                _driver.Navigate().GoToUrl(homeUrl);
                await Task.Delay(1000);

                bool is_logged = false, get_token_success = false;
                var login_result = _autoPurchase.Login(_driver, user_name, password);
                if (login_result.status_code == (int)MethodOutputStatusCode.Success)
                {
                    _logger.LogInformation("Login Success: " + login_result.message);
                    is_logged = true;
                }
                else
                {
                    //-- Manual and Check if logged:
                    _logger.LogInformation("Login Failed: " + login_result.message);
                    _logger.LogInformation("Please Do Manual Login in 30 seconds");
                    Thread.Sleep(30 * 1000);
                    login_result = _autoPurchase.Login(_driver, user_name, password, false);
                    if (login_result.status_code == (int)MethodOutputStatusCode.Success)
                    {
                        _logger.LogInformation("Retry Login Success. ");
                        is_logged = true;
                    }
                    else
                    {
                        _logger.LogInformation("Login Failed: " + login_result.message);
                    }
                }
                get_token_success = true;
                if (is_logged && get_token_success)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ChromeInitilization Error" + ex.ToString());
            }
            return false;
        }
        private void ClickElementAndWait(IWebElement element, int delay_in_seconds = 1)
        {
            if (element != null)
            {
                IJavaScriptExecutor executor = (IJavaScriptExecutor)_driver;
                executor.ExecuteScript("arguments[0].click();", element);
                Thread.Sleep(delay_in_seconds * 1000);
            }
        }
        private async Task<string> TakeScreenshot(string order_code, string step_name)
        {

            string file_path = ImageHelper.TakeScreenshot(_driver, step_name, order_code);
            var upload_image = await _usExAPI.UploadImage(file_path, _configuration["API_NEW:UploadImageDomain"]);
            if (upload_image.status_code == (int)MethodOutputStatusCode.Success)
            {
                return (string)upload_image.data;
            }
            else
            {
                _logger.LogInformation("TakeScreenshot Failed: " + upload_image.message);
                return null;
            }
        }
    }

}