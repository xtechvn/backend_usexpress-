using App_AutoPurchase_TrackingOrders.Lib;
using App_AutoPurchase_TrackingOrders.Model;
using Entities.Models;
using Entities.ViewModels.AutomaticPurchase;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Utilities.Contants;

namespace App_AutoPurchase_TrackingOrders.Repositories
{
    public class TrackingAmazonRepository : ITrackingAmazon
    {
        public MethodOutput CheckIfRefund(ChromeDriver driver, AutomaticPurchaseAmz model, Dictionary<string, string> xpath)
        {
            MethodOutput methodOutput = new MethodOutput();
            methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
            methodOutput.message = "Normal Pakage - No Refund";
            var updated_item = model;
            try
            {
                driver.Navigate().GoToUrl(model.OrderDetailUrl);
                Thread.Sleep(3000);
                IWebElement shipment_info = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(xpath["ShipmentInfo"]), out shipment_info))
                {
                    var text = shipment_info.GetAttribute("innerHTML");
                    if(text!=null && text.Trim().ToLower().Contains("return"))
                    {
                        updated_item.DeliveryStatus = (int)OrderDeliveryStatus.RefundPakage;
                        methodOutput.data = JsonConvert.SerializeObject(updated_item);
                        methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    }
                    else if (text != null && text.Trim().ToLower().Contains("delivered"))
                    {
                        updated_item.DeliveryStatus = (int)OrderDeliveryStatus.Delivered;
                        methodOutput.data = JsonConvert.SerializeObject(updated_item);
                        methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    }
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput DirectToTrackingPage(ChromeDriver driver, Dictionary<string, string> dictionary)
        {
            MethodOutput methodOutput = new MethodOutput();
            methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
            methodOutput.message = "Failed";
            try
            {
                if ((dictionary["OrderDetailUrl"] == null || dictionary["OrderDetailUrl"].Trim() == "") && (dictionary["PurchasedOrderId"] == null || dictionary["PurchasedOrderId"].Trim() == ""))
                {
                    methodOutput.message = "Cannot Go to Tracking Page: Data Invalid";

                    return methodOutput;
                }
                else if (dictionary["PurchasedOrderId"] == null || dictionary["PurchasedOrderId"].Trim() == "")
                {
                    dictionary["PurchasedOrderId"] = dictionary["OrderDetailUrl"].Trim().ToLower().Split("orderid=")[1];
                }
                driver.Navigate().GoToUrl(dictionary["TrackingURL"] + dictionary["PurchasedOrderId"]);
                Thread.Sleep(6000);
               
                //-- Check if Require Password, Login again:
                IWebElement elePassword = null;
                if (XpathHelper.IsElementPresent(driver, By.Id("ap_password"), out elePassword))
                {
                    if (elePassword.Displayed)
                    {
                        elePassword.Clear();
                        elePassword.SendKeys(dictionary["password"]);
                        Thread.Sleep(1000);
                    }
                    //-- Tick remember me:
                    IWebElement eleRemember = null;
                    if (XpathHelper.IsElementPresent(driver, By.XPath("//input[contains(@name,\"rememberMe\") and contains(@type,\"checkbox\")]"), out eleRemember))
                    {
                        eleRemember.Click();
                        Thread.Sleep(1000);
                    }
                    //-- Click Sign in
                    IWebElement eleBtnSignIn = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("signInSubmit"), out eleBtnSignIn))
                    {
                        // executor.ExecuteScript("arguments[0].click();", eleBtnSignIn);
                        eleBtnSignIn.Click();
                        Thread.Sleep(1000);
                        if (driver.Url.Contains("/signin"))
                        {
                            methodOutput.message = "Login Error/Cannot Press Login Button. Need to Login Manual";
                            methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                        }
                        else
                        {
                            methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                            methodOutput.message = "DirectToTrackingPage Success";
                        }
                        Thread.Sleep(1000);
                    }
                }
              

                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "DirectToTrackingPage Success";

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput GatherInformation(ChromeDriver driver, AutomaticPurchaseAmz model, Dictionary<string, string> xpath)
        {
            MethodOutput methodOutput = new MethodOutput();
            methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
            methodOutput.message = "Failed";
            methodOutput.data = JsonConvert.SerializeObject(model);
            var updated_item = model;
            try
            {
                if (driver.Url.Contains(xpath["URLDetect"]))
                {
                    if (xpath["PurchasedOrderId"] == null || xpath["PurchasedOrderId"].Trim() == "")
                    {
                        xpath["PurchasedOrderId"] = xpath["OrderDetailUrl"].Trim().ToLower().Split("orderid=")[1];
                    }
                    driver.Navigate().GoToUrl(xpath["TrackingURL_2"] + xpath["PurchasedOrderId"]);
                    Thread.Sleep(6000);
                }

                if (updated_item.DeliveryStatus == (int)OrderDeliveryStatus.Delivered)
                {
                    methodOutput.message = "Already Delivered";
                    return methodOutput;
                }
                bool is_delivered = false,on_delivering=false,if_cancelled=false;
                updated_item.DeliveryMessage = "";
                //-- Get Main slot
                IWebElement element = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(xpath["Cancelled_Delivery"]), out element))
                {
                    string mainslot_date_text = element.Text;
                    if (mainslot_date_text != null && mainslot_date_text.Contains("Cancelled"))
                    {
                        if_cancelled = true;
                        updated_item.DeliveryStatus = (int)OrderDeliveryStatus.RefundPakage;
                        IWebElement message = null;
                        if (XpathHelper.IsElementPresent(driver, By.XPath(xpath["Cancelled_Message"]), out message))
                        {
                            updated_item.DeliveryMessage = message.Text.Trim();
                        }
                        else
                        {
                            updated_item.DeliveryStatus = (int)OrderDeliveryStatus.CannotTracking;
                            updated_item.DeliveryMessage = "Item Cancelled";
                        }
                    }
                    
                }
                if (XpathHelper.IsElementPresent(driver, By.XPath(xpath["DeliveryMainSlot"]), out element))
                {
                    string main_slot = element.Text;
                    if (main_slot!=null && main_slot.Trim().ToLower().Contains("delivered"))
                    {
                        updated_item.DeliveryStatus = (int)OrderDeliveryStatus.Delivered;
                        is_delivered = true;
                        updated_item.DeliveryMessage += main_slot + ". ";
                    }
                    //-- Get EstimatedDeliveryDate: Change to parse Main Text:
                    IWebElement mainslot_date = null;
                    if (XpathHelper.IsElementPresent(element, By.XPath(xpath["DeliveryMainSlot_Date"]), out mainslot_date))
                    {
                        string mainslot_date_text = mainslot_date.Text;
                        updated_item.OrderEstimatedDeliveryDate = XpathHelper.ParseDateTimeFromStringNoYear(element.Text);
                    }
                    //-- Check if On Shipped
                    IWebElement detail_status = null;
                    if (!is_delivered && XpathHelper.IsElementPresent(driver, By.XPath(xpath["DeliveryStatus"]), out detail_status))
                    {
                        string status = detail_status.GetAttribute("innerHTML");
                        if (status != null && status.Trim().ToLower().Contains("shipped"))
                        {
                            updated_item.DeliveryStatus = (int)OrderDeliveryStatus.CarrierPickedUpPakage;
                            IWebElement tracking_id = null;
                            if (XpathHelper.IsElementPresent(driver, By.XPath(xpath["ShippingTrackingID"]), out tracking_id))
                            {
                                updated_item.DeliveryMessage += tracking_id.GetAttribute("innerHTML").Trim() + ". ";
                                updated_item.DeliveryStatus = (int)OrderDeliveryStatus.OnDelivering;
                            }
                            on_delivering = true;
                        }
                        else
                        {
                            updated_item.DeliveryStatus = (int)OrderDeliveryStatus.OrderPlaced;
                            IWebElement status_detail = null;
                            if (XpathHelper.IsElementPresent(driver, By.XPath(xpath["DeliveryStatus_Detail"]), out status_detail))
                            {
                                updated_item.DeliveryMessage += status_detail.GetAttribute("innerHTML").Trim()+". ";
                                updated_item.DeliveryStatus = (int)OrderDeliveryStatus.OnDelivering;
                            }
                        }
                        //-- Check if have status detail
                    }
                    //-- Check if on delivering to Warehouse:
                    IWebElement shipping_detail = null;
                    if (!on_delivering && XpathHelper.IsElementPresent(driver, By.XPath(xpath["ShippingDetail"]), out shipping_detail))
                    {
                        IWebElement carrier = null;
                        if (XpathHelper.IsElementPresent(shipping_detail, By.XPath(xpath["ShippingDetail_Carrier"]), out carrier))
                        {
                            updated_item.DeliveryMessage +=carrier.GetAttribute("innerHTML").Trim()+ ".  ";
                            if (!is_delivered)
                            {
                                updated_item.DeliveryStatus = (int)OrderDeliveryStatus.OnDelivering;
                            }
                        }
                    }
                    methodOutput.data = JsonConvert.SerializeObject(updated_item);
                }
                else
                {
                    if (if_cancelled)
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Success;

                        methodOutput.message = "Item Cancelled.";
                    }
                    else
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                        methodOutput.message = "Cannot Find Element xpath[\"DeliveryMainSlot\"] To Gather Infomation";

                    }
                }
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput Login(ChromeDriver driver, string user_name, string password, bool remembered = true, bool login_need_redirect = true)
        {
            MethodOutput methodOutput = new MethodOutput();
            // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            if (login_need_redirect)
            {
                driver.Navigate().GoToUrl("https://www.amazon.com");
            }
            try
            {
                if (user_name == null || user_name.Trim() == "")
                {
                    methodOutput.message = "Username Null";
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                }
                else if (password == null || password.Trim() == "")
                {
                    methodOutput.message = "Password Null";
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                }
                //-- Check nếu đã đăng nhập bằng cách tìm box account, nếu có url redirect về trang home của account thì = đã login
                IWebElement login_element = null;
                var xpath_check_login = "//a[contains(@class,\"nav-a nav-a-2   nav-progressive-attribute\")]";
                if (XpathHelper.IsElementPresent(driver, By.XPath(xpath_check_login), out login_element))
                {
                    var url = login_element.GetAttribute("href");
                    if (url.StartsWith("https://www.amazon.com/gp/css/homepage.html"))
                    {
                        //-- If Login, do nothing, if 2 will Resolve it:
                    }
                    else
                    {
                        //-- Chưa login, redirect về trang login
                        var aTag = login_element;
                        var urlLogin = aTag.GetAttribute("href");
                        var uriLogin = new Uri(urlLogin);
                        driver.Navigate().GoToUrl(uriLogin);
                    }
                }
                //-- Nếu đã chuyển tới trang login
                if (driver.Url.Contains("https://www.amazon.com/ap/signin"))
                {
                    //check trường hợp switch account
                    IWebElement eleSwitchAccount = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("ap-account-switcher-container"), out eleSwitchAccount))
                    {
                        IWebElement eleBtnSwitchAccount = null;
                        if (XpathHelper.IsElementPresent(driver, By.XPath("//div[contains(@class,'cvf-account-switcher-claim')]"), out eleBtnSwitchAccount))
                        {
                            //executor.ExecuteScript("arguments[0].click();", eleBtnSwitchAccount);
                            eleBtnSwitchAccount.Click();
                            Thread.Sleep(1000);
                            methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                            methodOutput.message = "Already logged in. Switch Account Clicked";
                            return methodOutput;
                        }
                    }
                    //-- Email
                    IWebElement eleEmail = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("ap_email"), out eleEmail)) // nếu chưa login
                    {
                        if (eleEmail.Displayed)
                        {
                            eleEmail.Clear();
                            eleEmail.SendKeys(user_name);
                        }

                        IWebElement eleBtnContinue = null;
                        if (XpathHelper.IsElementPresent(driver, By.XPath("//input[@id='continue' and contains(@aria-labelledby,'continue-announce')]"), out eleBtnContinue))
                        {
                            eleBtnContinue.Click();
                            Thread.Sleep(1000);
                        }
                    }
                    //-- Password
                    IWebElement elePassword = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("ap_password"), out elePassword))
                    {
                        if (elePassword.Displayed)
                        {
                            elePassword.Clear();
                            elePassword.SendKeys(password);
                            Thread.Sleep(1000);
                        }
                        //-- Tick remember me:
                        IWebElement eleRemember = null;
                        if (XpathHelper.IsElementPresent(driver, By.XPath("//input[contains(@name,\"rememberMe\") and contains(@type,\"checkbox\")]"), out eleRemember))
                        {
                            eleRemember.Click();
                            Thread.Sleep(1000);
                        }
                        //-- Click Sign in
                        IWebElement eleBtnSignIn = null;
                        if (XpathHelper.IsElementPresent(driver, By.Id("signInSubmit"), out eleBtnSignIn))
                        {
                            // executor.ExecuteScript("arguments[0].click();", eleBtnSignIn);
                            eleBtnSignIn.Click();
                            Thread.Sleep(1000);
                            if (driver.Url.Contains("/signin"))
                            {
                                methodOutput.message = "Login Error/Cannot Press Login Button. Need to Login Manual";
                                methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                            }
                            else
                            {
                                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                                methodOutput.message = "New Logged in Success";
                            }
                            Thread.Sleep(1000);
                        }
                    }
                }
                //-- Đã login
                else
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = "Already Logged in";
                }
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }
    }
}
