using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Aspose.Cells;
using Caching.RedisWorker;
using Entities.ViewModels;
using Entities.ViewModels.Orders;
using Entities.ViewModels.ServicePublic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.MongoRepositories.Product;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Models;
using WEB.CMS.Models.Order;
using WEB.CMS.Models.RecentActivity;
using static Utilities.Contants.Constants;

namespace WEB.CMS.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly IClientRepository _ClientRepository;
        private readonly IOrderRepository _OrderRepository;
        private readonly IConfiguration _Configuration;
        private readonly RedisConn _redisService;
        private readonly IProductRepository _ProductRepository;


        public DashBoardController(IClientRepository clientRepository, IProductRepository productRepository,
           IOrderRepository orderRepository, IConfiguration configuration, RedisConn redisService)
        {
            _ClientRepository = clientRepository;
            _OrderRepository = orderRepository;
            _Configuration = configuration;
            _redisService = redisService;
            _ProductRepository = productRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// thong ke so khach hang dang ky trong ngay
        /// </summary>
        /// <returns></returns>
        public IActionResult CustomerInDay()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetCustomerInDay()
        {
            var totalCLient = _ClientRepository.GetTotalClientInDay();
            var totalReturningClient = _OrderRepository.GetTotalReturningClientInDay().Result;
            var totalPaymentClient = _OrderRepository.GetTotalPaymentClientInDay().Result;
            return new JsonResult(new
            {
                total_new_client = totalCLient,
                total_returning_client = totalReturningClient,
                total_payment_client = totalPaymentClient
            });
        }

        /// <summary>
        /// thong ke doanh thu cua ngay hien tai
        /// </summary>
        /// <returns></returns>
        public IActionResult RevenuToday()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetRevenuToday()
        {
            var revenueModel = _OrderRepository.SummaryRevenuToday();
            return new JsonResult(new
            {
                Data = revenueModel,
                Message = "Thành công"
            });
        }

        [HttpPost]
        public IActionResult GetTotalErrorOrder()
        {
            return new JsonResult(new
            {
                Data = _OrderRepository.GetTotalErrorOrderCount(),
                Message = "Thành công"
            });
        }

        /// <summary>
        /// compare doanh thu cua ngay hien tai voi ngay hom truoc
        /// </summary>
        /// <returns></returns>
        public IActionResult RevenuDay()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetRevenuDay()
        {
            var percent = _OrderRepository.GetRevenuDay();
            return new JsonResult(new
            {
                Data = percent,
                Message = "Thành công"
            });
        }

        /// <summary>
        /// lay danh sach cac don chua thanh toan
        /// </summary>
        /// <returns></returns>
        public IActionResult RevenuTemp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetRevenuTemp()
        {
            var revenueModel = _OrderRepository.SummaryRevenuTodayTemp();
            return new JsonResult(new
            {
                Data = revenueModel,
                Message = "Thành công"
            });
        }

        /// <summary>
        /// compare ty le miss crawl
        /// </summary>
        /// <returns></returns>
        public IActionResult CrawlPercent()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCrawlPercent()
        {
            long data = 0;
            int status = -1;
            string message = "Success";
            try
            {
                ProductFilterModel model = new ProductFilterModel()
                {
                    FromDate = DateTime.Today,
                    ToDate = DateTime.Today,
                    PageIndex = 1,
                    PageSize = 5,
                };
                var result = await _ProductRepository.GetProductPagingList(model);
                data = result.TotalRecord;
                status = 0;
            }
            catch (Exception ex)
            {
                message = "Failed";
                LogHelper.InsertLogTelegram("GetCrawlPercent - DashBoardController: " + ex.ToString());
            }
            return new JsonResult(new
            {
                status = status,
                data = data,
                message = message
            });
        }

        public IActionResult NotFound()
        {
            return View();
        }

        public IActionResult ChartRevenu()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetChartRevenu(int revenuType)
        {
            var listDataChart = new List<ChartRevenuViewModel>();
            var listDataChartAgo = new List<ChartRevenuViewModel>();
            try
            {
                OrderSearchModel searchModel = new OrderSearchModel();

                var toDate = DateTime.Now.Date.AddDays(-1);

                if (revenuType == (int)Constants.Chart_Revenu_Type.Week || revenuType == 0)
                {
                    //lay doanh thu 7 ngay truoc do - tinh tu truoc ngay hien tai 1 ngay
                    searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                    searchModel.FromDate = toDate.AddDays(-6).ToString("dd/MM/yyyy");
                    listDataChart = _OrderRepository.GetRevenuByDateRange(searchModel);
                    var fromDated = DateUtil.StringToDate(searchModel.FromDate);
                    var toDated = DateUtil.StringToDate(searchModel.ToDate);
                    for (var dateRunner = fromDated; dateRunner <= toDated; dateRunner = dateRunner.Value.AddDays(1))
                    {
                        if (listDataChart.FirstOrDefault(n => n.Date == dateRunner) == null)
                        {
                            ChartRevenuViewModel chartRevenuViewModel = new ChartRevenuViewModel();
                            chartRevenuViewModel.Date = dateRunner;
                            chartRevenuViewModel.OrderCount = 0;
                            chartRevenuViewModel.TotalRevenu = 0;
                            chartRevenuViewModel.TotalShipFee = 0;
                            listDataChart.Add(chartRevenuViewModel);
                        }
                    }

                    //lay 7 ngay doanh thu tuong ung voi 7 ngày tinh tu hien tai
                    var toDateAgo = toDate.AddDays(-7);
                    searchModel.ToDate = toDateAgo.ToString("dd/MM/yyyy");
                    searchModel.FromDate = toDateAgo.AddDays(-6).ToString("dd/MM/yyyy");
                    listDataChartAgo = _OrderRepository.GetRevenuByDateRange(searchModel, false);
                    var fromDateAgo = DateUtil.StringToDate(searchModel.FromDate);
                    var toDatedAgo = DateUtil.StringToDate(searchModel.ToDate);
                    //for (var dateRunner = fromDateAgo; dateRunner <= toDatedAgo; dateRunner = dateRunner.Value.AddDays(1))
                    //{
                    //    if (listDataChartAgo.FirstOrDefault(n => n.Date == dateRunner) == null)
                    //    {
                    //        ChartRevenuViewModel chartRevenuViewModel = new ChartRevenuViewModel();
                    //        chartRevenuViewModel.Date = dateRunner;
                    //        chartRevenuViewModel.OrderCount = 0;
                    //        chartRevenuViewModel.TotalRevenu = 0;
                    //        listDataChartAgo.Add(chartRevenuViewModel);
                    //    }
                    //}
                }

                if (revenuType == (int)Constants.Chart_Revenu_Type.Month)
                {
                    //lay doanh thu 1 thang qua
                    searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                    searchModel.FromDate = toDate.AddDays(-29).ToString("dd/MM/yyyy");
                    listDataChart = _OrderRepository.GetRevenuByDateRange(searchModel);
                    var fromDated = DateUtil.StringToDate(searchModel.FromDate);
                    var toDated = DateUtil.StringToDate(searchModel.ToDate);

                    //lay 30 ngay doanh thu tuong ung voi 30 ngày tinh tu hien tai
                    var toDateAgo = toDate.AddDays(-30);
                    searchModel.ToDate = toDateAgo.ToString("dd/MM/yyyy");
                    searchModel.FromDate = toDateAgo.AddDays(-29).ToString("dd/MM/yyyy");
                    listDataChartAgo = _OrderRepository.GetRevenuByDateRange(searchModel, false);
                    var fromDateAgo = DateUtil.StringToDate(searchModel.FromDate);
                    var toDatedAgo = DateUtil.StringToDate(searchModel.ToDate);
                }

                var listDataChartSummary = new List<ChartRevenuViewModel>();

                foreach (var item in listDataChart)
                {
                    ChartRevenuViewModel chartRevenuViewModel = new ChartRevenuViewModel();
                    chartRevenuViewModel.Date = item.Date;
                    chartRevenuViewModel.TotalRevenu = item.TotalRevenu;
                    chartRevenuViewModel.TotalShipFee = item.TotalShipFee;
                    chartRevenuViewModel.OrderCount = item.OrderCount;
                    chartRevenuViewModel.StoreName = item.StoreName;
                    var date = item.Date.Value.AddDays(-7);
                    if (revenuType == (int)Constants.Chart_Revenu_Type.Week || revenuType == 0)
                    {
                        var revenuModelAgo = listDataChartAgo.FirstOrDefault(n => n.DatePass.Value.Date == date.Date);
                        if (revenuModelAgo != null)
                        {
                            chartRevenuViewModel.DatePass = revenuModelAgo.DatePass;
                            chartRevenuViewModel.TotalRevenuPass = revenuModelAgo.TotalRevenuPass;
                            chartRevenuViewModel.OrderCountPass = revenuModelAgo.OrderCountPass;
                            chartRevenuViewModel.TotalShipFeePass = revenuModelAgo.TotalShipFeePass;
                            chartRevenuViewModel.StoreNamePass = revenuModelAgo.StoreNamePass;
                        }
                    }
                    else
                    {
                        var revenuModelAgo = listDataChartAgo.FirstOrDefault(n => n.DatePass == item.Date.Value.AddDays(-30));
                        if (revenuModelAgo != null)
                        {
                            chartRevenuViewModel.DatePass = revenuModelAgo.DatePass;
                            chartRevenuViewModel.TotalRevenuPass = revenuModelAgo.TotalRevenuPass;
                            chartRevenuViewModel.OrderCountPass = revenuModelAgo.OrderCountPass;
                            chartRevenuViewModel.TotalShipFeePass = revenuModelAgo.TotalShipFeePass;
                            chartRevenuViewModel.StoreNamePass = revenuModelAgo.StoreNamePass;
                        }
                    }
                    listDataChartSummary.Add(chartRevenuViewModel);
                }

                return new JsonResult(new
                {
                    DataChart = listDataChartSummary.OrderBy(n => n.Date).ToList(),
                    TotalRevenu = listDataChartSummary.Sum(n => n.TotalShipFee).ToString("N0"),
                    Message = "Thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetChartRevenu - DashBoardController: " + ex);
                return new JsonResult(new
                {
                    DataWeek = new List<ChartRevenuViewModel>(),
                    TotalRevenu = 0,
                    Message = "Có lỗi ngoại lệ"
                });
            }
        }

        public IActionResult ChartLabel()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetChartLabel(int revenuType, int revenuChartType)
        {
            var listDataChart = new List<ChartRevenuViewModel>();
            try
            {
                OrderSearchModel searchModel = new OrderSearchModel();
                var toDate = DateTime.Now.Date.AddDays(-1);

                if (revenuChartType == (int)Constants.Chart_Type_Label.Revenu)
                {
                    switch (revenuType)
                    {
                        case (int)Constants.Chart_Label_Type.Today:
                            searchModel.ToDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            searchModel.FromDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelRevenuByDateRange(searchModel);
                            break;
                        case (int)Constants.Chart_Label_Type.Yesterday:
                            searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                            searchModel.FromDate = toDate.ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelRevenuByDateRange(searchModel);
                            break;
                        case (int)Constants.Chart_Label_Type.Week:
                            //lay doanh thu 7 ngay qua
                            searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                            searchModel.FromDate = toDate.AddDays(-6).ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelRevenuByDateRange(searchModel);
                            break;
                        case (int)Constants.Chart_Label_Type.Month:
                            //lay doanh thu 1 thang qua
                            searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                            searchModel.FromDate = toDate.AddDays(-29).ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelRevenuByDateRange(searchModel);
                            break;
                        default:
                            searchModel.ToDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            searchModel.FromDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelRevenuByDateRange(searchModel);
                            break;
                    }
                }
                if (revenuChartType == (int)Constants.Chart_Type_Label.Quantity)
                {
                    switch (revenuType)
                    {
                        case (int)Constants.Chart_Label_Type.Today:
                            searchModel.ToDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            searchModel.FromDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelQuantityByDateRange(searchModel);
                            break;
                        case (int)Constants.Chart_Label_Type.Yesterday:
                            searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                            searchModel.FromDate = toDate.ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelQuantityByDateRange(searchModel);
                            break;
                        case (int)Constants.Chart_Label_Type.Week:
                            //lay doanh thu 7 ngay qua
                            searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                            searchModel.FromDate = toDate.AddDays(-6).ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelQuantityByDateRange(searchModel);
                            break;
                        case (int)Constants.Chart_Label_Type.Month:
                            //lay doanh thu 1 thang qua
                            searchModel.ToDate = toDate.ToString("dd/MM/yyyy");
                            searchModel.FromDate = toDate.AddDays(-29).ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelQuantityByDateRange(searchModel);
                            break;
                        default:
                            searchModel.ToDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            searchModel.FromDate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            listDataChart = _OrderRepository.GetLabelQuantityByDateRange(searchModel);
                            break;
                    }
                }

                return new JsonResult(new
                {
                    DataChart = listDataChart,
                    Message = "Thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetChartRevenu - DashBoardController: " + ex);
                return new JsonResult(new
                {
                    DataChart = new List<ChartRevenuViewModel>(),
                    Message = "Có lỗi ngoại lệ"
                });
            }
        }

        [HttpGet]
        public IActionResult ExportChartRevenu()
        {
            Workbook workbook = new Workbook();
            return View();
        }

        [HttpGet]
        public IActionResult ExportChartLabelRevenu()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> GetOrderLogActivityToday()
        //{
        //    try
        //    {
        //        OrderLogActivity orderLogActivity = new OrderLogActivity(_Configuration);
        //        int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_common"]);
        //        _redisService.Connect();
        //        var cacheOrderLogActivity = "";
        //        try
        //        {
        //            cacheOrderLogActivity = await _redisService.GetAsync(CacheType.KEY_ORDER_LOG_ACTIVITY, db_index);
        //        }
        //        catch (Exception)
        //        { cacheOrderLogActivity = null; }
        //        var listOrderLogActivity = new List<OrderLogActivityViewModel>();
        //        if (!string.IsNullOrEmpty(cacheOrderLogActivity))
        //        {
        //            listOrderLogActivity = JsonConvert.DeserializeObject<List<OrderLogActivityViewModel>>(cacheOrderLogActivity);
        //        }
        //        //listOrderLogActivity = new List<OrderLogActivityViewModel>();
        //        if (listOrderLogActivity.Count == 0)
        //        {
        //            listOrderLogActivity = orderLogActivity.getOrderLogActivityToday();
        //            _redisService.Set(CacheType.KEY_ORDER_LOG_ACTIVITY,
        //                JsonConvert.SerializeObject(listOrderLogActivity.Select(n => new LogOrderActivityModel
        //                {
        //                    id = n.id,
        //                    order_no = n.order_no,
        //                    create_date = n.create_date,
        //                    email_client = n.email_client,
        //                    email_user = n.email_user,
        //                    status = n.status,
        //                    payment_type = n.payment_type,
        //                    amount = n.amount,
        //                })), db_index);
        //        }
        //        listOrderLogActivity.ForEach(o =>
        //        {
        //            var now = DateTime.Now;
        //            var create_date = DateUtil.UnixTimeStampToDateTime(o.create_date);
        //            o.day = (int)(DateTime.Now.Subtract(create_date).TotalMinutes);
        //            o.hour = create_date.Hour;
        //            o.minute = create_date.Minute;
        //            o.time = GetTime(o.day);
        //            o.notify_name = GetStatusName(o.status, o.order_no, o.amount);
        //            o.orderId = _OrderRepository.FindOrderIdByOrderNo(o.order_no).Result;
        //        });
        //        return new JsonResult(new
        //        {
        //            Data = listOrderLogActivity,
        //            Message = "Thành công"
        //        });
        //    }
        //    catch (Exception)
        //    {
        //        return new JsonResult(new
        //        {
        //            Data = new List<OrderLogActivityViewModel>(),
        //            Message = "Có lỗi xảy ra. Vui lòng báo quản trị viên"
        //        });
        //    }
        //}

        [HttpPost]
        public IActionResult GetOrderLogActiviry(int pageIndex = 1, int pageSize = 10)
        {
            OrderLogActivity orderLogActivity = new OrderLogActivity(_Configuration);
            var data = orderLogActivity.getOrderLogActivity(pageIndex, pageSize);
            return new JsonResult(new
            {
                Data = data,
                Message = "Thành công"
            });
        }
        [HttpPost]
        public async Task<string> GetOrderShippingLogToday(int filter=0)
        {
            List<OrderLogShippingDateViewModel> result = new List<OrderLogShippingDateViewModel>();
            try
            {
                var orders =  _OrderRepository.GetOrderShippingLogToday();
                if (orders != null)
                {
                    foreach (var item in orders)
                    {
                       
                        switch (item.OrderStatus)
                        {
                            case 6:
                                {
                                    item.exprire_day_count = (item.LastestOrderProgressDay - 6) <= 0 ? 0 : (item.LastestOrderProgressDay - 6);

                                }
                                break;
                            case 13:
                                {
                                    item.exprire_day_count = (item.LastestOrderProgressDay - 1) <= 0 ? 0 : (item.LastestOrderProgressDay - 1);

                                }
                                break;
                            case 7:
                                {
                                    item.exprire_day_count = (item.LastestOrderProgressDay - 7) <= 0 ? 0 : (item.LastestOrderProgressDay - 7);

                                }
                                break;
                            case 10:
                                {
                                    item.exprire_day_count = (item.LastestOrderProgressDay - 5) <= 0 ? 0 : (item.LastestOrderProgressDay - 5);

                                }
                                break;
                            case 11:
                                {
                                    item.exprire_day_count = (item.LastestOrderProgressDay - 5) <= 0 ? 0 : (item.LastestOrderProgressDay - 5);

                                }
                                break;
                            case 16:
                                {
                                    item.exprire_day_count = (item.LastestOrderProgressDay - 7) <= 0 ? 0 : (item.LastestOrderProgressDay - 7);
                                }
                                break;
                            default:
                                {

                                }
                                break;
                        }
                        result.Add(item);
                    }
                    result = result.Where(x => x.exprire_day_count > 0 || (x.TotalOrderProgressDay - 14)>0).ToList();
                    switch (filter)
                    {
                        case 0: // Mặc định theo ngày tạo đơn
                            {
                                
                            }
                            break;
                        case 1: // Đơn hàng thanh toán quá 1 ngày
                            {
                                result = result.Where(s => s.OrderStatus==(int)OrderStatus.PAID_ORDER).ToList();
                            }
                            break;
                        case 2: // Đơn hàng đã mua quá 5 ngày
                            {
                                result = result.Where(s => s.OrderStatus == (int)OrderStatus.BOUGHT_ORDER).ToList();
                            }
                            break;
                        case 3: // Đơn hàng về VN nhưng chưa đến tay khách quá 7 ngày
                            {
                                List<int> status_check = new List<int>() { (int)OrderStatus.CLIENT_TRANSPORT_ORDER, (int)OrderStatus.VN_TRANSPORT_ORDER, (int)OrderStatus.VN_STORAGE_ORDER };
                                result = result.Where(s => status_check.Contains(s.OrderStatus) ).ToList();
                            }
                            break;
                        case 4: // Tất cả đơn nhãn amz
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.amazon).ToList();
                            }
                            break;
                        case 5: // Tất cả đơn mã costco
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.costco).ToList();
                            }
                            break;
                        case 6:  // Tất cả đơn mã bestbuy
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.bestbuy).ToList();
                            }
                            break;
                        case 7: // Tất cả đơn mã hautelook
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.hautelook).ToList();
                            }
                            break;
                        case 8: // Tất cả đơn mã jomashop
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.jomashop).ToList();
                            }
                            break;
                        case 9: // Tất cả đơn mã nordstromrack
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.nordstromrack).ToList();
                            }
                            break;
                        case 10: // Tất cả đơn mã sephora
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.sephora).ToList();
                            }
                            break;
                        case 11: // Tất cả đơn mã victoria_secret
                            {
                                result = result.Where(s => s.LabelId == (int)LabelType.victoria_secret).ToList();
                            }
                            break;
                        case 12: // Đơn hàng chưa về kho việt nam
                            {
                                result = result.Where(s => s.OrderStatus == (int)OrderStatus.VN_TRANSPORT_ORDER).ToList();
                            }
                            break;
                        case 13: // Đơn hàng chưa về kho việt nam
                            {
                                result = result.Where(s => s.OrderStatus == (int)OrderStatus.VN_TRANSPORT_STORAGE_ORDER).ToList();
                            }
                            break;
                        default:
                            {
                            }
                            break;

                    }
                    result = result.OrderByDescending(x => x.exprire_day_count).ToList();
                }

            } catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderShippingLogToday - DashBoardController: " + ex.ToString());

            }
            return JsonConvert.SerializeObject(result);
        }
        public string GetTime(double minute)
        {
            //if (minute < 0) minute = -minute;
            string time = "";
            double minutes = 0;

            if (minute < 60)
            {
                time = minute + " phút trước";
            }

            if (minute > 60 && minute < 1440)
            {
                minutes = minute % 60;
                time = (int)(minute / 60) + " giờ " + (minutes > 0 ? minutes + " phút trước" : " trước");
            }

            if (minute > 1440 && minute < 43200)
            {
                minutes = minute % 1440;
                time = (int)(minute / 1440) + " ngày " + (minutes > 0 ? minutes + " giờ trước" : " trước");
            }

            if (minute > 43200)
            {
                minutes = minute % 43200;
                time = (int)(minute / 43200) + " tháng " + (minute > 0 ? minutes + " ngày trước" : " trước");
            }

            return time;
        }

        public string GetStatusName(int status, string order_no, double amount)
        {
            string name = " đã chuyển trạng thái";
            switch (status)
            {
                case (int)Constants.OrderStatus.CREATED_ORDER:
                    name = " đã được khởi tạo thành công";
                    break;
                case (int)Constants.OrderStatus.SUCCEED_ORDER:
                    name = " đã hoàn thành";
                    break;
                case (int)Constants.OrderStatus.CANCEL_ORDER:
                    name = " đã bị hủy";
                    break;
                case (int)Constants.OrderStatus.BOUGHT_ORDER:
                    name = " đã được mua thành công";
                    break;
                case (int)Constants.OrderStatus.CLIENT_TRANSPORT_ORDER:
                    name = " đang được giao cho khách hàng";
                    break;
                case (int)Constants.OrderStatus.VN_TRANSPORT_ORDER:
                    name = " đang được chuyển về Việt Nam";
                    break;
                case (int)Constants.OrderStatus.BUY_FAILED_ORDER:
                    name = " đã mua thất bại";
                    break;
                case (int)Constants.OrderStatus.PAID_ORDER:
                    name = " đã thanh toán thành công. Số tiền thanh toán là: " + amount.ToString("N0") + " vnđ";
                    break;
                case (int)Constants.OrderStatus.VN_STORAGE_ORDER:
                    name = " đã lưu kho Việt Nam";
                    break;
            }
            return name;
        }

        public IActionResult RecentActivity()
        {
            return View();
        }

        public string GetAnswerSurvery(int page_index, int page_size)
        {
            List<RecentActivityLog> result = new List<RecentActivityLog>();
            try
            {
                if (page_index > 0 && page_size > 0)
                {
                    AnswerSurvey answerSurveries = new AnswerSurvey(_Configuration);
                    List<AnswerSurveryViewModel> survey_list = answerSurveries.GetAnswerSurveryPagnition(page_index, page_size);
                    foreach (var survey in survey_list)
                    {
                        string create_on_str = survey.CreateOn.Day + "/" + survey.CreateOn.Month + "/" + survey.CreateOn.Year + " " + survey.CreateOn.Hour + "h " + survey.CreateOn.Minute + " phút";
                        double compare = (DateTime.Now - survey.CreateOn).TotalSeconds;
                        var time_from_str = "Hôm nay";
                        string function_name = "Trang chủ";
                        if (compare > 60 * 60 * 24)
                        {
                            int days = (int)compare / (60 * 60 * 24);
                            time_from_str = days + " ngày trước";
                        }
                        switch (survey.FuntionId)
                        {
                            case "1":
                                function_name = "Trang chủ"; break;
                            case "2":
                                function_name = "Tìm kiếm sản phẩm"; break;
                            case "3":
                                function_name = "Thông tin sản phẩm"; break;
                            case "4":
                                function_name = "Giỏ hàng"; break;
                            case "5":
                                function_name = "Trang thanh toán"; break;
                            case "6":
                                function_name = "Đăng nhập / Đăng ký"; break;
                            default:
                                function_name = "Trang chủ"; break;
                        }
                        result.Add(
                            new RecentActivityLog()
                            {
                                user_name = survey.Email,
                                log_type = function_name,
                                log = survey.Answer,
                                log_date = create_on_str,
                                time_from_today = time_from_str
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAnswerSurvery - DashBoardController: " + ex.ToString());
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}