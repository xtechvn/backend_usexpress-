using AppMappingOrder.ViewModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Telegram.Bot.Types.Payments;
using Utilities;
using Utilities.Contants;
using static Utilities.Contants.Constants;

namespace AppMappingOrder
{
    /// <summary>
    /// App này sẽ lấy dữ liệu từ hệ thống cũ về để update db
    /// </summary>
    class Program
    {
        public static string QUEUE_HOST = ConfigurationManager.AppSettings["QUEUE_HOST"];
        public static string QUEUE_V_HOST = ConfigurationManager.AppSettings["QUEUE_V_HOST"];
        public static string QUEUE_USERNAME = ConfigurationManager.AppSettings["QUEUE_USERNAME"];
        public static string QUEUE_PASSWORD = ConfigurationManager.AppSettings["QUEUE_PASSWORD"];
        public static string QUEUE_PORT = ConfigurationManager.AppSettings["QUEUE_PORT"];
        public static string QUEUE_KEY_API = ConfigurationManager.AppSettings["QUEUE_KEY_API"];
        public static string EncryptApi = ConfigurationManager.AppSettings["EncryptApi"];
        public static string url_usexpress_old = ConfigurationManager.AppSettings["domain_us_old"];
        public static string domain_us_api_new = ConfigurationManager.AppSettings["domain_us_api_new"];
        public static string domain_order_detail = ConfigurationManager.AppSettings["domain_order_detail"];
        private static string task_queue = TaskQueueName.order_old_convert_queue;

        static void Main(string[] args)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = QUEUE_HOST,
                    UserName = QUEUE_USERNAME,
                    Password = QUEUE_PASSWORD,
                    VirtualHost = QUEUE_V_HOST,
                    Port = Protocols.DefaultProtocol.DefaultPort
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: task_queue,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        #region Get data from Queue & Analys
                        //var order = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(message);
                        var order = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                        if (order.Count > 0)
                        {
                            // string order_no = (order[0].FirstOrDefault(x => x.Key == "order_no").Value.ToString());
                            string order_no = order["order_no"].Trim().ToString();
                            // get detail order: order,orderitem
                            var order_detail = mappingOrder(order_no);
                        }
                        else
                        {
                            // writelog tele
                            LogHelper.InsertLogTelegram("[Warning-job-process-order]" + message + " is valid !!!!");
                        }
                        #endregion

                        Console.WriteLine("[" + DateTime.Now.ToString() + "] - [x] Process Success - Data Received {0}", message);

                        //   int dots = message.Split('.').Length - 1;
                        //  Thread.Sleep(dots * 1000);

                        //   Console.WriteLine(" [x] Done");

