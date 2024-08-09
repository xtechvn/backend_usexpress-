using Entities.ViewModels;
using Entities.ViewModels.Payment;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Payoo.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Utilities.Contants;


namespace WEB.UI.Controllers.Payment.Payoo
{
    public partial class PayooService
    {
        private readonly IConfiguration configuration;
        public PayooService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public async Task<List<PayooBankViewModel>> getBankList()
        {
            string JsonContent = string.Empty;
            try
            {
                var obj_bank_payoo = new List<PayooBankViewModel>();
                string url = configuration["payoo:url_load_icon_bank_atm"];

                using (var webclient = new System.Net.WebClient())
                {
                    JsonContent = await webclient.DownloadStringTaskAsync(url);
                }
                JArray JsonParent = JArray.Parse("[" + JsonContent.Trim() + "]");
                JObject j_bank_payment = JsonParent[0] as JObject;

                var icons = j_bank_payment["bank_payment"]["icons"];
                foreach (var item in icons) 
                {
                    var model = new PayooBankViewModel
                    {
                        code = item["code"].ToString(),
                        name = item["name"].ToString(),
                        url_icon = configuration["payoo:url_bank_logo_new"] + item["code"].ToString().ToLower() + ".png"
                    };
                    obj_bank_payoo.Add(model);
                }
                return obj_bank_payoo.Count() > 0 ? obj_bank_payoo : null;
            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getBankList _msg error = " + ex.ToString());
                return null;
            }
        }

