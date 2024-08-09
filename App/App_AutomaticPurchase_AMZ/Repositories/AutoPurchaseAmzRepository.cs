using App_AutomaticPurchase_AMZ.Lib;
using App_AutomaticPurchase_AMZ.Model;
using Crawler.ScraperLib.Amazon;
using Entities.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Utilities;
using Utilities.Contants;

namespace App_AutomaticPurchase_AMZ.Repositories
{
    public class AutoPurchaseAmzRepository : IAutoPurchaseAmz
    {
        private readonly IUsExAPI _usExAPI;
        public AutoPurchaseAmzRepository(IUsExAPI usExAPI)
        {
            _usExAPI = usExAPI;
        }

        public MethodOutput CheckAddToCartButtonAvailable(ChromeDriver driver, List<string> dictionary, out IWebElement add_to_cart_button)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "Add to Cart Button Not Found at any case",
                data = "Không tìm thấy nút Add to Cart."
            };
            if (dictionary == null || dictionary.Count < 1)
            {
                add_to_cart_button = null;
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnData;
                methodOutput.message = "Input Data Invalid";
                return methodOutput;
            }
            add_to_cart_button = null;
            try
            {
                foreach (var xpath in dictionary)
                {
                    if (XpathHelper.IsElementPresent(driver, By.XPath(xpath), out add_to_cart_button))
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                        methodOutput.message = "CheckAddToCartButtonAvailable Success. Case By Xpath: " + xpath;
                        methodOutput.data = "Success";
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
                methodOutput.data = "Lỗi khi tìm nút Add To Cart";
            }
            return methodOutput;
        }

