using App_AutoPurchase_TrackingOrders.Repositories;
using Entities.Models;
using Entities.ViewModels.AutomaticPurchase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace App_AutoPurchase_TrackingOrders
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUsExAPI _usExAPI;
        private readonly ITrackingAmazon _trackingAmazon;
        private string user_name = null, password = null;
        private int time_delay = 3600 * 1000;
        private int delay_login = 30 * 1000;
        private ChromeDriver _driver;
        private string app_path = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\net6.0", "");
        public Worker(ILogger<Worker> logger, IConfiguration configuration, IUsExAPI usExAPI, ITrackingAmazon trackingAmazon)
        {
            _logger = logger;
            _configuration = configuration;
            _usExAPI = usExAPI;
            _trackingAmazon = trackingAmazon;

        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {

            user_name = _configuration["Login:Username"];
            password = _configuration["Login:Password"];
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
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    time_delay = Convert.ToInt32(_configuration["Setting:TimeDelay_Main"])*1000;
                    delay_login = Convert.ToInt32(_configuration["Setting:Delay_Login"]) * 1000;

                }
                catch
                {

                }
                _logger.LogInformation("App_AutoPurchase_TrackingOrders running: {time}", DateTimeOffset.Now);
                await ExcuteService();
                _logger.LogInformation("Excute Completed. Delay : "+(time_delay/1000)+" s.");
                await Task.Delay(time_delay, stoppingToken);
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
        public async Task ExcuteService()
        {
            try
            {
                string trackinglist_url = _configuration["API_NEW:Domain"].Trim() + _configuration["API_NEW:API_Get_TrackingList"].Trim();

                var tracking_list = await _usExAPI.GetTrackingList(trackinglist_url);

                if (tracking_list.status_code == (int)MethodOutputStatusCode.Success)
                {
                    List<AutomaticPurchaseAmz> excute_list = JsonConvert.DeserializeObject<List<AutomaticPurchaseAmz>>(tracking_list.data);
                    if (excute_list == null || excute_list.Count <= 0)
                    {
                        return;
                    }
                    string update_new_url = _configuration["API_NEW:Domain"] + _configuration["API_NEW:API_UpdateTrackingDetail"];
                    string api_new_key = _configuration["API_NEW:API_Key"];
                    int user_excution = Convert.ToInt32(_configuration["Login:UserExcution"]);

                    foreach (var item in excute_list)
                    {
                        _logger.LogInformation("Order Code: "+item.OrderCode+". Product Code: "+item.ProductCode);

                        AutomaticPurchaseAmz updated_item = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(JsonConvert.SerializeObject(item));
                        if (updated_item.PurchaseStatus != (int)AutomaticPurchaseStatus.PurchaseSuccess|| updated_item.OrderDetailUrl==null || !updated_item.OrderDetailUrl.Contains(_configuration["Tracking:URLDetect"]))
                        {
                            updated_item.DeliveryStatus = (int)OrderDeliveryStatus.RefundPakage;
                            updated_item.DeliveryMessage = "Order Not Success or Order not have OrderDetailURL";
                            updated_item.OrderEstimatedDeliveryDate = DateTime.Now;
                            //-- Update DB + History:
                            var update_db_cannot_excute = await _usExAPI.UpdateTrackingDetail(updated_item, update_new_url, updated_item.DeliveryMessage, user_excution, api_new_key);
                            _logger.LogInformation(update_db_cannot_excute.message);
                            if (update_db_cannot_excute.status_code != (int)MethodOutputStatusCode.Success)
                            {
                                LogHelper.InsertLogTelegram("App_AutoPurchase_TrackingOrders - UpdateTrackingDetail with AutoPurchaseID: " + updated_item.Id + "  Purchase URL: " + updated_item.DeliveryMessage + "  \nError" + update_db_cannot_excute.message);
                            }
                            continue;
                        }
                        Dictionary<string, string> tracking_dict = new Dictionary<string, string>();
                        tracking_dict.Add("TrackingURL", _configuration["Tracking:TrackingURL"]);
                        tracking_dict.Add("TrackingURL_2", _configuration["Tracking:TrackingURL_2"]);
                        tracking_dict.Add("URLDetect", _configuration["Tracking:URLDetect_2"]);
                        tracking_dict.Add("OrderDetailUrl", updated_item.OrderDetailUrl);
                        tracking_dict.Add("PurchasedOrderId", updated_item.PurchasedOrderId);
                        tracking_dict.Add("password", password);

                        //-- Direct to Tracking URL:
                        var direct_to_tracking = _trackingAmazon.DirectToTrackingPage(_driver, tracking_dict);
                        if (direct_to_tracking.status_code != (int)MethodOutputStatusCode.Success)
                        {
                            _logger.LogInformation("DirectToTrackingPage Failed: " + direct_to_tracking.message);
                            _logger.LogInformation("Waiting to Manual Login ... ");
                            await Task.Delay(delay_login);
                            if (!_driver.Url.Contains(_configuration["Tracking:TrackingURL"]))
                            {
                                //---- Return if cannot Direct to Tracking URL:
                                continue;
                            }
                        }
                        else
                        {
                            _logger.LogInformation("DirectToTrackingPage Success: " + direct_to_tracking.message);
                        }
                        Dictionary<string, string> xpath = new Dictionary<string, string>();
                        xpath.Add("DeliveryMainSlot", _configuration["Tracking:DeliveryMainSlot"]);
                        xpath.Add("DeliveryMainSlot_Date", _configuration["Tracking:DeliveryMainSlot_Date"]);
                        xpath.Add("DeliveryStatus", _configuration["Tracking:DeliveryStatus"]);
                        xpath.Add("DeliveryStatus_Detail", _configuration["Tracking:DeliveryStatus_Detail"]);
                        xpath.Add("DeliveryMessage", _configuration["Tracking:DeliveryMessage"]);
                        xpath.Add("ShippingDetail", _configuration["Tracking:ShippingDetail"]);
                        xpath.Add("ShippingTrackingID", _configuration["Tracking:ShippingTrackingID"]);
                        xpath.Add("ShippingDetail_Carrier", _configuration["Tracking:ShippingDetail_Carrier"]);
                        xpath.Add("TrackingEventDetail", _configuration["Tracking:TrackingEventDetail"]);
                        xpath.Add("TrackingEventDetail_Time", _configuration["Tracking:TrackingEventDetail_Time"]);
                        xpath.Add("TrackingEventDetail_Message", _configuration["Tracking:TrackingEventDetail_Message"]);
                        xpath.Add("Cancelled_Delivery", _configuration["Tracking:Cancelled_Delivery"]);
                        xpath.Add("Cancelled_Message", _configuration["Tracking:Cancelled_Message"]);
                        xpath.Add("ShipmentInfo", _configuration["OrderDetail:ShipmentInfo"]);

                        xpath.Add("URLDetect", _configuration["Tracking:URLDetect_2"]);
                        xpath.Add("TrackingURL_2", _configuration["Tracking:TrackingURL_2"]);
                        xpath.Add("OrderDetailUrl", updated_item.OrderDetailUrl);
                        xpath.Add("PurchasedOrderId", updated_item.PurchasedOrderId);
                        var gather_information = _trackingAmazon.GatherInformation(_driver, updated_item, xpath);
                        _logger.LogInformation(" GatherInformation :" + gather_information.message);
                        if (direct_to_tracking.status_code == (int)MethodOutputStatusCode.Success)
                        {
                            updated_item = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(gather_information.data);
                        }
                        else
                        {
                            updated_item.DeliveryStatus = (int)OrderDeliveryStatus.CannotTracking;
                            updated_item.DeliveryMessage = "Cannot Tracking: " + gather_information.message;
                        }
                        var if_refund = _trackingAmazon.CheckIfRefund(_driver, updated_item, xpath);
                        if (if_refund.status_code == (int)MethodOutputStatusCode.Success)
                        {
                            updated_item = JsonConvert.DeserializeObject<AutomaticPurchaseAmz>(if_refund.data);
                        }
                        else
                        {
                            _logger.LogInformation("CheckIfRefund :" + if_refund.message);
                        }
                        _logger.LogInformation("Delivery Status: " + updated_item.DeliveryStatus + ". Delivery Message: " + updated_item.DeliveryMessage);

                        if (updated_item.DeliveryStatus!= item.DeliveryStatus || updated_item.OrderEstimatedDeliveryDate != item.OrderEstimatedDeliveryDate)
                        {
                            updated_item.UpdateLast = DateTime.Now;
                            //-- Update DB + History:
                            var update_db_new = await _usExAPI.UpdateTrackingDetail(updated_item, update_new_url, updated_item.DeliveryMessage, user_excution, api_new_key);
                            _logger.LogInformation(update_db_new.message);
                            if (update_db_new.status_code != (int)MethodOutputStatusCode.Success)
                            {
                                LogHelper.InsertLogTelegram("App_AutoPurchase_TrackingOrders - UpdateTrackingDetail with AutoPurchaseID: " + updated_item.Id + "  Purchase URL: " + updated_item.DeliveryMessage + "  \nError" + update_db_new.message);
                            }
                            _logger.LogInformation("Updated: "+ DateTime.Now);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Cannot Get Tracking List: " + tracking_list.message);
                }
            } 
            catch(Exception ex)
            {
                _logger.LogInformation("Error While Excute: " + ex.ToString());
            }
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
                var login_result = _trackingAmazon.Login(_driver, user_name, password);
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
                    login_result = _trackingAmazon.Login(_driver, user_name, password, false);
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
    }
}