        public string getDetailErrorPayoo(string error_msg)
        {
            try
            {
                switch (error_msg)
                {
                    case "501":
                        return "Declined";
                    case "503":
                        return " Merchant Not Exist";
                    case "505":
                        return " Invalid Amount";
                    case "507":
                        return "Unspecified Failure";
                    case "508":
                        return "Invalid Card Number";
                    case "509":
                        return " Invalid Card Name";
                    case "510":
                        return "Expiry Card";
                    case "511":
                        return "Not Registered";
                    case "512":
                        return " Invalid Card Date";
                    case "513":
                        return "Exist Amount";
                    case "521":
                        return "Insufficient Fund";
                    case "522":
                        return " Invalid Account";
                    case "523":
                        return " Account Lock";
                    case "524":
                        return "Invalid Card Infor";
                    case "525":
                        return "Invalid OTP";
                    case "599":
                        return "User cancel";
                    case "800":
                        return "Bank Pending";
                    case "755":
                        return "Invalid parameters";
                    case "500":
                        return "Payment fail";
                    default:
                        return "Error undefined with error_msg = " + error_msg;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getDetailErrorPayoo _msg error = " + ex.ToString());
                return "Error undefined with error_msg = " + ex.ToString();
            }
        }

        public async Task<string> RedirectToPayoo(PayooConfigViewModel payoo_config)
        {
            try
            {
                PayooOrder order = new PayooOrder();
                order.Session = new Random().Next().ToString();
                order.OrderDescription = payoo_config.OrderDescription; //"Thanh toán đơn hàng " + payoo_config.OrderNo + ". Số tiền: " + payoo_config.TotalAmountLast.ToString("N0") + "đ. Sản phẩm: Đồng hồ Garmin... Số lượng 1";
                order.BusinessUsername = payoo_config.BusinessUsername;
                order.OrderCashAmount = Convert.ToInt64(payoo_config.TotalAmountLast); // Tong so tien khach phai tra sau khi tru het chi phí
                order.OrderNo = payoo_config.OrderNo;
                order.ShippingDays = short.Parse(payoo_config.ShippingDays);
                order.ShopBackUrl = HttpUtility.UrlEncode(payoo_config.ShopBackUrl);//urlencode lại
                order.ShopDomain = payoo_config.ShopDomain;
                order.ShopID = long.Parse(payoo_config.ShopID);
                order.ShopTitle = payoo_config.ShopTitle;
                order.StartShippingDate = DateTime.Now.ToString("dd/MM/yyyy");
                order.NotifyUrl = HttpUtility.UrlEncode(payoo_config.NotifyUrl);//urlencode lại
                                                                                //order.ValidityTime = queryOrder.CreatedDate.AddDays(1).ToString("yyyyMMddHHmmss");
                order.ValidityTime = DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmss");
                // thong tin khach hang
                order.CustomerName = payoo_config.CustomerName;
                order.CustomerPhone = payoo_config.Phone;
                //order.CustomerEmail = model.Email;
                order.CustomerEmail = payoo_config.EmailUsexpress;
                order.CustomerAddress = payoo_config.Address;                
                order.OrderDescription = HttpUtility.UrlEncode("Thanh toán đơn hàng "+ payoo_config.OrderNo + ". Số tiền: "+ payoo_config.TotalAmountLast.ToString("N0") + " đ. Sản phẩm " + payoo_config.OrderDescription);

                string XML = PaymentXMLFactory.GetPaymentXML(order);
                string Checksum = string.Empty;
                //Su dung checksum ko ma hoa du lieu
                Checksum = new PayooCommon().EncryptSHA512(payoo_config.ChecksumKey + XML);

                /// start request to payoo server
                /// code này của payoo cung cấp
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(payoo_config.ApiPayooCheckout);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Method = "POST";

                string postData = "data=" + HttpUtility.UrlEncode(XML);
                postData += "&checksum=" + HttpUtility.UrlEncode(Checksum);
                postData += "&refer=" + HttpUtility.UrlEncode(payoo_config.ShopDomain);

                if (payoo_config.CardType == (int)PaymentType.VISA_PAYOO_PAY)
                    postData += "&method=cc-payment";
                else
                    postData += "&method=bank-payment";


                #region PAYOO BY CHOOSE BANK -----Upgrade: CuongLv. CreateByDate: 10-04-2019 ------
                if (payoo_config.CardType == (int)PaymentType.ATM_PAYOO_PAY)
                {
                    // Chuyển hướng người dùng sang thẳng kênh ngân hàng đã chọn
                    // add thêm param
                    postData += "&bank=" + HttpUtility.UrlEncode(payoo_config.BankCode);  //(Là viết tắt của tên ngân hàng do bên Payoo cũng cấp get động qua api. Ví dụ: VCB--> VietComBank)
                }
                #endregion


                httpWebRequest.ContentLength = postData.Length;
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(postData.ToString());
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var result = String.Empty;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                // convert từ text sang json
                dynamic json = JsonConvert.DeserializeObject(result);

                if (json.order != null)
                {
                    if (json.result == "success")
                    {
                        string _returnUrl = json.order.payment_url;
                        return _returnUrl;
                    }
                }

                // write log error
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "RedirectToPayoo _msg failed payoo response  = " + result + "--> param input us = " + JsonConvert.SerializeObject(payoo_config));
                //add log db

                return string.Empty;

                //if (json.order != null)
                //{
                //    if (json.result == "success")
                //    {
                //        // ép kiểu thành string nếu không sẽ trả về kiểu mảng ở json
                //        string _returnUrl = json.order.payment_url;
                //        // CuongLv bổ sung param order_id ngày 28-10-2018
                //        return Json(new { brs = true, msg = "[PAYOO] Khởi tạo thành công đơn hàng từ USEXPRESS", paymentType = model.Paymentype, createRequestUrlPayMent = _returnUrl, order_id = order_id }, JsonRequestBehavior.AllowGet);
                //    }
                //    else
                //    {
                //       // Repository.addLogResponseTrueMoney(Code, JsonConvert.SerializeObject(json));
                //        return Json(new { brs = false, paymentType = model.Paymentype, msg = "Xin lỗi bạn. Kênh thanh toán của chúng tôi đang gặp sự cố. Xin vui lòng quay lại sau. Bấm <a href='/'>vào đây</a> để quay về trang chủ", }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                //else
                //{
                //    //kuonglv log them ngay 14-3-2019
                //   // Repository.addLogResponseTrueMoney(Code, (json));
                //    return Json(new { brs = false, paymentType = model.Paymentype, msg = "Xin lỗi bạn. Kênh thanh toán qua PAYOO của chúng tôi đang trong quá trình hoàn thiện. Xin vui lòng quay lại sau. Bấm <a href='/'>vào đây</a> để quay về trang chủ", }, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "RedirectToPayoo _msg error = " + ex.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// Payoo libraries for IPN Listener
        /// </summary>
        /// <param name="strPackage"></param>
        /// <returns></returns>
        public PayooConnectionPackage GetPayooConnectionPackage(string strPackage)
        {
            try
            {
                string PackageData = strPackage;
                PayooConnectionPackage objPackage = new PayooConnectionPackage();
                XmlDocument Doc = new XmlDocument();
                Doc.LoadXml(PackageData);
                objPackage.Data = ReadNodeValue(Doc, "Data");
                objPackage.Signature = ReadNodeValue(Doc, "Signature");
                objPackage.KeyFields = ReadNodeValue(Doc, "KeyFields");
                return objPackage;
            }
            catch (Exception ex)
            {                
                return null;
            }
        }
        private string ReadNodeValue(XmlDocument Doc, string TagName)
        {
            try
            {
                XmlNodeList nodeList = Doc.GetElementsByTagName(TagName);
                string NodeValue = null;
                if (nodeList.Count > 0)
                {
                    XmlNode node = nodeList.Item(0);
                    NodeValue = (node == null) ? "" : node.InnerText;
                }
                return NodeValue == null ? "" : NodeValue;
            }
            catch (Exception ex)
            {            
                return string.Empty;
            }
        }

        public PaymentNotification GetPaymentNotify(string NotifyData)
        {
            try
            {
                string Data = Encoding.UTF8.GetString(Convert.FromBase64String(NotifyData));
                PaymentNotification invoice = new PaymentNotification();
                XmlDocument Doc = new XmlDocument();
                Doc.LoadXml(Data);
                if (!string.IsNullOrEmpty(ReadNodeValue(Doc, "BillingCode")))
                {
                    // Pay at store
                    if (!string.IsNullOrEmpty(ReadNodeValue(Doc, "ShopId")))
                    {
                        invoice.ShopID = long.Parse(ReadNodeValue(Doc, "ShopId"));
                    }
                    invoice.OrderNo = ReadNodeValue(Doc, "OrderNo");
                    if (!string.IsNullOrEmpty(ReadNodeValue(Doc, "OrderCashAmount")))
                    {
                        invoice.OrderCashAmount = long.Parse(ReadNodeValue(Doc, "OrderCashAmount"));
                    }
                    invoice.State = ReadNodeValue(Doc, "State");
                    invoice.PaymentMethod = ReadNodeValue(Doc, "PaymentMethod");
                    invoice.BillingCode = ReadNodeValue(Doc, "BillingCode");
                    invoice.PaymentExpireDate = ReadNodeValue(Doc, "PaymentExpireDate");
                }
                else
                {
                    invoice.Session = ReadNodeValue(Doc, "session");
                    invoice.BusinessUsername = ReadNodeValue(Doc, "username");
                    invoice.ShopID = long.Parse(ReadNodeValue(Doc, "shop_id"));
                    invoice.ShopTitle = ReadNodeValue(Doc, "shop_title");
                    invoice.ShopDomain = ReadNodeValue(Doc, "shop_domain");
                    invoice.ShopBackUrl = ReadNodeValue(Doc, "shop_back_url");
                    invoice.OrderNo = ReadNodeValue(Doc, "order_no");
                    invoice.OrderCashAmount = long.Parse(ReadNodeValue(Doc, "order_cash_amount"));
                    invoice.StartShippingDate = ReadNodeValue(Doc, "order_ship_date");
                    invoice.ShippingDays = short.Parse(ReadNodeValue(Doc, "order_ship_days"));
                    invoice.OrderDescription = System.Web.HttpUtility.UrlDecode((ReadNodeValue(Doc, "order_description")));
                    invoice.NotifyUrl = ReadNodeValue(Doc, "notify_url");
                    invoice.State = ReadNodeValue(Doc, "State");
                    invoice.PaymentMethod = ReadNodeValue(Doc, "PaymentMethod");
                    invoice.PaymentExpireDate = ReadNodeValue(Doc, "validity_time");
                }
                return invoice;
            }
            catch (Exception ex)
            {
                //ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "GetPaymentNotify(NotifyData= " + NotifyData + ")", ex.ToString());
                return null;
            }
        }
    }
}
