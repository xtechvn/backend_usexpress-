using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Utilities;
using Utilities.Contants;
using WEB.UI.Controllers.Payment.Payoo;
using WEB.UI.Controllers.Payment.Service;
using static Utilities.Contants.OrderConstants;
using Payoo.Lib;

namespace WEB.UI.Controllers.Payment
{
    //[Route("[controller]")]
    public class PayooController : Controller
    {
        private readonly IConfiguration configuration;
        public PayooController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        [HttpGet("Payoo/shop-back.html")]
        public ActionResult ShopbackPayoo()
        {
            string token_tele = configuration["telegram_log_error_fe:Token"];
            string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
            string url_api = configuration["url_api_usexpress_new"];
            string encrypt_api_2 = configuration["KEY_TOKEN_API_2"];
            string orderNo = string.Empty;
            try
            {
                string urlLinkResponse = string.Empty;
                string _viewMsg = string.Empty;

                // note: ghi nhận query string từ kết quả trả về của Payoo
                orderNo = string.IsNullOrEmpty(HttpContext.Request.Query["order_no"]) ? "N/A" : HttpContext.Request.Query["order_no"].ToString();
                var payoo_session = HttpContext.Request.Query["session"].ToString();
                var secretKey = configuration["payoo:ChecksumKey"];
                var checkSum = HttpContext.Request.Query["checksum"].ToString();
                var paymentFee = HttpContext.Request.Query["paymentFee"].ToString();
                var totalAmount = HttpContext.Request.Query["totalAmount"].ToString();

                var payooPaymentStatus = string.IsNullOrEmpty(HttpContext.Request.Query["status"]) ? -1 : Int32.Parse(HttpContext.Request.Query["status"].ToString());

                // note: ghi log đầu hàm để kiểm tra có vào được trang trả về thanh toán thành công hay không
                //Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "ShopbackPayoo return param: " + "orderNo = " + orderNo + "status = " + payooPaymentStatus + ", session = " + payoo_session + ", secretKey = " + secretKey + ", checkSum = " + checkSum + ", paymentFee = " + paymentFee + ", totalAmount = " + totalAmount);

                var orderTmp = new { session = payoo_session, order_no = orderNo, payment_status = payooPaymentStatus, checksum = checkSum, payment_fee = paymentFee, total_amount = totalAmount };
                var orderJson = JsonConvert.SerializeObject(orderTmp).ToString();
                var compare = EncryptSHA.GenerateSHA512String(secretKey + payoo_session + '.' + orderNo + '.' + Convert.ToInt32(payooPaymentStatus));

                var payment = new PaymentConnection(configuration, url_api, "", -1,  token_tele, group_id_tele, -1);
                var order_detail = payment.getOrderDetail(orderNo).Result;
                if(order_detail == null)
                {
                    Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "Đơn hàng " + orderNo + " khong ton tai trong he thong" );
                }
                // B1. Giải mã thông tin trả về từ Payoo có hợp lệ hay không
                if (checkSum.Trim().ToUpper() != compare.Trim().ToUpper())
                {
                    Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "Đơn hàng " + orderNo + " giải mã không hợp lệ  compare="+ compare.Trim().ToUpper() + "checkSum()="+ checkSum.Trim().ToUpper() );
                    ViewBag.is_success = false;
                    ViewBag.payment_type = order_detail.PaymentType;
                    ViewBag.order_no = orderNo;
                    return View("~/Views/Payment/Confirm.cshtml");
                }
                else
                {
                    // B2. Kiểm tra đơn hàng có trong hệ thống
                    if (order_detail != null)
                    {
                        // B2.1. Kiểm tra đơn hàng này đã thanh toán chưa
                        if (order_detail.PaymentStatus == (short)Payment_Status.DA_THANH_TOAN) // note: kiểm tra đơn hàng đã thanh toán chưa
                        {
                            Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "Đơn hàng " + orderNo + " đã được thanh toán thành công !");
                        }

                        ViewBag.payment_type = order_detail.PaymentType;
                        ViewBag.order_no = orderNo;

                        if (payooPaymentStatus == 1)
                        {
                            ViewBag.is_success = true;
                        }
                        else
                        {
                            ViewBag.is_success = false;
                            string errormsg = HttpContext.Request.Query["errormsg"].ToString();
                            string errorcode = HttpContext.Request.Query["errorcode"].ToString();
                            // write log notify với trường hợp thẻ khách hàng gặp lỗi
                            var payoo = new PayooService(configuration);
                            Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[Chi tiết lỗi đơn hàng "+ orderNo + ": errormsg = " + errormsg + "] Lỗi xảy ra khi khách hàng thanh toán thẻ qua Payoo: " + payoo.getDetailErrorPayoo(errorcode));
                        }
                        return View("~/Views/Payment/Confirm.cshtml");
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "Không tìm thấy đơn hàng " + orderNo);
                        ViewBag.is_success = false;
                        ViewBag.payment_type = order_detail.PaymentType;
                        ViewBag.order_no = orderNo;
                        return View("~/Views/Payment/Confirm.cshtml");
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, orderNo + "=" + ex.ToString());
                ViewBag.is_success = false;
                ViewBag.payment_type = -1;
                ViewBag.order_no = "";
                return View("~/Views/Payment/Confirm.cshtml");
            }
        }

        /// <summary>
        /// waiting test payoo....
        /// </summary>
        /// <param name="NotifyData"></param>
        /// <returns></returns>
        [HttpPost("/Payoo/Listener.json")]
        public ActionResult ListenerPayoo(string NotifyData)
        {
      
            string token_tele = configuration["telegram_log_error_fe:Token"];
            string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
            string url_api = configuration["url_api_usexpress_new"];
            string encrypt_api_2 = configuration["KEY_TOKEN_API_2"];
            string orderNo = string.Empty;

           // Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR-NEW:/Payoo/Listener.json] Param Payoo return về qua post  NotifyData" + NotifyData);

            try
            {
                //test data bên payoo
                //NotifyData = "<?xml version=\"1.0\"?><PayooConnectionPackage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Data>PFBheW1lbnROb3RpZmljYXRpb24+PHNob3BzPjxzaG9wPjxzZXNzaW9uPjcwOTM3NTgwNzwvc2Vzc2lvbj48dXNlcm5hbWU+U0JfVVNFWFBSRVNTPC91c2VybmFtZT48c2hvcF9pZD4xNDY5PC9zaG9wX2lkPjxzaG9wX3RpdGxlPlVTRVhQUkVTUzwvc2hvcF90aXRsZT48c2hvcF9kb21haW4+aHR0cDovL3VzZXhwcmVzc3ZuLmNvbTwvc2hvcF9kb21haW4+PHNob3BfYmFja191cmw+aHR0cCUzYSUyZiUyZnFjLmZlLnJldml2aWZ5dmlldG5hbS5jb20lMmZQYXlvbyUyZnNob3AtYmFjay5odG1sPC9zaG9wX2JhY2tfdXJsPjxvcmRlcl9ubz5URVNULVVBTS0yTTA5MDkyPC9vcmRlcl9ubz48b3JkZXJfY2FzaF9hbW91bnQ+NjY3OTI5NDU8L29yZGVyX2Nhc2hfYW1vdW50PjxvcmRlcl9zaGlwX2RhdGU+MDkvMTEvMjAyMDwvb3JkZXJfc2hpcF9kYXRlPjxvcmRlcl9zaGlwX2RheXM+Mzwvb3JkZXJfc2hpcF9kYXlzPjxvcmRlcl9kZXNjcmlwdGlvbj5ET05fSEFOR19URVNULVVBTS0yTTA5MDkyPC9vcmRlcl9kZXNjcmlwdGlvbj48bm90aWZ5X3VybD5odHRwJTNhJTJmJTJmcWMuZmUucmV2aXZpZnl2aWV0bmFtLmNvbSUyZlBheW9vJTJmTGlzdGVuZXIuanNvbjwvbm90aWZ5X3VybD48dmFsaWRpdHlfdGltZT4yMDIwMTExMDIxMzcyNTwvdmFsaWRpdHlfdGltZT48Y3VzdG9tZXI+PG5hbWU+TMOqIFbEg24gQ8aw4budbmc8L25hbWU+PHBob25lPjA5NDIwNjYyOTk8L3Bob25lPjxhZGRyZXNzPktodSDEkcO0IHRo4buLIHbEg24ga2jDqiB0w7JhIG5ow6AgY3QgOSBBMSAtIFBow7pjIFTDom4gLSBIb8OgbiBLaeG6v20gLSBIw6AgTuG7mWk8L2FkZHJlc3M+PGVtYWlsPmNza2hAdXNleHByZXNzLnZuPC9lbWFpbD48L2N1c3RvbWVyPjwvc2hvcD48L3Nob3BzPjxTdGF0ZT5QQVlNRU5UX1JFQ0VJVkVEPC9TdGF0ZT48UGF5bWVudE1ldGhvZD5JTlRFUk5BTF9DQVJEPC9QYXltZW50TWV0aG9kPjwvUGF5bWVudE5vdGlmaWNhdGlvbj4=</Data><Signature>8913863329E8F107C19B12A1C7B7BB2F2D617E40BF8BBE5FF5952D0AC6E39B701EDDA32DB8553CBD1FAAAA0624A8CAB89806DF689FDD95C85EAB8BD2FBBAF105</Signature><PayooSessionID>tw5fN0vt08vNwXju3XUeCzylI1zdnw5XKgFLP/JJah1n5SGK1Ijgd1q3MYCMScp6B7A9521rCsH3HLgQZ0thFQ==</PayooSessionID><KeyFields>PaymentMethod|State|Session|BusinessUsername|ShopID|ShopTitle|ShopDomain|ShopBackUrl|OrderNo|OrderCashAmount|StartShippingDate|ShippingDays|OrderDescription|NotifyUrl|PaymentExpireDate</KeyFields></PayooConnectionPackage>";
                // dữ liệu bên us old
                //NotifyData= "<?xml version =\"1.0\"?><PayooConnectionPackage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Data>PFBheW1lbnROb3RpZmljYXRpb24+PHNob3BzPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzaG9wPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c2Vzc2lvbj45MDI1NTI4NjE8L3Nlc3Npb24+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx1c2VybmFtZT5zaG9wZGVtb19jaGVja3N1bTwvdXNlcm5hbWU+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzaG9wX2lkPjU5MDwvc2hvcF9pZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNob3BfdGl0bGU+U2hvcERlbW88L3Nob3BfdGl0bGU+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzaG9wX2RvbWFpbj5odHRwOi8vbG9jYWxob3N0PC9zaG9wX2RvbWFpbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNob3BfYmFja191cmw+aHR0cDovL1Nob3BEZW1vL1RoYW5rWW91LmFzcHg8L3Nob3BfYmFja191cmw+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxvcmRlcl9ubz45MDI1NTI4NjE8L29yZGVyX25vPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8b3JkZXJfY2FzaF9hbW91bnQ+MTAwMDA8L29yZGVyX2Nhc2hfYW1vdW50PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8b3JkZXJfc2hpcF9kYXRlPjE5LzA0LzIwMTc8L29yZGVyX3NoaXBfZGF0ZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPG9yZGVyX3NoaXBfZGF5cz4xPC9vcmRlcl9zaGlwX2RheXM+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxvcmRlcl9kZXNjcmlwdGlvbj4lM2N0YWJsZSt3aWR0aCUzZCUyNzEwMCUyNSUyNytib3JkZXIlM2QlMjcxJTI3K2NlbGxzcGFjaW5nJTNkJTI3MCUyNyUzZSUzY3RoZWFkJTNlJTNjdHIlM2UlM2N0ZCt3aWR0aCUzZCUyNzQwJTI1JTI3K2FsaWduJTNkJTI3Y2VudGVyJTI3JTNlJTNjYiUzZVQlYzMlYWFuK2glYzMlYTBuZyUzYyUyZmIlM2UlM2MlMmZ0ZCUzZSUzY3RkK3dpZHRoJTNkJTI3MjAlMjUlMjcrYWxpZ24lM2QlMjdjZW50ZXIlMjclM2UlM2NiJTNlJWM0JTkwJWM2JWExbitnaSVjMyVhMSUzYyUyZmIlM2UlM2MlMmZ0ZCUzZSUzY3RkK3dpZHRoJTNkJTI3MTUlMjUlMjcrYWxpZ24lM2QlMjdjZW50ZXIlMjclM2UlM2NiJTNlUyVlMSViYiU5MStsJWM2JWIwJWUxJWJiJWEzbmclM2MlMmZiJTNlJTNjJTJmdGQlM2UlM2N0ZCt3aWR0aCUzZCUyNzI1JTI1JTI3K2FsaWduJTNkJTI3Y2VudGVyJTI3JTNlJTNjYiUzZVRoJWMzJWEwbmgrdGklZTElYmIlODFuJTNjJTJmYiUzZSUzYyUyZnRkJTNlJTNjJTJmdHIlM2UlM2MlMmZ0aGVhZCUzZSUzY3Rib2R5JTNlJTNjdHIlM2UlM2N0ZCthbGlnbiUzZCUyN2xlZnQlMjclM2VIUCtQYXZpbGlvbitEVjMtMzUwMlRYJTNjJTJmdGQlM2UlM2N0ZCthbGlnbiUzZCUyN3JpZ2h0JTI3JTNlMjMlMmMwMDAlM2MlMmZ0ZCUzZSUzY3RkK2FsaWduJTNkJTI3Y2VudGVyJTI3JTNlMSUzYyUyZnRkJTNlJTNjdGQrYWxpZ24lM2QlMjdyaWdodCUyNyUzZTIzJTJjMDAwJTNjJTJmdGQlM2UlM2MlMmZ0ciUzZSUzY3RyJTNlJTNjdGQrYWxpZ24lM2QlMjdsZWZ0JTI3JTNlRkFOK05vdGVib29rKyhCNCklM2MlMmZ0ZCUzZSUzY3RkK2FsaWduJTNkJTI3cmlnaHQlMjclM2UxMCUyYzAwMCUzYyUyZnRkJTNlJTNjdGQrYWxpZ24lM2QlMjdjZW50ZXIlMjclM2UxJTNjJTJmdGQlM2UlM2N0ZCthbGlnbiUzZCUyN3JpZ2h0JTI3JTNlMTAlMmMwMDAlM2MlMmZ0ZCUzZSUzYyUyZnRyJTNlJTNjdHIlM2UlM2N0ZCthbGlnbiUzZCUyN3JpZ2h0JTI3K2NvbHNwYW4lM2QlMjczJTI3JTNlJTNjYiUzZVQlZTElYmIlOTVuZyt0aSVlMSViYiU4MW4lM2ElM2MlMmZiJTNlJTNjJTJmdGQlM2UlM2N0ZCthbGlnbiUzZCUyN3JpZ2h0JTI3JTNlNDMlMmMwMDAlM2MlMmZ0ZCUzZSUzYyUyZnRyJTNlJTNjdHIlM2UlM2N0ZCthbGlnbiUzZCUyN2xlZnQlMjcrY29sc3BhbiUzZCUyNzQlMjclM2VTb21lK25vdGVzK2Zvcit0aGUrb3JkZXIlM2MlMmZ0ZCUzZSUzYyUyZnRyJTNlJTNjJTJmdGJvZHklM2UlM2MlMmZ0YWJsZSUzZTwvb3JkZXJfZGVzY3JpcHRpb24+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxub3RpZnlfdXJsPmh0dHBzOi8vU2hvcERlbW8vTm90aWZ5LmFzbXg8L25vdGlmeV91cmw+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx2YWxpZGl0eV90aW1lPjIwMTcwNDIwMTU0MDAxPC92YWxpZGl0eV90aW1lPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Y3VzdG9tZXI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxuYW1lPkR1eS5UaGFpPC9uYW1lPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8cGhvbmU+MDkwMzExNzA1NTwvcGhvbmU+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxhZGRyZXNzPjEwMTEgLSBkaWEgY2hpIG5oYTwvYWRkcmVzcz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGNpdHk+NjAwMDA8L2NpdHk+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxlbWFpbD52dS5uZ3V5ZW5AdmlkaWVudHUudm48L2VtYWlsPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2N1c3RvbWVyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc2hvcD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc2hvcHM+PFN0YXRlPlBBWU1FTlRfUkVDRUlWRUQ8L1N0YXRlPjxQYXltZW50TWV0aG9kPkVfV0FMTEVUPC9QYXltZW50TWV0aG9kPjwvUGF5bWVudE5vdGlmaWNhdGlvbj4=</Data><Signature>161F015A91A24D9F5B9DDA92674FB73F1F34CF77476166C747DE54C3D5F222B978488F4DBE387ADBDB853AEAE9A5DE3183E50112F7E93CEE4C2BEB5C67892CAE</Signature><PayooSessionID>skQ1IWQkbnNmzzSx0HYm9jSPqYesiu7gqoYfEwlNMdNJXl71J9PnkKhTwdd53798urqO8oTVdFIqkuUg/D95ZA==</PayooSessionID><KeyFields>PaymentMethod|State|Session|BusinessUsername|ShopID|ShopTitle|ShopDomain|ShopBackUrl|OrderNo|OrderCashAmount|StartShippingDate|ShippingDays|OrderDescription|NotifyUrl|PaymentExpireDate</KeyFields></PayooConnectionPackage>"; 
                // end test

                if (NotifyData == null || "".Equals(NotifyData)) { return null; }
                var payoo = new PayooService(configuration);

                var NotifyPackage = payoo.GetPayooConnectionPackage(NotifyData);

                string strNotifyData = NotifyPackage.Data.Trim();
                string outCheckSum = NotifyPackage.Signature.Trim(); //"4C676A1C9CCB45A8BF28AF445999B342EEA7B39BE28462450078E5908118AC6C1BF3354A8BDC00DA47F88134A75943988AD0F23645CB5B9B05AFB801D19CE26D";// NotifyPackage.Signature.Trim();
                string KeyFields = NotifyPackage.KeyFields.Trim();
                string _msg = String.Empty;


                var invoice = payoo.GetPaymentNotify(strNotifyData);

                // B1. Giải mã kiểm tra thông tin gửi qua server UsExpress hợp lệ
                string generate_check_sum = CallAPI.GenerateCheckSum(StringBuilder(invoice, KeyFields, configuration["payoo:ChecksumKey"]));
                if (outCheckSum == generate_check_sum)
                {
                    // B2. Kiểm tra tín hiệu gửi qua là gì, có phải là đã thanh toán PAYMENT_RECEIVED haykho6ng
                    if (invoice.State.Equals("PAYMENT_RECEIVED", StringComparison.OrdinalIgnoreCase))
                    {
                        orderNo = invoice.OrderNo;

                        // CQ: 11/03/19: Tiến trình thực hiện mua hàng, gọi api sau khi thanh toán thành công với ca1c nha4n
                        // string[] brands =  Repository.getStoreNameById() // { "UAM", "UCC", "UNR", "UHL", "UBB" };
                        //string prefix = invoice.OrderNo.Substring(0, 3);
                        //var store = Repository.getStoreNameByPrefixCode(prefix);
                        //if (store != null)
                        //{
                        // Models.DBContext.Order _ORDER = Repository.GetOrderDetailByCode(invoice.OrderNo);
                        var payment = new PaymentConnection(configuration, url_api, "", -1,  token_tele, group_id_tele, -1);
                        var order_detail = payment.getOrderDetail(orderNo).Result;

                        if (order_detail.PaymentStatus == (int)Payment_Status.CHUA_THANH_TOAN)
                        {
                            #region step 1: PROCESS CALL API US_OLD:                            
                            //1. Push notify kích hoạt bot mua hàng
                            //2. Send SMS
                            //3. Cập nhật trạng thái đơn hàng đã mua bên us old    
                            //4. Gửi email khi đơn hàng có sản phẩm cân nặng = 0 [off]
                            // 5. Push Queue để thực hiện mapping status về us new
                            var result =  payment.UpdatePaymentOrderToUsOld(configuration["url_api_usexpress_old"], orderNo).Result;
                            if (!result)
                            {
                                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR-NEW:ListenerPayoo] _Đã có 1 request gọi lại khi đơn đã thanh toán rồi invoice.OrderNo = " + invoice.OrderNo);
                            }
                            #endregion

                            #region step 2: Cập nhật trạng thái đã thanh toán cho đơn hàng: Process này sẽ được xử lý sau khi active mua tự động bên us old. Push queue để job cs xử lý đồng bộ lại
                            // payment.UpdatePaymentStatus(order_detail.Id);
                            #endregion

                        }
                        else
                        {
                            //ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "[notify] _Đã có 1 request gọi lại khi đơn đã thanh toán rồi invoice.OrderNo = " + invoice.OrderNo);
                            Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR-NEW:ListenerPayoo] _Đã có 1 request gọi lại khi đơn đã thanh toán rồi invoice.OrderNo = " + invoice.OrderNo);
                        }
                        return Content("NOTIFY_RECEIVED");
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR-NEW:ListenerPayoo] Kết quả thanh toán thất bại. Thông tin push" + JsonConvert.SerializeObject(invoice) + "-NotifyData=" + NotifyData);
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR-NEW:ListenerPayoo] Checksum fail. Thông tin outCheckSum:" + outCheckSum + ", generate_check_sum =" + generate_check_sum + "-NotifyData="+ NotifyData);
                }
                return Content("Lỗi xảy ra khi nhận dữ liệu");
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR-NEW:ListenerPayoo] ListenerPayoo error with NotifyData: " + NotifyData + "-- ex = " + ex.ToString());
                return Content("Lỗi xảy ra khi nhận dữ liệu");
            }
        }

        public static string StringBuilder(object APIParam, string KeyFields, string strChecksumKey)
        {
            try
            {
                StringBuilder strData = new StringBuilder();
                strData.Append(strChecksumKey);
                string[] arrKeyFields = KeyFields.Split('|');
                foreach (string strKey in arrKeyFields)
                {
                    strData.Append("|" + APIParam.GetType().GetProperty(strKey).GetValue(APIParam, null));
                }
                return strData.ToString();
            }
            catch (Exception ex)
            {
                //   ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "StringBuilder()", "(" + KeyFields + "):" + ex.ToString());
                return string.Empty;
            }
        }

    }
}