        public MethodOutput CloseAdsPopupAfterClickAddToCart(ChromeDriver driver, List<string> skip_btn_xpath)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "CheckAdsPopupAfterClickAddToCart Failed",
            };
            // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            try
            {
                IWebElement popupAddYourOrder = null;
                if (XpathHelper.IsElementPresent(driver, By.ClassName("a-popover-modal"), out popupAddYourOrder))
                {
                    IWebElement closeButton = null;
                    if (XpathHelper.IsElementPresent(driver, By.ClassName("a-button-close"), out closeButton))
                    {
                        try
                        {
                            closeButton.Click();
                            Thread.Sleep(1000);
                        }
                        catch { }

                    }
                    IWebElement nothanks = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("siNoCoverage-announce"), out nothanks))
                    {
                        nothanks.Click();
                        Thread.Sleep(1000);
                    }

                    Thread.Sleep(2000);
                }
                try
                {
                    IWebElement btnNoThanks = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("attachSiNoCoverage-announce"), out btnNoThanks))
                    {

                        btnNoThanks.Click();
                        Thread.Sleep(2000);


                    }
                }
                catch { }
                if (skip_btn_xpath != null && skip_btn_xpath.Count > 0)
                {
                    foreach (var xpath in skip_btn_xpath)
                    {
                        try
                        {
                            if (xpath == null && xpath.Trim() == "")
                            {
                                continue;
                            }
                            IWebElement skip_btn = null;
                            if (XpathHelper.IsElementPresent(driver, By.XPath(xpath), out skip_btn))
                            {
                            
                                    skip_btn.Click();
                                    Thread.Sleep(2000);
                           
                            }
                        }
                        catch { }
                    }
                }
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "CheckAdsPopupAfterClickAddToCart Success.";
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
                methodOutput.data = null;
            }
            return methodOutput;

        }

        public MethodOutput CheckDealOrDiscountAvailable(ChromeDriver driver, Dictionary<string, string> dictionary)
        {
            MethodOutput methodOutput = new MethodOutput();
            // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            try
            {
                string log = "Clicked: ";
                IWebElement btn = null;

                //check subscribe option
                IWebElement snsBuyBoxElement = null;
                if (XpathHelper.IsElementPresent(driver, By.Id("snsBuyBox"), out snsBuyBoxElement))
                {
                    //executor.ExecuteScript("arguments[0].click();", snsBuyBoxElement);
                    snsBuyBoxElement.Click();
                    Thread.Sleep(1000);
                    log += " By.Id(\"snsBuyBox\") .";
                }
                IWebElement snsOptionElement = null;
                if (XpathHelper.IsElementPresent(driver, By.Id("snsOption"), out snsOptionElement))
                {
                    if (XpathHelper.IsElementPresent(driver, By.Id("onetimeOption"), out btn))
                    {
                        // executor.ExecuteScript("arguments[0].click();", btn);
                        btn.Click();
                        Thread.Sleep(1000);
                        log += " By.Id(\"snsOption\") .";

                    }
                    Thread.Sleep(1000);
                }

                //check lightning deal
                IWebElement lightningDealElement = null;
                if (XpathHelper.IsElementPresent(driver, By.Id("LDBuybox"), out lightningDealElement))
                {
                    IWebElement lightningDealButton = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("a-autoid-2-announce"), out lightningDealButton))
                    {
                        var buttonText = lightningDealButton.Text.Trim();
                        if (buttonText.Equals("View Offer")) //lightning deal hết hạn => thì chọn regular price
                        {
                            btn = null;
                            if (XpathHelper.IsElementPresent(driver, By.Id("regularBuybox"), out btn))
                            {
                                // executor.ExecuteScript("arguments[0].click();", btn);
                                btn.Click();
                                Thread.Sleep(1000);
                                log += " By.Id(\"regularBuybox\") .";

                            }
                            btn = null;
                            if (XpathHelper.IsElementPresent(driver, By.Id("oneTimeBuyBox"), out btn))
                            {
                                // executor.ExecuteScript("arguments[0].click();", btn);
                                btn.Click();
                                Thread.Sleep(1000);
                                log += " By.Id(\"oneTimeBuyBox\") .";

                            }
                        }
                    }

                    IWebElement dealOftheDayButton = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("a-autoid-0-announce"), out dealOftheDayButton))
                    {
                        var buttonText = dealOftheDayButton.Text.Trim();
                        if (buttonText.Equals("View Offer"))
                        {
                            btn = null;
                            if (XpathHelper.IsElementPresent(driver, By.Id("regularBuybox"), out btn))
                            {
                                // executor.ExecuteScript("arguments[0].click();", btn);
                                btn.Click();
                                Thread.Sleep(1000);
                                log += " By.Id(\"regularBuybox\") .";

                            }
                            btn = null;
                            if (XpathHelper.IsElementPresent(driver, By.Id("oneTimeBuyBox"), out btn))
                            {
                                // executor.ExecuteScript("arguments[0].click();", btn);
                                btn.Click();
                                Thread.Sleep(1000);
                                log += " By.Id(\"oneTimeBuyBox\") .";

                            }
                        }
                    }
                }

                //-- Check Coupon and Click (Additional Function):
                IReadOnlyCollection<IWebElement> coupon_1 = null;
                if (XpathHelper.IsElementPresents(driver, By.XPath(dictionary["Coupon_1"]), out coupon_1))
                {
                    if (coupon_1.Count > 0)
                    {
                        foreach (var coupon_input in coupon_1)
                        {
                            try
                            {
                                coupon_input.Click();
                                Thread.Sleep(1500);
                                log += " By.XPath(\"" + dictionary["Coupon_1"] + "\") .";

                            }
                            catch { }
                        }
                    }
                }
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "Check Deal and Discount (Old Function) Completed. " + log;
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput CheckOfferListing(ChromeDriver driver, string offer_xpath)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "Offers Listing with Xpath [" + offer_xpath + "] NOT available."
            };
            try
            {
                IWebElement offer_listing = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(offer_xpath), out offer_listing))
                {
                    //-- Return Found:
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = "Offers Listing with Xpath [" + offer_xpath + "] available.";
                }
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput ProccessCheckOut(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item)
        {
            MethodOutput methodOutput = new MethodOutput();
            // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            if (dictionary == null || dictionary.Count < 1)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnData;
                methodOutput.message = "Input Data Invalid";
                return methodOutput;
            }
            try
            {
                //-- Check If Logged again to Process Checkout
                var login_result = Login(driver, dictionary["Login_Username"], dictionary["Login_Password"], false, false);
                FileHelper.WriteLogToFile(login_result.message, item.OrderCode, item.ProductCode);

                if (login_result.status_code != (int)MethodOutputStatusCode.Success)
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "ProccessCheckOut - Cannot Detect Account to Checkout: ";
                    methodOutput.data = "ProccessCheckOut - Không tìm thấy Account để checkout: ";
                    return methodOutput;
                }

                //-- Find and Click Checkout Button
                IWebElement checkout_btn = null;
                if (!XpathHelper.IsElementPresent(driver, By.XPath(dictionary["CheckoutButton"]), out checkout_btn))
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "ProccessCheckOut - Cannot Find Checkout Button";
                    FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);

                    return methodOutput;
                }
                // executor.ExecuteScript("arguments[0].click();", checkout_btn);
                checkout_btn.Click();
                Thread.Sleep(5000);

                string file_path = ImageHelper.TakeScreenshot(driver, "ProccessCheckOut ", item.OrderCode);
                //-- Detect if in Checkout Page:
                if (!driver.Url.Contains(dictionary["CheckoutURLDetectPart"]))
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "ProccessCheckOut - Not in Checkout Page. Current Page: " + driver.Url;
                    methodOutput.data = "ProccessCheckOut - Không thể Redirect đến trang đặt Order. URL hiện tại: " + driver.Url;
                    methodOutput.error_img_path = file_path;
                    FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);

                    return methodOutput;
                }

                //-- Detect Input lock and Write Order code
                bool PO_Continued = false;
                if (driver.Url.Contains(dictionary["PO_URLDetect"]))
                {
                    //Detect Form PO
                    IWebElement form_PO = null;
                    if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["PO_FormInput"]), out form_PO))
                    {
                        IWebElement inputPONumber = null;
                        if (XpathHelper.IsElementPresent(form_PO, By.XPath(dictionary["PO_Input"]), out inputPONumber))
                        {
                            inputPONumber.SendKeys(item.OrderCode);
                            Thread.Sleep(2000);
                        }
                    }

                    //-- Continue if input PO number div is found
                    IWebElement btnContinue = null;
                    if (XpathHelper.IsElementPresent(form_PO, By.XPath(dictionary["PO_SubmitBtn"]), out btnContinue))
                    {
                        //executor.ExecuteScript("arguments[0].click();", btnContinue);
                        btnContinue.Click();
                        Thread.Sleep(3000);
                        PO_Continued = true;
                    }
                    else if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["PO_SkipCase1"]), out btnContinue))
                    {
                        //executor.ExecuteScript("arguments[0].click();", btnContinue);
                        btnContinue.Click();
                        Thread.Sleep(3000);
                        PO_Continued = true;
                    }
                    else if (XpathHelper.IsElementPresent(driver, By.XPath("PO_SkipCase2"), out btnContinue))
                    {
                        // executor.ExecuteScript("arguments[0].click();", btnContinue);
                        btnContinue.Click();
                        Thread.Sleep(3000);
                        PO_Continued = true;
                    }
                }
                if (!PO_Continued)
                {
                    file_path = ImageHelper.TakeScreenshot(driver, "ProccessCheckOut ", item.OrderCode);
                    //-- If Cannot Skip PO in any case:
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "ProccessCheckOut - Cannot Skip PO with any Case: ";
                    methodOutput.data = "ProccessCheckOut - Cannot Skip PO with any Case";
                    methodOutput.error_img_path = file_path;
                    FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);
                    return methodOutput;
                }
                Thread.Sleep(4000);

                //-- Old Function: check nếu có popup addon item, addonReminderPopover
                IWebElement btnAddonItemCheckout = null;
                var xpathPopupAddon = "//div[contains(@class,'a-modal-scroller')]" +
                                      "/./" +
                                      "/form[@id='addonReminderPopover']" +
                                      "/./" +
                                      "/input[@name='proceedToCheckout']";

                if (XpathHelper.IsElementPresent(driver, By.XPath(xpathPopupAddon), out btnAddonItemCheckout))
                {
                    // executor.ExecuteScript("arguments[0].click();", btnAddonItemCheckout);
                    btnAddonItemCheckout.Click();
                    Thread.Sleep(2000);
                    login_result = Login(driver, dictionary["Login_Username"], dictionary["Login_Password"]);
                    if (login_result.status_code != (int)MethodOutputStatusCode.Success)
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                        methodOutput.message = "ProccessCheckOut - addonReminderPopover Cannot Detect Account to Checkout. ";
                        methodOutput.data = "ProccessCheckOut - addonReminderPopover Không tìm thấy Account để checkout. ";
                        FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);
                        return methodOutput;
                    }
                }

                //-- Old Function: Check prime video
                IWebElement prime_video_btn = null;
                if (driver.Url.Contains("primeinterstitial") && XpathHelper.IsElementPresent(driver, By.XPath(xpathPopupAddon), out prime_video_btn))
                {
                    // executor.ExecuteScript("arguments[0].click();", prime_video_btn);
                    prime_video_btn.Click();
                    Thread.Sleep(2000);

                }

                //--  Select Shipping Address
                if (driver.Url.Contains(dictionary["ShippingAddress_URLDetect"]))
                {
                    //ship-to-this-address a-button a-button-primary a-button-span12 a-spacing-medium
                    //data-action="page-spinner-show"
                    IWebElement shippingadress_continues = null;
                    if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["ShippingAddress_ContinuesBtn"]), out shippingadress_continues))
                    {
                        // executor.ExecuteScript("arguments[0].click();", shippingadress_continues);
                        shippingadress_continues.Click();
                        Thread.Sleep(3000);
                    }
                    else if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["ShippingAddress_ContinuesBtn_2"]), out shippingadress_continues))
                    {
                        // executor.ExecuteScript("arguments[0].click();", shippingadress_continues);
                        shippingadress_continues.Click();
                        Thread.Sleep(3000);
                    }
                }

                //-- Choose your shipping options
                if (driver.Url.Contains(dictionary["ShippingAddress_URLDetect"]))
                {
                    IWebElement shipping_option = null;
                    if (XpathHelper.IsElementPresent(driver, By.ClassName("sosp-continue-button"), out shipping_option))
                    {
                        // executor.ExecuteScript("arguments[0].click();", shipping_option);
                        shipping_option.Click();
                        Thread.Sleep(3000);
                    }
                }

                //-- Select a payment method
                if (driver.Url.Contains(dictionary["PaymentMethod_URLDetect"]))
                {
                    IWebElement payment_method = null;
                    if (XpathHelper.IsElementPresent(driver, By.Id("continue-top"), out payment_method))
                    {
                        // executor.ExecuteScript("arguments[0].click();", payment_method);
                        payment_method.Click();
                        Thread.Sleep(3000);

                    }
                    else if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["PaymentMethod_ContinuesBtn"]), out payment_method))
                    {
                        // executor.ExecuteScript("arguments[0].click();", payment_method);
                        payment_method.Click();
                        Thread.Sleep(3000);
                    }
                }
                Thread.Sleep(3000);
                file_path = ImageHelper.TakeScreenshot(driver, "ProccessCheckOut ", item.OrderCode);

                //-- Check if there was problem with order product
                IWebElement max_quantity_exceeded = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["Checkout_ProblemElementDetect"]), out max_quantity_exceeded))
                {
                    if (max_quantity_exceeded.GetAttribute("innerHTML").Trim().Contains(dictionary["Checkout_ProblemTextDetect"]))
                    {
                        IWebElement max_quantity_exceeded_content = null;
                        if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["Checkout_ProblemErrorDetail"]), out max_quantity_exceeded_content))
                        {
                            string notify = max_quantity_exceeded_content.GetAttribute("innerHTML");
                            methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                            methodOutput.message = "ProccessCheckOut - There a Problem with Product in Order: " + notify;
                            methodOutput.data = "ProccessCheckOut - Không thể đặt hàng SP. Lỗi:  " + notify;
                            methodOutput.error_img_path = file_path;
                            FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);
                            return methodOutput;
                        }
                    }
                }
                //-- Place Your Order Click
                IWebElement btnPlaceYourOrder = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["PlaceYourOrder_Btn"]), out btnPlaceYourOrder))
                {
                    btnPlaceYourOrder.Click();
                    Thread.Sleep(5000);
                    
                }
              
                //-- Old Function : check duplicate order,check có bị duplicate product hay ko, co thi force click
                // No Force Click Return failed
                IWebElement eleBtnDuplicate = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["ForcePlaceOrder_Btn"]), out eleBtnDuplicate))
                {
                    eleBtnDuplicate.Click();
                    Thread.Sleep(4000);
                }
                
                Thread.Sleep(5000);
                //-- Check if still Stuck at checkoutpage - ReClick PlaceOrder:
                if (driver.Url.Trim().Contains(dictionary["Checkout_ErorURLDetect"]))
                {
                    //-- Place Your Order Click
                    btnPlaceYourOrder = null;
                    if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["PlaceYourOrder_Btn"]), out btnPlaceYourOrder))
                    {
                        btnPlaceYourOrder.Click();

                    }
                }
                Thread.Sleep(5000);

                //-- if still Stuck at checkoutpage - Return Failed:
                if (driver.Url.Trim().Contains(dictionary["Checkout_ErorURLDetect"]))
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "ProccessCheckOut - There a Problem with Product while Place Your Order Click - Not In Checkout URL. Current:  " + driver.Url;
                    methodOutput.data = "ProccessCheckOut - Lỗi xảy ra khi nhấn nút đặt mua";
                    file_path = ImageHelper.TakeScreenshot(driver, "ProccessCheckOut Stuck", item.OrderCode);
                    methodOutput.error_img_path = file_path;
                    FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);
                    return methodOutput;
                }
                //-- If all Passed: return Success:
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "ProccessCheckOut Success.";
                methodOutput.data = "ProccessCheckOut Success.";
                methodOutput.error_img_path = file_path;
                FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput CheckPrice(ChromeDriver driver, List<string> price_xpath, double order_price,Dictionary<string,string> shipping_fee_xpath=null)
        {
            MethodOutput methodOutput = new MethodOutput();
            if (price_xpath == null || price_xpath.Count < 1)
            {

                methodOutput = new MethodOutput()
                {
                    status_code = (int)MethodOutputStatusCode.Failed,
                    message = "BOT Configuration Not Correct. [Check Price xpath is NULL]",
                    data = "BOT Configuration Not Correct. [Check Price xpath is NULL]",
                };
                return methodOutput;
            }
            try
            {
                double shipping_fee_value = 0;
                if (shipping_fee_xpath != null && shipping_fee_xpath.Count>0)
                {
                    IWebElement shipping_fee_element = null;

                    shipping_fee_element = null;
                    if (XpathHelper.IsElementPresent(driver, By.XPath(shipping_fee_xpath["Xpath"]), out shipping_fee_element))
                    {
                        var str = shipping_fee_element.GetAttribute(shipping_fee_xpath["Field"]).Trim().ToLower();
                        if (!str.Contains("free"))
                        {
                            try
                            {
                                shipping_fee_value = Convert.ToDouble(str.Replace("$", "").Replace(",",""));
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                bool is_price_range = false;
                double product_price = 0;
                string text = "";
                foreach (var xpath in price_xpath)
                {
                    try
                    {
                        text = driver.FindElement(By.XPath(xpath)).GetAttribute("innerHTML").Trim();
                        if (text.Split("$").Count() > 2)
                        {
                            is_price_range = true;
                            break;
                        }
                        if (text != null)
                        {
                            product_price = Convert.ToDouble(text.Replace("$", "").Replace(",", ""));
                        }
                        if (product_price > 0) break;
                    }
                    catch
                    {
                        product_price = 0;
                    }
                }
                if (product_price <= 0)
                {
                    product_price = ParserAmz.getPrice(driver.PageSource, out is_price_range);

                }
                if (product_price <= 0)
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Failed,
                        message = "Cannot Crawl Price. Shipping_fee: "+shipping_fee_value,
                        data = "Không thể lấy được giá sản phẩm. URL mới nhất:  " + driver.Url +". Giá khách đã đặt mua: $" + order_price
                    };
                }
                else if (is_price_range)
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Failed,
                        message = "Product Variation Not Selected. Price Range Return TRUE. Shipping_fee: " + shipping_fee_value,
                        data = "URL sản phẩm chưa chọn kích cỡ / màu sắc. URL mới nhất: " + driver.Url + ". Giá khách đã đặt mua: $" + order_price
                    };
                }
                else if ((product_price+shipping_fee_value) <= (order_price + 2))
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Success,
                        message = "Price: $" + product_price + " + Shipping fee: $"+shipping_fee_value+" <= [Order Amount +2$]: $" + (order_price + 2),
                        data = product_price +shipping_fee_value
                    };
                }
                else
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Failed,
                        message = "Price: $" + product_price+shipping_fee_value + " > [Order Amount +2$]: $" + (order_price + 2),
                        data = "Giá sản phẩm trang Detail cao hơn so với giá khách đã thanh toán. Giá SP hiện tại: $" + product_price + ". Phí Ship hiện tại : $"+shipping_fee_value+". Tổng: $"+(product_price+shipping_fee_value)+". Giá khách đã đặt mua: $" + order_price
                    };
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
                methodOutput.data = -1;
            }
            return methodOutput;

        }

        public MethodOutput CheckProductAvailable(ChromeDriver driver)
        {
            MethodOutput methodOutput = new MethodOutput();
            try
            {
                var product_name = ParserAmz.GetProductName(driver.PageSource).Trim().Replace("\"", "").Replace("'", "");
                if (product_name == String.Empty)
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "Product Not Found";
                    methodOutput.data = "Không tìm thấy sản phẩm";
                }
                else
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = "Product Available";

                }
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
                methodOutput.data = "CheckProductAvailable - ErrorOnExcution";
            }
            return methodOutput;
        }

        public MethodOutput SelectQuanity(ChromeDriver driver, int quanity, List<string> add_to_cart_xpath)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "CheckQuanity Excution Failed.",
            };
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;

            try
            {
                methodOutput.message = "";
                IWebElement eleQuantity = null;
                if (XpathHelper.IsElementPresent(driver, By.Id("quantity"), out eleQuantity)) //chọn lại số lượng nếu có thể
                {
                    if (!eleQuantity.Displayed) // nếu ẩn thì show select lên
                    {
                        executor.ExecuteScript("arguments[0].setAttribute('class', '')", eleQuantity);
                        Thread.Sleep(2000);

                        executor.ExecuteScript("arguments[0].setAttribute('class', '')", driver.FindElement(By.Id("selectQuantity")));
                        Thread.Sleep(1000);
                    }
                    //Check if prime subscribe and save is active, disable it:
                    bool is_exsists = false;

                    IWebElement onetime_purchase = null;
                    IWebElement elequantity_case2 = null;
                    is_exsists = XpathHelper.IsElementPresent(driver, By.XPath("//div[@id=\"accordionRows_feature_div\"]//div[@id=\"buyBoxAccordion\"]//div[@id=\"dealsAccordionRow\"]//span[contains(@class,\"a-size-base gb-accordion-active\")]"), out onetime_purchase);
                    if (is_exsists)
                    {
                        onetime_purchase.Click();

                        //methodOutput.message += "\nSelect Quanity: "+quanity+". ";
                        elequantity_case2 = null;
                        if (XpathHelper.IsElementPresent(driver, By.XPath("//select[contains(@id,'dealOrderQuantityDropdown')]"), out elequantity_case2))
                        {
                            eleQuantity = elequantity_case2;
                        }
                    }
                    else
                    {
                        onetime_purchase = null;
                        is_exsists = XpathHelper.IsElementPresent(driver, By.XPath("//div[@id=\"accordionRows_feature_div\"]//div[@id=\"buyBoxAccordion\"]//div[@id=\"newAccordionRow\"]//span[contains(@class,\"a-text-bold\")]"), out onetime_purchase);
                        if (is_exsists) onetime_purchase.Click();
                    }

                    if (!eleQuantity.Displayed)
                    {
                        //methodOutput.message += "\nSelect Quanity: "+quanity+". ";
                        if (XpathHelper.IsElementPresent(driver, By.XPath("//select[contains(@id,'buyingOption_0-predefinedQuantitiesDropdown')]"), out elequantity_case2))
                        {
                            eleQuantity = elequantity_case2;
                        }
                       
                    }
                    IWebElement stickybuybox = null;
                    //check & remove stickybuybox
                    if (XpathHelper.IsElementPresent(driver, By.ClassName("stickybuybox"), out stickybuybox))
                    {
                        executor.ExecuteScript("return document.getElementsByClassName('stickybuybox')[0].remove(); "); //remove nút buy now

                        Thread.Sleep(1000);
                    }

                    var selectElement = new SelectElement(eleQuantity);
                    var minQuantity = selectElement.Options.FirstOrDefault().Text.Trim();
                    var maxQuantity = selectElement.Options.LastOrDefault().Text.Trim();
                    methodOutput.message = "\nProduct MinQuantity: " + minQuantity + " MaxQuantity: " + maxQuantity + ". ";
                    if (minQuantity.Equals("Select Qty")) //check minimum quantity
                    {
                        var minValue = selectElement.Options.FirstOrDefault(x => x.Text.Contains("Minimum")).GetAttribute("value");
                        if (int.Parse(minValue) > quanity)
                        {
                            methodOutput.message = "\nSelect Quantity FAILED. Product Minimum order quantity Requirement: " + minValue + ". " + methodOutput.message;
                            methodOutput.data += "Số SP đặt: " + quanity + ". Không thể mua sản phẩm tự động. Seller yêu cầu số lượng SP mua tối thiểu: " + minValue;
                            return methodOutput;
                        }

                    }
                    var max = 0;
                    if (int.TryParse(maxQuantity, out max))
                    {
                        if (max < quanity)
                        {
                            methodOutput.message = "\nSelect Quantity FAILED. Product Maximum order quantity Requirement: " + max + ". " + methodOutput.message;
                            methodOutput.data += "Số SP đặt: " + quanity + ". Không thể mua sản phẩm tự động. Seller chỉ cho phép tối đa mua " + max + " sản phẩm ";
                            return methodOutput;
                        }
                    }
                    //set quantity
                    selectElement.SelectByValue(quanity.ToString());
                    Thread.Sleep(1000);
                }
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "\nSelect Quantity Success. Product quantity: " + quanity + ". " + methodOutput.message;
                methodOutput.data += "Số SP đặt: " + quanity + ". Chọn mua số lượng thành công";
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
                methodOutput.data = null;
            }
            return methodOutput;
        }

        public MethodOutput CheckSeller(ChromeDriver driver, string product_code)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "CheckSeller Not Found at any case",
            };
            try
            {
                var seller = ParserAmz.getSellerIdSelected(driver.PageSource, product_code).Replace("\"", "").Replace("'", "");
                if (seller != null && seller.Trim() != "")
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Success,
                        message = "CheckSeller success.",
                        data = seller
                    };
                }
                else
                {
                    methodOutput = new MethodOutput()
                    {
                        status_code = (int)MethodOutputStatusCode.Failed,
                        message = "CheckSeller failed.",
                        data = seller
                    };
                }

            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
                methodOutput.data = null;
            }
            return methodOutput;
        }

        public MethodOutput CheckBuyNewOption(ChromeDriver driver, List<string> dictionary, out IWebElement buy_new_btn)
        {
            MethodOutput methodOutput = new MethodOutput();
            buy_new_btn = null;
            if (dictionary == null || dictionary.Count < 1)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnData;
                methodOutput.message = "Input Data Invalid";
                return methodOutput;
            }
            try
            {

                foreach (var xpath in dictionary)
                {
                    if (XpathHelper.IsElementPresent(driver, By.XPath(xpath), out buy_new_btn))
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                        methodOutput.message = "CheckBuyNewOption Success. Case By Xpath: " + xpath;
                        break;
                    }
                }
                if(methodOutput.message==null || methodOutput.message.Trim() == "")
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "CheckBuyNewOption - Not found in any Case. No monthly delivering option found.";
                }
            }
            catch (Exception ex) { methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution; methodOutput.message = ex.ToString(); }
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

        public MethodOutput CartRemoveNonOrderItems(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "CartRemoveNonOrderItems: Data Incorrect",
            };
            if (dictionary == null || dictionary.Count < 1)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnData;
                methodOutput.message = "Input Data Invalid";
                return methodOutput;
            }
            // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            try
            {
                //-- If Xpath List Null
                if (dictionary == null || dictionary.Count < 1)
                {
                    methodOutput.message = "CartRemoveNonOrderItems: Xpath Null";
                    return methodOutput;
                }
                //-- If No item in cart:
                IWebElement element;
                if (!XpathHelper.IsElementPresent(driver, By.XPath(dictionary["CartList"]), out element))
                {
                    methodOutput.message = "CartRemoveNonOrderItems: No Item in Cart";
                    return methodOutput;
                }
                //-- Xpath not Correct:
                var list = driver.FindElements(By.XPath(dictionary["CartList"]));
                if (list == null || list.Count < 1)
                {
                    methodOutput.message = "CartRemoveNonOrderItems: Xpath find Cart Item found No Items";
                    return methodOutput;
                }
                //-- Delete Other Product:
                string product_code_in_cart; IWebElement del_btn = null;
                var error_ele = new List<IWebElement>();
                string deleted = "";
                foreach (var div in list)
                {
                    product_code_in_cart = null;
                    product_code_in_cart = div.GetAttribute("data-asin");
                    if (product_code_in_cart.Trim() == "" || !product_code_in_cart.Trim().ToUpper().Contains(item.ProductCode.ToUpper()))
                    {
                        del_btn = null;
                        try
                        {
                            del_btn = div.FindElement(By.XPath(dictionary["CartItemDelete"]));
                            deleted += product_code_in_cart.Trim() + " ";
                        }
                        catch
                        {
                            error_ele.Add(div);
                        }
                        if (del_btn != null)
                        {
                            // executor.ExecuteScript("arguments[0].click();", del_btn);
                            del_btn.Click();
                            Thread.Sleep(2000);
                        }
                    }
                }
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "Delete Other Product Success. Error Element Count: " + error_ele.Count + " . Deleted: " + deleted;
                methodOutput.data = error_ele;
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput ReCheckProductInCart(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "ReCheckProductInCart: Data Incorrect",
            };
            if (dictionary == null || dictionary.Count < 1)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnData;
                methodOutput.message = "Input Data Invalid";
                return methodOutput;
            }
            try
            {
                //-- If No item in cart:
                IReadOnlyCollection<IWebElement> list;
                if (!XpathHelper.IsElementPresents(driver, By.XPath(dictionary["CartList"]), out list))
                {
                    methodOutput.message = "ReCheckProductInCart: No Item in Cart";
                    return methodOutput;
                }
                Thread.Sleep(2000);
                //-- Correct Cart Items:
                string product_code_in_cart;
                bool is_product_exists = false;
                foreach (var div in list)
                {

                    product_code_in_cart = null;
                    product_code_in_cart = div.GetAttribute("data-asin");
                    //---- If have another product with diffirent asin:
                    if (product_code_in_cart.Trim() == "" || !product_code_in_cart.Trim().ToUpper().Contains(item.ProductCode.ToUpper()))
                    {
                        methodOutput.message = "Have another product with ProductCode: " + product_code_in_cart;
                        return methodOutput;
                    }
                    //---- If Product is Exists, Check Price, Quanity:
                    if (product_code_in_cart.Trim().ToUpper().Contains(item.ProductCode.ToUpper()))
                    {
                        //-- Product Exists:
                        is_product_exists = true;
                        //-- Check Price
                        List<string> cart_price_xpath = new List<string>();
                        cart_price_xpath.Add(dictionary["cart_price_1"]);
                        var cartitem_checkprice = CartItemCheckPrice(div, cart_price_xpath, item.Amount);
                        if (cartitem_checkprice.status_code != (int)MethodOutputStatusCode.Success)
                        {
                            methodOutput.message = cartitem_checkprice.message;
                            methodOutput.data = cartitem_checkprice.data;
                            return methodOutput;
                        }

                        //-- Check Quanity:
                        var recorrect_cart = CartItemCheckAndReSelectQuantity(driver, div, dictionary["CartItemSelectQuantity"], item.Quanity, dictionary["CartItemByASIN"].Replace("(&ASIN&)", item.ProductCode));
                        if (recorrect_cart.status_code != (int)MethodOutputStatusCode.Success)
                        {
                            methodOutput.message = recorrect_cart.message;
                            methodOutput.data = recorrect_cart.data;
                            return methodOutput;
                        }


                    }
                }
                if (!is_product_exists)
                {
                    methodOutput.message = "ReCheckProductInCart Failed. No item with " + item.ProductCode + " found in Cart";
                    return methodOutput;
                }
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "ReCheckProductInCart Success.";
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }
        private MethodOutput CartItemCheckPrice(IWebElement cart_div, List<string> cart_price_xpath, double order_price, double over_price_accepted = 2)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "CartProductCheckPrice: Data Incorrect",
            };
            try
            {
                var price_text = cart_div.GetAttribute("data-price");
                double cart_price = 0;
                try
                {
                    cart_price = Convert.ToDouble(price_text.Replace("$", "").Replace(",", "").Trim());
                }
                catch
                {
                    cart_price = 0;
                }
                if (cart_price <= 0 && cart_price_xpath.Count > 0)
                {
                    foreach (var xpath in cart_price_xpath)
                    {
                        try
                        {
                            var text = cart_div.FindElement(By.XPath(xpath)).GetAttribute("innerHTML");
                            cart_price = Convert.ToDouble(text.Replace("$", "").Trim().Replace(",", ""));
                        }
                        catch
                        {

                        }
                    }
                }
                if (cart_price <= 0)
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "Cannot Crawl Product Price in Cart. Product Price in Cart: " + cart_price + ". Order Price per 1: " + order_price + ". Diffirent with Accepted Range: " + (cart_price - order_price - over_price_accepted) + "$";
                    methodOutput.data = "Không lấy được giá SP trong cart. Giá SP trong cart: $" + cart_price + ". Giá khách đã đặt mua: $" + order_price;
                }
                else if (cart_price <= order_price + over_price_accepted)
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = "Product Price in Cart: " + cart_price + ". Order Price per 1: " + order_price + ". Diffirent with Accepted Range: " + (cart_price - order_price - over_price_accepted) + "$";

                }
                else
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "Product Price Not In Accepted Range. Product Price in Cart: " + cart_price + ". Order Price per 1: " + order_price + ". Diffirent with Accepted Range: " + (cart_price - order_price - over_price_accepted) + "$";
                    methodOutput.data = "Giá sản phẩm trong Cart cao hơn so với giá khách đã thanh toán. Giá SP trong cart: $" + cart_price + ". Giá khách đã đặt mua: $" + order_price;
                }
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message = ex.ToString();
            }
            return methodOutput;
        }
        private MethodOutput CartItemCheckAndReSelectQuantity(ChromeDriver driver, IWebElement cart_div, string reselect_xpath, int order_quantity, string recheck_xpath)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "CartProductCheckQuantity: Error - ",
            };
            try
            {
                var price_text = cart_div.GetAttribute("data-quantity");
                int cart_quantity = 0;
                cart_quantity = Convert.ToInt32(price_text);
                methodOutput.message = "Order Item Quantity: " + order_quantity + ". Cart Item Quantity: " + cart_quantity;
                if (cart_quantity == order_quantity)
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    return methodOutput;
                }
                //-- Re-select Quantity:
                var selectElement = new SelectElement(cart_div.FindElement(By.XPath(reselect_xpath)));
                selectElement.SelectByValue(order_quantity.ToString());
                Thread.Sleep(4000);

                //-- Check Again:
                IWebElement recheck_div = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(recheck_xpath), out recheck_div))
                {
                    price_text = recheck_div.GetAttribute("data-quantity");
                    cart_quantity = Convert.ToInt32(price_text);
                    methodOutput.message = "Re-Select Quantity. Order Item Quantity: " + order_quantity + ". Cart Item Quantity: " + cart_quantity;
                    if (cart_quantity == order_quantity)
                    {
                        methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                        return methodOutput;
                    }
                    //-- If still not exact Value:
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "Product Quantity Not In Accepted Range. Order Item Quantity: " + order_quantity + ". Cart Item Quantity: " + cart_quantity;
                    methodOutput.data = "Không thể đặt chính xác số lượng sản phẩm trong đơn. Số lượng yêu cầu trong đơn: " + order_quantity + ". Số lượng SP trên cart đang chọn: " + cart_quantity;
                }
                else
                {
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "Cart Item DIV search by Xpath: " + recheck_xpath + " return NULL";
                    methodOutput.data = "Không thể re-check số lượng sản phẩm trong đơn. [Cart Item DIV search by Xpath: " + recheck_xpath + " return NULL] Số lượng yêu cầu trong đơn: " + order_quantity + ". Số lượng SP trên cart đang chọn: " + cart_quantity;
                }
            }
            catch (Exception ex)
            {
                methodOutput.status_code = (int)MethodOutputStatusCode.ErrorOnExcution;
                methodOutput.message += ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput CartCheckCoupon(ChromeDriver driver, AutomaticPurchaseAmz item)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.ErrorOnExcution,
                message = "CartCheckCoupon: Excution Failed: ",
            };
            //  IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            try
            {
                IWebElement couponElement = null;
                var xpathCoupon = "//form[@id='activeCartViewForm']" +
                                    "/div[contains(@class,'sc-list-body')]" +
                                    "/div[contains(@data-asin,'" + item.ProductCode.ToUpper() + "')]" +
                                    "/./" +
                                    "/div[contains(@class,'sc-clipcoupon')]" +
                                    "/./" +
                                    "/a[contains(@class,'a-size-small a-link-normal sc-action-link')]";
                if (XpathHelper.IsElementPresent(driver, By.XPath(xpathCoupon), out couponElement))
                {
                    if (couponElement.Text.Trim().Equals("Clip Coupon"))
                    {
                        //executor.ExecuteScript("arguments[0].click();", couponElement);
                        couponElement.Click();
                        Thread.Sleep(1000);
                    }

                }
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "CartCheckCoupon - Excution Succes";

            }
            catch (Exception ex)
            {
                methodOutput.message += ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput PurchasedSuccess(ChromeDriver driver, Dictionary<string, string> dictionary, AutomaticPurchaseAmz item)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.ErrorOnExcution,
                message = "PurchasedSuccess: Excution Failed. ",
            };
            //IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {

                //-- Detect URL Success
                if (!driver.Url.Contains(dictionary["URLDetect"]))
                {
                    //---- if not, return failed:
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "PurchasedSuccess - Order Success URL InCorrect. Current URL: " + driver.Url;
                    methodOutput.data = "PurchasedSuccess - URL đặt hàng thành công không đúng. URL hiện tại: " + driver.Url;
                    FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);

                    return methodOutput;
                }
                //---- Add Success URL:
                result.Add("SuccessURL", driver.Url);

                //-- Redirect to OrderURL:
                driver.Navigate().GoToUrl(dictionary["DirectToOrderURL"]);
                Thread.Sleep(2000);

                //-- Detect if have order (only First page searching because right-after click to order):
                IWebElement order_div = null;
                if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["OrderList_Elements"]), out order_div))
                {
                    var list_order = driver.FindElements(By.XPath(dictionary["OrderList_Elements"]));
                    if (list_order != null && list_order.Count > 0)
                    {
                        IWebElement correct_order_div = null;

                        foreach (var div in list_order)
                        {
                            IWebElement order_product = null;
                            //---- Check if correct Purchase Product Exists:
                            if (XpathHelper.IsElementPresent(driver, By.XPath(dictionary["OrderListItem_OrderedProductURL"]), out order_product))
                            {
                                string url = order_product.GetAttribute("href");
                                if (url == null || url.Trim() == "" || !url.Contains("/product/"))
                                {
                                    //---- URL not correct
                                }
                                else
                                {
                                    //---- Check if Correct Product
                                    url = url.StartsWith("https://www.amazon.com") ? url : "https://www.amazon.com" + url;
                                    string product_code = null;
                                    if (CommonHelper.CheckAsinByLink(url, out product_code))
                                    {
                                        if (item.ProductCode.Trim().ToUpper() == product_code.Trim().ToUpper())
                                        {
                                            correct_order_div = div;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (correct_order_div != null)
                        {
                            try
                            {
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", correct_order_div);
                                Thread.Sleep(2500);
                            }
                            catch { }
                            //---- Get Purchased Order URL:
                            IWebElement order_url_element = null;
                            if (!XpathHelper.IsElementPresent(correct_order_div, By.XPath(dictionary["OrderListItem_OrderURL"]), out order_url_element))
                            {
                                //---- if not, return failed:
                                methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                                methodOutput.message = "PurchasedSuccess - Cannot found Order Detail URL with Product Code " + item.ProductCode + " in " + driver.Url;
                                methodOutput.data = "Không tìm thấy URL Order Detail tương ứng với " + item.ProductCode + ". Trang order hiện tại: " + driver.Url;
                                FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);

                                return methodOutput;
                            }
                            if (order_url_element != null)
                            {
                                string order_url = order_url_element.GetAttribute("href");
                                order_url = order_url.StartsWith("https://www.amazon.com") ? order_url : "https://www.amazon.com" + order_url;
                                //---- Add Order Detail URL:
                                result.Add("OrderDetailURL", order_url);
                                //---- Get OrderDetailID:
                                result.Add("OrderDetailID", order_url.Trim().Split("orderID=")[1]);
                            }
                            else
                            {
                                result.Add("OrderDetailURL", "Cannot Get Detail URL");
                                result.Add("OrderDetailID", "Cannot Get Detail ID");

                            }
                            //---- Get Exactly SellerID Ordered:
                            IWebElement sellerID_ordered = null;
                            if (XpathHelper.IsElementPresent(order_div, By.XPath(dictionary["Order_SellerID"]), out sellerID_ordered))
                            {
                                string order_url = sellerID_ordered.GetAttribute("data-merchant");
                                if (order_url != null && order_url.Trim() != "")
                                {
                                    result.Add("SellerIDOrdered", order_url.Trim().ToUpper());
                                }
                                else
                                {
                                    result.Add("SellerIDOrdered", "");
                                }
                            }
                            else
                            {
                                result.Add("SellerIDOrdered", "Amazon.com");
                            }
                            //---- Get Expected delivery dates
                            IWebElement order_EDD = null;
                            if (XpathHelper.IsElementPresent(order_div, By.XPath(dictionary["Order_ExpectedDeliveryDates"]), out order_EDD))
                            {
                                string arriving_date = order_EDD.GetAttribute("innerHTML");
                                if (arriving_date != null && arriving_date.Trim() != "")
                                {
                                    result.Add("SellerEDD", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(arriving_date.Trim().Replace("Arriving ", "").ToLower()));
                                }
                                else
                                {
                                    result.Add("SellerEDD", "");
                                }
                            }
                            else
                            {
                                result.Add("SellerEDD", "Unknown EDD");
                            }
                        }
                        else
                        {
                            //---- if not, return failed:
                            methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                            methodOutput.message = "PurchasedSuccess - Cannot found Order with Product Code " + item.ProductCode + " in " + driver.Url;
                            methodOutput.data = "Không tìm thấy bất kỳ Order nào tương ứng với Product Code: " + item.ProductCode + ". Trang order hiện tại: " + driver.Url;
                            FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);

                            return methodOutput;
                        }
                    }
                }
                else
                {
                    //---- if not, return failed:
                    methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                    methodOutput.message = "PurchasedSuccess - Order not found in " + driver.Url;
                    methodOutput.data = "Không tìm thấy bất kỳ Order nào trong trang: " + driver.Url;
                    FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);

                    return methodOutput;
                }
                //-- If All Passed, Success:
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "Gather Information Success";
                methodOutput.data = result;
            }
            catch (Exception ex)
            {
                methodOutput.message += ex.ToString();
            }
            FileHelper.WriteLogToFile(methodOutput.message, item.OrderCode, item.ProductCode);
            return methodOutput;
        }

        public MethodOutput OfferListing_OfferCheck(ChromeDriver driver, IWebElement selected_offer, Dictionary<string, string> offer_xpath, AutomaticPurchaseAmz item)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "OfferListing_OfferCheck: Excution Failed. ",
            };
            OfferListingAvailable result = new OfferListingAvailable();
            try
            {
                //-- Check Header: 
                IWebElement header = null;
                if (XpathHelper.IsElementPresent(selected_offer, By.XPath(offer_xpath["OfferHeadingText"]), out header))
                {
                    //---- If Header Found, check
                    var header_list = selected_offer.FindElements(By.XPath(offer_xpath["OfferHeadingText"]));
                    if (header_list.Count > 0)
                    {
                        foreach (var header_h5 in header_list)
                        {
                            var text = header_h5.GetAttribute("innerHTML");
                            if (text == null || text.Trim() == "" || text.Trim().Contains("Recommended Offer"))
                            {
                                continue;
                            }
                            else
                            {
                                switch (text.Trim().ToLower())
                                {
                                    case "new":
                                        {

                                        }
                                        break;
                                    default:
                                        {
                                            methodOutput.status_code = (int)MethodOutputStatusCode.Failed;
                                            methodOutput.message = "OfferListing_OfferCheck - Offer " + selected_offer.GetAttribute("id") + ": Offer is Used or Others than NEW, Skip offer";
                                            return methodOutput;
                                        }
                                }
                            }
                        }
                    }
                    else
                    {
                        //---- if not, return failed:
                        methodOutput.message = "OfferListing_OfferCheck - Offer " + selected_offer.GetAttribute("id") + ": cannot Detect Header";
                        return methodOutput;
                    }

                }

                //-- Check Price:
                IWebElement price = null;
                if (XpathHelper.IsElementPresent(selected_offer, By.XPath(offer_xpath["OfferCheckPrice"]), out price))
                {
                    try
                    {
                        double shipping_fee = 0;
                        IWebElement shipping_fee_offer = null;
                        if (XpathHelper.IsElementPresent(selected_offer, By.XPath(offer_xpath["OfferCheckShippingFee"]), out shipping_fee_offer))
                        {
                            try
                            {
                                shipping_fee = Convert.ToDouble(shipping_fee_offer.GetAttribute("data-csa-c-delivery-price").Trim().Replace("$", "").Replace(",", ""));
                            }
                            catch { }
                        }
                        var offer_price = Convert.ToDouble(price.GetAttribute("innerHTML").Trim().Replace(",", "").Replace("$", ""));
                        if ((offer_price+shipping_fee) <= item.Amount + 2)
                        {
                        }
                        else
                        {
                            methodOutput.message += "OfferListing_OfferCheck - Offer " + selected_offer.GetAttribute("id") + ": Not Acceptable Price -  Crawl Return: " + offer_price + " , Order Price: " + item.Amount;
                            return methodOutput;
                        }
                    }
                    catch
                    {
                        //---- if not, return failed:
                        methodOutput.message += "OfferListing_OfferCheck - Offer " + selected_offer.GetAttribute("id") + ": Cannot get Price - String Return: " + price.GetAttribute("innerHTML");
                        return methodOutput;
                    }
                }

                //-- Get SellerID
                IWebElement seller = null;
                if (XpathHelper.IsElementPresent(selected_offer, By.XPath(offer_xpath["OfferHeadingText"]), out seller))
                {
                    result.seller_URL = seller.GetAttribute("href");
                    result.seller_name = seller.GetAttribute("innerHTML").Trim();
                }
                else
                {
                    result.seller_URL = "Amazon.com";
                    result.seller_name = "Amazon.com";
                }

                //-- Check if Prime:
                IWebElement is_prime = null;
                if (XpathHelper.IsElementPresent(selected_offer, By.XPath(offer_xpath["OfferISPrimeDetect"]), out is_prime))
                {
                    result.is_prime = true;
                }

                //-- If Passed All, return:
                result.offer = selected_offer;
                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "OfferListing_OfferCheck: Excution Success. All Passed";

                methodOutput.data = result;
            }
            catch (Exception ex)
            {
                methodOutput.message += ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput OfferListing_AddToCart(ChromeDriver driver, IWebElement selected_offer, Dictionary<string, string> offer_xpath)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "OfferListing_AddToCart: Excution Failed. ",
            };
           // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            try
            {
                //-- Get SellerID
                IWebElement add_to_cart = null;
                if (XpathHelper.IsElementPresent(selected_offer, By.XPath(offer_xpath["OfferAddToCart"]), out add_to_cart))
                {
                    //executor.ExecuteScript("arguments[0].scrollIntoView(true);", add_to_cart);
                    //Thread.Sleep(2000);
                    add_to_cart = selected_offer.FindElement(By.XPath(offer_xpath["OfferAddToCart"]));
                    add_to_cart.Click();
                    Thread.Sleep(2000);
                    methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                    methodOutput.message = "Add Offer To Cart Success";
                }
                else
                {
                    methodOutput.message += "Cannot Find Button Add to Cart.";
                }

            }
            catch (Exception ex)
            {
                methodOutput.message += ex.ToString();
            }
            return methodOutput;
        }

        public MethodOutput LoadMoreOffers(ChromeDriver driver, string normal_offer_xpath)
        {
            MethodOutput methodOutput = new MethodOutput()
            {
                status_code = (int)MethodOutputStatusCode.Failed,
                message = "LoadMoreOffers: Excution Failed. ",
            };
            // IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            try
            {
                IWebElement current_item = null;
                int count = 0;
                //-- Get SellerID
                IReadOnlyCollection<IWebElement> lastest_offer = null;
                do
                {
                    if (XpathHelper.IsElementPresents(driver, By.XPath(normal_offer_xpath), out lastest_offer))
                    {
                        if (current_item == null || current_item != lastest_offer.Last())
                        {
                            current_item = lastest_offer.Last();
                        }
                        else if (current_item == lastest_offer.Last())
                        {
                            break;
                        }
                        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                        executor.ExecuteScript("arguments[0].scrollIntoView(true);", lastest_offer.Last());
                        Thread.Sleep(2000);
                    }
                    count++;
                } while (count < 5);

                methodOutput.status_code = (int)MethodOutputStatusCode.Success;
                methodOutput.message = "LoadMoreOffers: Load Success";

            }
            catch (Exception ex)
            {
                methodOutput.message += ex.ToString();
            }
            return methodOutput;
        }
    }
}