                        // Note: it is possible to access the channel via
                        //   ((EventingBasicConsumer)sender).Model here
                        //channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    channel.BasicConsume(queue: task_queue,
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("execute Queue error: " + ex.ToString());
                Console.ReadLine();
            }
        }

        private static bool mappingOrder(string order_no)
        {
            string order_info = string.Empty;
            try
            {
                #region get order detail by us old
                string url_api_us_old = url_usexpress_old + "ApiUsexpress/getOrderDetailUsexpress";
                string j_param = "{'order_no':'" + order_no + "'}";
                string token = CommonHelper.Encode(j_param, EncryptApi);

                string response_api_order = connectApi(url_api_us_old, token);

                if (response_api_order == string.Empty) return false;

                JArray JsonParent = JArray.Parse("[" + response_api_order + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");

                if (order_status.ToLower() != "success")
                {
                    string msg = JsonParent[0]["msg"].ToString().Replace("\"", "");
                    LogHelper.InsertLogTelegram("Don hang: " + order_no + " xay ra loi khi get tu US OLD: " + msg + " ==> token =  " + token + "=> enpoint: " + url_api_us_old);
                    return false;
                }
                else
                {
                    order_info = JsonParent[0]["order"].ToString();
                }

                var order_map = JsonConvert.DeserializeObject<OrderEntities>(order_info.ToString());
                #endregion

                var response_push_order = PushOrderOldDetail(order_map);

                return response_push_order;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("mappingOrder Don hang: " + order_no + "that bai: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Push order to us news
        /// </summary>
        /// <param name="order_no"></param>
        private static bool PushOrderOldDetail(OrderEntities order_map)
        {
            string token = string.Empty;
            try
            {
                var order = order_map.obj_order;
                var order_item = order_map.obj_order_item;
                var list_order_item_model = new List<OrderItemViewModel>();
                var product_model = new List<ProductViewModel>();
                var image_product_model = new List<ImageProductViewModel>();                

                #region INPUT
                var order_model = new OrderViewModel
                {
                    ClientId = order.UserId, //clientid ben us old
                    
                    UserId = -1,
                    LabelId = (short)order.StoreId,
                    OrderNo = order.code,
                    ClientName = order.CustomerName,
                    Email = order.Email,
                    Phone = order.Phone,
                    Address = order.Address,
                    CreatedOn = Convert.ToDateTime(order.CreatedDate),
                    RateCurrent = order.rate,
                    PriceVnd = order.TotalPriceVND,
                    AmountVnd = order.TotalPriceVND,

                    TotalDiscount2ndUsd = Convert.ToDouble(order.TotalDiscount2ndUsd),
                    TotalShippingFeeUsd = order.TotalShippingFeeUsd,

                    TotalDiscount2ndVnd = order.TotalDiscount2ndVnd,
                    TotalShippingFeeVnd = order.TotalShippingFeeVnd,
                    TotalDiscountVoucherVnd = order.TotalDiscountVoucherVnd,

                    UtmMedium = order.UtmMedium,
                    UtmCampaign = order.UtmCampaign,
                    UtmSource = order.UtmSource,
                    UtmFirstTime = order.UtmFirstTime,

                    Voucher = order.voucherCode,
                    Discount = 0, // chiet khau % cua don hang quy doi
                    PriceUsd = order.TotalPrice,
                    AmountUsd = order.TotalPriceSales,

                    Note = order.Note,
                    PaymentType = (short)order.Paymentype,
                    PaymentStatus = (short)order.PaymentStatus,
                    PaymentDate = order.payment_date == string.Empty ? Convert.ToDateTime("2000-01-01 00:00:00.000") : Convert.ToDateTime(order.payment_date),
                    OrderStatus = order.Status
                };

                // create product
                var variations = new Dictionary<string, string>();
                int SalesRank = 0;
                bool IsEligibleForPrime = false;
                foreach (var item in order_item)
                {
                    if (item.JProductData != string.Empty)
                    {
                        var product_detail = JsonConvert.DeserializeObject<ProductMappingModel>(item.JProductData);

                        variations = new Dictionary<string, string>
                        {
                            { "color",product_detail.Color ?? ""},
                            { "size",product_detail.size ?? ""},
                        };
                        SalesRank = string.IsNullOrEmpty(product_detail.SalesRank) ? 0 : Convert.ToInt32(product_detail.SalesRank);
                        IsEligibleForPrime = product_detail.IsEligibleForPrime;
                    }

                    var ImageProduct = new ImageProductViewModel
                    {
                        ProductMapId = item.OrderItemId,
                        Image = item.ProductImage ?? string.Empty
                    };
                    image_product_model.Add(ImageProduct);

                    var model = new ProductViewModel
                    {
                        product_code = item.AmazoneItemId,
                        product_map_id = item.OrderItemId,
                        product_name = item.ProductName.Replace("'", ""),
                        price = Convert.ToDouble(item.PriceAmazon + item.ShippingUs),
                        discount = 0,
                        amount = Convert.ToDouble(item.PriceAmazon),
                        rating = "0",
                        manufacturer = item.SellerName.Replace("'", ""),
                        seller_name = item.SellerName.Replace("'", ""),
                        label_id = order.StoreId,
                        reviews_count = SalesRank,
                        is_prime_eligible = IsEligibleForPrime,
                        rate = Convert.ToDouble(order.rate),
                        seller_id = item.SellerId,
                        variations = string.Empty //variations.Count == 0 ? string.Empty : CommonHelper.Encode(JsonConvert.SerializeObject(variations), EncryptApi)
                    };
                    product_model.Add(model);                   

                    var order_item_model = new OrderItemViewModel
                    {
                        ProductMapId = item.OrderItemId,
                        //Price = item.OriginUnitPrice, // gia san pham tai thoi diem mua đã nhân số lương. Giá này đã được trừ sau các khỏa khuyến mại
                        DiscountShippingFirstPound = Convert.ToDouble(item.DiscountShippingFirstPound),
                        Price = Convert.ToDouble(item.PriceAmazon),
                        ProductCode = item.AmazoneItemId,
                        ProductImage = item.ProductName.Replace("'", ""),
                        FirstPoundFee = item.ShippingFirstPound,
                        NextPoundFee = (order.StoreId == (int)LabelType.bestbuy) ? item.ShippingPound : item.ShippingProcess,
                        ShippingFeeUs = item.ShippingUs,
                        LuxuryFee = item.ShippingLuxury,
                        Quantity = item.Quantity,
                        Weight = item.ShippingPound

                    };
                    list_order_item_model.Add(order_item_model);
                }

                string data_order = "{'order_info': '" + JsonConvert.SerializeObject(order_model) +
                    "','list_order_item_info': '" + JsonConvert.SerializeObject(list_order_item_model) +
                      "','list_product_info': '" + JsonConvert.SerializeObject(product_model) +
                      "','list_note_order': '" + "" +
                      "','list_image_product': '" + JsonConvert.SerializeObject(image_product_model) + "'}";


                token = CommonHelper.Encode(data_order, EncryptApi);
                #endregion


                string url_api_push_order = domain_us_api_new + "api/order/addNewOrder";
                string response_api_order = connectApi(url_api_push_order, token);
                JArray JsonParent = JArray.Parse("[" + response_api_order + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");

                if (order_status.ToLower() == "success")
                {
                    string order_id_response = JsonParent[0]["order_id_response"] == null ? "" : JsonParent[0]["order_id_response"].ToString().Replace("\"", "");
                    string msg = string.Empty;
                    string payment_name = string.Empty;
                    switch (order.Paymentype)
                    {
                        case (int)PaymentType.USEXPRESS_BANK:
                            payment_name = "CHUYỂN KHOẢN TRỰC TIẾP";
                            break;
                        case (int)PaymentType.ATM_PAYOO_PAY:
                            payment_name = "BANK - PAYOO";
                            break;
                        case (int)PaymentType.VISA_PAYOO_PAY:
                            payment_name = "VISA - PAYOO";
                            break;
                    }

                    if (order.PaymentStatus == 0)
                    {
                        // msg = "Đơn hàng " + order.code + " đã chuyển khoản thành công qua hình thức " + payment_name + " bởi tài khoản " + order.Email + ". Số tiền chuyển: " + (order.TotalPriceVND ?? 0).ToString("N0") + " . Chuyển lúc: " + order.payment_date;
                        msg = "Thông tin đơn hàng " + order.code + " đã được thay đổi. Hình thức thanh toán:" + payment_name + " - Trạng thái đơn hàng: " + order.StatusName + " - Số tiền chuyển: " + (order.TotalPriceVND ?? 0).ToString("N0") ;
                        msg += " - Thời gian chuyển: " + order.payment_date.ToString()+  " - Email chuyển: " + order.Email.ToString();
                    }
                    else if (order.TotalChangePayment > 1)
                    {
                        string note_help_me = string.Empty;
                        if (order.TotalChangePayment == 2)
                        {
                            note_help_me = "PROPBLEM-PAYMENT";
                        }
                        else if (order.TotalChangePayment > 2 && order.TotalChangePayment < 5)
                        {
                            note_help_me = "CAN YOU HELP ME";
                        }
                        else if (order.TotalChangePayment >= 5)
                        {
                            note_help_me = "WARNING-SPAM";
                        }
                        msg = "[" + note_help_me + "] Đơn hàng " + order.code + " đã thay đổi hình thức thanh toán sang " + payment_name + " bởi tài khoản " + order.Email + " với số tiền: " + (order.TotalPriceVND ?? 0).ToString("N0");
                    }
                    else
                    {
                        string link_detail_order = ". Xem chi tiết đơn tại link " + domain_order_detail + "/order/detail/" + order_id_response;
                        msg = "Đơn hàng " + order.code + " tạo mới thành công qua hình thức thanh toán: " + payment_name + " bởi tài khoản " + order.Email + " với số tiền: " + (order.TotalPriceVND ?? 0).ToString("N0") + link_detail_order;
                    }


                    LogHelper.InsertLogTelegramByUrl(LogHelper.botToken_monitor_order, LogHelper.group_id_monitor_order, msg);
                    Console.WriteLine("Order : " + order.code + " Create new Successfully");
                    return true;
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString().Replace("\"", "");
                    LogHelper.InsertLogTelegram("Order : " + order.code + " push that bai qua endpoint: " + url_api_push_order + ". Msg =" + msg + ". Token = " + token);
                    return false;
                }
            }
            catch (Exception ex)
            {
                string data_error = JsonConvert.SerializeObject(order_map);
                LogHelper.InsertLogTelegram("PushOrderOldDetail: " + ex.ToString() + " --> order_data = " + data_error);
                return false;
            }
        }

        private static string connectApi(string url, string token)
        {
            try
            {
                string _post = "token=" + token;
                byte[] byteArray = Encoding.UTF8.GetBytes(_post);
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string response_api_order = reader.ReadToEnd();

                return response_api_order;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" connectApi: " + ex.ToString());
                return string.Empty;
            }
        }

    }
}
