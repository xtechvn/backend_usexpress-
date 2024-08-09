using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Utilities.Contants;
using static Utilities.Contants.Constants;
using System.Globalization;
using static Utilities.Contants.OrderConstants;
using Utilities;
using Newtonsoft.Json;
using Entities.ViewModels.Orders;
using Entities.ViewModels.Affiliate;

namespace DAL
{
    public class OrderDAL : GenericService<Order>
    {
        private static DbWorker _DbWorker;

        public OrderDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public DataTable GetPagingList(OrderSearchModel searchModel, int currentPage, int pageSize)
        {
            try
            {
                DateTime _FromDate = DateTime.MinValue;
                DateTime _ToDate = DateTime.MinValue;
                DateTime _PaymentDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(searchModel.FromDate))
                {
                    _FromDate = DateTime.ParseExact(searchModel.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(searchModel.ToDate))
                {
                    _ToDate = DateTime.ParseExact(searchModel.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(searchModel.PaymentDate))
                {
                    _PaymentDate = DateTime.ParseExact(searchModel.PaymentDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                SqlParameter[] objParam = new SqlParameter[18];
                objParam[0] = new SqlParameter("@OrderNo", searchModel.OrderNo ?? string.Empty);

                if (_FromDate != DateTime.MinValue)
                    objParam[1] = new SqlParameter("@FromDate", _FromDate);
                else
                    objParam[1] = new SqlParameter("@FromDate", DBNull.Value);

                if (_ToDate != DateTime.MinValue)
                    objParam[2] = new SqlParameter("@ToDate", _ToDate);
                else
                    objParam[2] = new SqlParameter("@ToDate", DBNull.Value);

                objParam[3] = new SqlParameter("@ClientName", searchModel.ClientName ?? string.Empty);
                objParam[4] = new SqlParameter("@Phone", searchModel.Phone ?? string.Empty);
                objParam[5] = new SqlParameter("@OrderStatus", searchModel.OrderStatus ?? string.Empty);
                objParam[6] = new SqlParameter("@VoucherNo", searchModel.VoucherNo ?? string.Empty);
                objParam[7] = new SqlParameter("@UtmSource", searchModel.UtmSource ?? string.Empty);
                objParam[8] = new SqlParameter("@PaymentType", searchModel.PaymentType);
                objParam[9] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);

                if (_PaymentDate != DateTime.MinValue)
                    objParam[10] = new SqlParameter("@PaymentDate", _PaymentDate);
                else
                    objParam[10] = new SqlParameter("@PaymentDate", DBNull.Value);

                objParam[11] = new SqlParameter("@IsErrorOrder", searchModel.IsErrorOrder);
                objParam[12] = new SqlParameter("@ProductCode", searchModel.ProductCode ?? string.Empty);
                objParam[13] = new SqlParameter("@CurentPage", currentPage);
                objParam[14] = new SqlParameter("@PageSize", pageSize);
                objParam[15] = new SqlParameter("@IsAffialiate", searchModel.IsAffialiate);
                objParam[16] = new SqlParameter("@UtmMedium", searchModel.UtmMedium ?? string.Empty);
                objParam[17] = new SqlParameter("@LabelId", searchModel.LabelId);

                return _DbWorker.GetDataTable(ProcedureConstants.ORDER_SEARCH, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - OrderDAL: " + ex);
            }
            return null;
        }

        public DataSet GetOrderReport(OrderSearchModel searchModel)
        {
            try
            {
                DateTime _FromDate = DateTime.MinValue;
                DateTime _ToDate = DateTime.MinValue;
                DateTime _PaymentDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(searchModel.FromDate))
                {
                    _FromDate = DateTime.ParseExact(searchModel.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(searchModel.ToDate))
                {
                    _ToDate = DateTime.ParseExact(searchModel.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(searchModel.PaymentDate))
                {
                    _PaymentDate = DateTime.ParseExact(searchModel.PaymentDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                SqlParameter[] objParam = new SqlParameter[14];
                objParam[0] = new SqlParameter("@OrderNo", searchModel.OrderNo ?? string.Empty);

                if (_FromDate != DateTime.MinValue)
                    objParam[1] = new SqlParameter("@FromDate", _FromDate);
                else
                    objParam[1] = new SqlParameter("@FromDate", DBNull.Value);

                if (_ToDate != DateTime.MinValue)
                    objParam[2] = new SqlParameter("@ToDate", _ToDate);
                else
                    objParam[2] = new SqlParameter("@ToDate", DBNull.Value);

                objParam[3] = new SqlParameter("@ClientName", searchModel.ClientName ?? string.Empty);
                objParam[4] = new SqlParameter("@Phone", searchModel.Phone ?? string.Empty);
                objParam[5] = new SqlParameter("@OrderStatus", searchModel.OrderStatus ?? string.Empty);
                objParam[6] = new SqlParameter("@VoucherNo", searchModel.VoucherNo ?? string.Empty);
                objParam[7] = new SqlParameter("@UtmSource", searchModel.UtmSource ?? string.Empty);
                objParam[8] = new SqlParameter("@PaymentType", searchModel.PaymentType);
                objParam[9] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);

                if (_PaymentDate != DateTime.MinValue)
                    objParam[10] = new SqlParameter("@PaymentDate", _PaymentDate);
                else
                    objParam[10] = new SqlParameter("@PaymentDate", DBNull.Value);

                objParam[11] = new SqlParameter("@IsAffialiate", searchModel.IsAffialiate);
                objParam[12] = new SqlParameter("@UtmMedium", searchModel.UtmMedium ?? string.Empty);
                objParam[13] = new SqlParameter("@LabelId", searchModel.LabelId);

                return _DbWorker.GetDataSet(ProcedureConstants.ORDER_REPORT, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - OrderDAL: " + ex);
            }
            return null;
        }
        public DataSet GetOrderExpectedReport()
        {
            try
            {
               
                return _DbWorker.GetDataSet(ProcedureConstants.OrderExpected_Export);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderExpectedReport - OrderDAL: " + ex);
            }
            return null;
        }

        public double GetRevenuDay()
        {
            try
            {
                var result = _DbWorker.ExecuteScalar(ProcedureConstants.GET_REVENU_DAY);
                if (result == null)
                    return 0;
                return Math.Round((double)result, 2);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenuDay - OrderDAL: " + ex);
                return 0;
            }
        }

        public long GetTotalErrorOrderCount()
        {
            try
            {
                return _DbWorker.ExecuteNonQuery(ProcedureConstants.ORDER_GetErrorOrderCount);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalErrorOrderCount - OrderDAL: " + ex);
                return 0;
            }
        }


        /// <summary>
        /// get doanh thu theo thoi gian
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public DataTable GetRevenuByDateRange(OrderSearchModel searchModel)
        {
            try
            {
                DateTime? _FromDate = DateTime.MinValue;
                DateTime? _ToDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(searchModel.FromDate))
                {
                    _FromDate = DateTime.ParseExact(searchModel.FromDate, "dd/MM/yyyy"
                        , CultureInfo.InvariantCulture);
                    var fromDate = _FromDate.Value;
                    _FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                }

                if (!string.IsNullOrEmpty(searchModel.ToDate))
                {
                    _ToDate = DateTime.ParseExact(searchModel.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var toDate = _ToDate.Value;
                    _ToDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                }

                SqlParameter[] objParam = new SqlParameter[2];
                if (_FromDate != DateTime.MinValue)
                    objParam[0] = new SqlParameter("@FromDate", _FromDate);
                else
                    objParam[0] = new SqlParameter("@FromDate", DBNull.Value);

                if (_ToDate != DateTime.MinValue)
                    objParam[1] = new SqlParameter("@ToDate", _ToDate);
                else
                    objParam[1] = new SqlParameter("@ToDate", DBNull.Value);
                var result = _DbWorker.GetDataTable(ProcedureConstants.GET_REVENU_DATE_RANGE, objParam);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenuByDateRange - OrderDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// get doanh thu label theo thoi gian
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public DataTable GetLabelRevenuByDateRange(OrderSearchModel searchModel)
        {
            try
            {
                DateTime? _FromDate = DateTime.MinValue;
                DateTime? _ToDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(searchModel.FromDate))
                {
                    _FromDate = DateTime.ParseExact(searchModel.FromDate, "dd/MM/yyyy"
                        , CultureInfo.InvariantCulture);
                    var fromDate = _FromDate.Value;
                    _FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                }

                if (!string.IsNullOrEmpty(searchModel.ToDate))
                {
                    _ToDate = DateTime.ParseExact(searchModel.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var toDate = _ToDate.Value;
                    _ToDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                }

                SqlParameter[] objParam = new SqlParameter[2];
                if (_FromDate != DateTime.MinValue)
                    objParam[0] = new SqlParameter("@FromDate", _FromDate);
                else
                    objParam[0] = new SqlParameter("@FromDate", DBNull.Value);

                if (_ToDate != DateTime.MinValue)
                    objParam[1] = new SqlParameter("@ToDate", _ToDate);
                else
                    objParam[1] = new SqlParameter("@ToDate", DBNull.Value);
                var result = _DbWorker.GetDataTable(ProcedureConstants.GET_LABEL_REVENU_DATE_RANGE, objParam);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLabelRevenuByDateRange - OrderDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// get so luong order store theo thoi gian
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public DataTable GetLabelQuantityByDateRange(OrderSearchModel searchModel)
        {
            try
            {
                DateTime? _FromDate = DateTime.MinValue;
                DateTime? _ToDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(searchModel.FromDate))
                {
                    _FromDate = DateTime.ParseExact(searchModel.FromDate, "dd/MM/yyyy"
                        , CultureInfo.InvariantCulture);
                    var fromDate = _FromDate.Value;
                    _FromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
                }

                if (!string.IsNullOrEmpty(searchModel.ToDate))
                {
                    _ToDate = DateTime.ParseExact(searchModel.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var toDate = _ToDate.Value;
                    _ToDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
                }

                SqlParameter[] objParam = new SqlParameter[2];
                if (_FromDate != DateTime.MinValue)
                    objParam[0] = new SqlParameter("@FromDate", _FromDate);
                else
                    objParam[0] = new SqlParameter("@FromDate", DBNull.Value);

                if (_ToDate != DateTime.MinValue)
                    objParam[1] = new SqlParameter("@ToDate", _ToDate);
                else
                    objParam[1] = new SqlParameter("@ToDate", DBNull.Value);
                var result = _DbWorker.GetDataTable(ProcedureConstants.GET_LABEL_QUANTITY_DATE_RANGE, objParam);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLabelQuantityByDateRange - OrderDAL: " + ex);
                return null;
            }
        }
        public async Task<OrderViewModel> GetOrderDetail(long orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await (from _order in _DbContext.Order.AsNoTracking()
                                        join a in _DbContext.Voucher.AsNoTracking() on _order.VoucherId equals a.Id into af
                                        from _voucher in af.DefaultIfEmpty()
                                        where _order.Id == orderId
                                        select new OrderViewModel
                                        {
                                            Id = _order.Id,
                                            OrderNo = _order.OrderNo,
                                            ClientName = _order.ClientName,
                                            Address = _order.Address,
                                            Phone = _order.Phone,
                                            AmountUsd = _order.AmountUsd,
                                            RateCurrent = _order.RateCurrent,
                                            OrderStatus = _order.OrderStatus,
                                            UtmSource = _order.UtmSource,
                                            UtmMedium = _order.UtmMedium,
                                            Voucher = _voucher.Code,
                                            VoucherDescription = _voucher.Description,
                                            TrackingId = _order.TrackingId,
                                            PaymentType = _order.PaymentType,
                                            CreatedOn = _order.CreatedOn,
                                            PaymentDate = _order.PaymentDate,
                                            AmountVnd = _order.AmountVnd,
                                            TotalDiscount2ndUsd = _order.TotalDiscount2ndUsd,
                                            TotalDiscount2ndVnd = _order.TotalDiscount2ndVnd,
                                            TotalDiscountVoucherUsd = _order.TotalDiscountVoucherUsd,
                                            TotalDiscountVoucherVnd = _order.TotalDiscountVoucherVnd,
                                            OrderMapId = _order.OrderMapId,
                                            ClientId = _order.ClientId,
                                            AddressId = _order.AddressId
                                        }).FirstOrDefaultAsync();
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetail - OrderDAL: " + ex);
                return null;
            }
        }

        public async Task<OrderViewModel> GetOrderDetail(string orderNo)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await (from _order in _DbContext.Order.AsNoTracking()
                                        where _order.OrderNo == orderNo
                                        select new OrderViewModel
                                        {
                                            Id = _order.Id,
                                            OrderNo = _order.OrderNo,
                                            ClientName = _order.ClientName,
                                            ClientId = _order.ClientId,
                                            Email = _order.Email,
                                            UserId = _order.UserId,
                                            PriceVnd = _order.PriceVnd,
                                            Note = _order.Note,
                                            Address = _order.Address,
                                            Phone = _order.Phone,
                                            AmountUsd = _order.AmountUsd,
                                            RateCurrent = _order.RateCurrent,
                                            OrderStatus = _order.OrderStatus,
                                            UtmSource = _order.UtmSource,
                                            TrackingId = _order.TrackingId,
                                            PaymentType = _order.PaymentType,
                                            CreatedOn = _order.CreatedOn,
                                            PaymentDate = _order.PaymentDate,
                                            AmountVnd = _order.AmountVnd,
                                            VoucherId = _order.VoucherId,
                                            Discount = _order.Discount,
                                            PriceUsd = _order.PriceUsd,
                                            PaymentStatus = _order.PaymentStatus,
                                            LabelId = _order.LabelId,
                                            TotalDiscount2ndUsd = _order.TotalDiscount2ndUsd,
                                            TotalDiscount2ndVnd = _order.TotalDiscount2ndVnd,
                                            TotalDiscountVoucherUsd = _order.TotalDiscountVoucherUsd,
                                            TotalDiscountVoucherVnd = _order.TotalDiscountVoucherVnd
                                        }).FirstOrDefaultAsync();
                    if (detail.VoucherId != null && detail.VoucherId > 0)
                    {
                        var voucherInfo = _DbContext.Voucher.FirstOrDefault(n => n.Id == detail.VoucherId);
                        detail.Voucher = voucherInfo != null ? voucherInfo.Code : "";
                    }
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetail(orderNo) - OrderDAL: " + ex);
                return null;
            }
        }

        public async Task<List<OrderItemViewModel>> GetOrderItemList(long orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = await (from _orderitem in _DbContext.OrderItem.AsNoTracking()
                                      join a in _DbContext.Product.AsNoTracking() on _orderitem.ProductId equals a.Id into af
                                      from _product in af.DefaultIfEmpty()
                                      where _orderitem.OrderId == orderId
                                      select new OrderItemViewModel
                                      {
                                          ProductId = _product.Id,
                                          OrderId = orderId,
                                          DiscountShippingFirstPound = _orderitem.DiscountShippingFirstPound != null
                                           ? _orderitem.DiscountShippingFirstPound.Value : 0,
                                          Weight = _orderitem.Weight != null ? _orderitem.Weight.Value : 0,
                                          ProductName = _product.Title,
                                          ProductCode = _product.ProductCode,
                                          ProductImage = _orderitem.ImageThumb,
                                          LabelId = _product.LabelId,
                                          ProductPath = _product.Path,
                                          Price = _orderitem.Price,
                                          Quantity = (int)_orderitem.Quantity,
                                          FirstPoundFee = (double)_orderitem.FirstPoundFee,
                                          NextPoundFee = (double)_orderitem.NextPoundFee,
                                          LuxuryFee = (double)_orderitem.LuxuryFee,
                                          ShippingFeeUs = (double)_orderitem.ShippingFeeUs,
                                          OrderItemMapId = _orderitem.OrderItempMapId != null ? _orderitem.OrderItempMapId.Value : 0,
                                          SpecialLuxuryId = _orderitem.SpecialLuxuryId != null ? _orderitem.SpecialLuxuryId.Value : 0,
                                          ProductMapId = _product.ProductMapId != null ? _product.ProductMapId.Value : -1
                                      }).ToListAsync();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderItemList - OrderDAL: " + ex);
                return null;
            }
        }

        public async Task<Order> FindByOrderNo(string orderNo)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Order.FirstOrDefaultAsync(s => s.OrderNo == orderNo);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByOrderNo - OrderDAL: " + ex);
                return null;
            }
        }

        public async Task<Order> FindByOrderId(int orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Order.
                        FirstOrDefaultAsync(s => s.Id == orderId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByOrderId - OrderDAL: " + ex);
                return null;
            }
        }

        public async Task<object> GetOrderSuggestionList(string orderNo)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Order.Where(s => s.OrderNo.Contains(orderNo)).Take(5)
                                                     .Select(m => new
                                                     {
                                                         m.Id,
                                                         m.OrderNo,
                                                         m.ClientName,
                                                         m.Address,
                                                         m.Phone
                                                     }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderSuggestionList - OrderDAL: " + ex);
                return null;
            }
        }

        public RevenueViewModel SummaryRevenuToday()
        {
            RevenueViewModel revenueViewModel = new RevenueViewModel();
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var listOrder = _DbContext.Order.Where(s =>
                    (s.PaymentDate ?? s.CreatedOn).Value.Date == DateTime.Today
                    && s.PaymentStatus == (int)Payment_Status.DA_THANH_TOAN
                    && s.OrderStatus != (int)OrderStatus.CANCEL_ORDER).ToList();

                    revenueViewModel.TotalOrder = listOrder.Count;
                    revenueViewModel.Revenue = listOrder.Sum(n => n.AmountVnd);
                    revenueViewModel.RevenueStr = revenueViewModel.Revenue.Value.ToString("N0");
                    return revenueViewModel;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SummaryRevenuToday - OrderDAL: " + ex);
                return revenueViewModel;
            }
        }

        public RevenueViewModel SummaryRevenuTodayTemp()
        {
            RevenueViewModel revenueViewModel = new RevenueViewModel();
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var listOrder = _DbContext.Order.Where(s => s.CreatedOn.Value.Date ==
                    DateTime.Today && s.PaymentStatus == (int)Payment_Status.CHUA_THANH_TOAN).ToList();
                    revenueViewModel.TotalOrder = listOrder.Count;
                    revenueViewModel.Revenue = listOrder.Sum(n => n.AmountVnd);
                    revenueViewModel.RevenueStr = revenueViewModel.Revenue.Value.ToString("N0");
                    return revenueViewModel;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SummaryRevenuTodayTemp - OrderDAL: " + ex);
                return revenueViewModel;
            }
        }

        public async Task<List<OrderGridModel>> GetOrderListByClientId(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var models = await (from _order in _DbContext.Order.AsNoTracking()
                                        join a in _DbContext.AllCode.Where(s => s.Type == AllCodeType.ORDER_STATUS).AsNoTracking() on _order.OrderStatus equals a.CodeValue into af
                                        from _orderStatus in af.DefaultIfEmpty()
                                        join b in _DbContext.AllCode.Where(s => s.Type == AllCodeType.PAYMENT_TYPE).AsNoTracking() on _order.PaymentType equals b.CodeValue into bf
                                        from _orderPayment in bf.DefaultIfEmpty()
                                        where _order.ClientId == clientId
                                        select new OrderGridModel
                                        {
                                            Id = _order.Id,
                                            OrderNo = _order.OrderNo,
                                            CreatedOn = _order.CreatedOn,
                                            AmountVnd = _order.AmountVnd,
                                            TotalShippingFeeVnd = _order.TotalShippingFeeVnd,
                                            OrderStatus = _order.OrderStatus,
                                            OrderStatusName = _orderStatus.Description,
                                            PaymentStatus = _order.PaymentStatus,
                                            PaymentType = _order.PaymentType,
                                            PaymentTypeName = _orderPayment.Description
                                        }
                                   ).OrderByDescending(s => s.CreatedOn).ToListAsync();
                    return models;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderListByClientId - OrderDAL: " + ex);
                return null;
            }
        }

        public async Task<List<OrderGridModel>> GetOrderListByReferralId(string ReferralId)
        {
            try
            {
                if (!string.IsNullOrEmpty(ReferralId))
                {
                    using (var _DbContext = new EntityDataContext(_connection))
                    {
                        var models = await (from _order in _DbContext.Order.AsNoTracking()
                                            join a in _DbContext.AllCode.Where(s => s.Type == AllCodeType.ORDER_STATUS).AsNoTracking() on _order.OrderStatus equals a.CodeValue into af
                                            from _orderStatus in af.DefaultIfEmpty()
                                            join b in _DbContext.AllCode.Where(s => s.Type == AllCodeType.PAYMENT_TYPE).AsNoTracking() on _order.PaymentType equals b.CodeValue into bf
                                            from _orderPayment in bf.DefaultIfEmpty()
                                            where _order.UtmMedium == ("us_" + ReferralId)
                                            select new OrderGridModel
                                            {
                                                Id = _order.Id,
                                                OrderNo = _order.OrderNo,
                                                CreatedOn = _order.CreatedOn,
                                                AmountVnd = _order.AmountVnd,
                                                TotalShippingFeeVnd = _order.TotalShippingFeeVnd,
                                                OrderStatus = _order.OrderStatus,
                                                OrderStatusName = _orderStatus.Description,
                                                PaymentStatus = _order.PaymentStatus,
                                                PaymentType = _order.PaymentType,
                                                PaymentTypeName = _orderPayment.Description
                                            }
                                       ).OrderByDescending(s => s.CreatedOn).ToListAsync();
                        return models;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderListByClientId - OrderDAL: " + ex);
            }

            return null;
        }

        public async Task<RevenueMinMax> GetMinMaxAmountOrder()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var datas = await _DbContext.Order.Where(s => s.PaymentStatus == (int)OrderConstants.Payment_Status.DA_THANH_TOAN)
                                                      .Select(s => s.AmountVnd).ToListAsync();
                    var model = new RevenueMinMax
                    {
                        Min = (double)datas.Min(),
                        Max = (double)datas.Max()
                    };
                    return model;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetMinMaxAmountOrder - OrderDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// Lấy ra tổng số order đc khởi tạo trong ngày
        /// </summary>
        /// <returns></returns>
        public async Task<int> getTotalOrderByCurrentDate()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    DateTime current = DateTime.Now;
                    DateTime startDate = new DateTime(current.Year, current.Month, current.Day, 0, 0, 0);

                    int total_order = await _DbContext.Order.CountAsync(s => s.CreatedOn >= startDate && s.CreatedOn <= current);

                    return total_order;
                }
            }
            catch (Exception ex)
            {
                var rd = new Random();
                LogHelper.InsertLogTelegram("getTotalOrderByDate - OrderDAL: " + ex);
                return rd.Next(100, 999);

            }
        }

        /// <summary>
        /// Hàm này sẽ lấy thông tin đơn hàng bắn sang hệ thống cũ
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<OrderApiViewModel> GetOrderByIdForAPI(long orderId)
        {
            try
            {

                string _provinceId = "-1";
                string _districtId = "-1";
                string _wardId = "-1";

                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var order_detail = await this.GetOrderDetail(orderId);

                    var address_detail = await _DbContext.AddressClient.AsNoTracking().FirstOrDefaultAsync(x => x.Id == order_detail.AddressId);

                    if (address_detail != null)
                    {
                        _provinceId = address_detail.ProvinceId;
                        _districtId = address_detail.DistrictId;
                        _wardId = address_detail.DistrictId;
                    }
                    else
                    {
                        var address_detail_2 = await _DbContext.AddressClient.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == order_detail.ClientId); //addressID
                        if (address_detail_2 == null)
                        {
                            LogHelper.InsertLogTelegram("[DAL] GetOrderByIdForAPI - OrderDAL: khong tim thay dia chi cho khach hang nay. Không thể bắn order " + orderId + " sang us old");
                        }
                        else
                        {
                            _provinceId = address_detail_2.ProvinceId;
                            _districtId = address_detail_2.DistrictId;
                            _wardId = address_detail_2.DistrictId;
                        }
                    }

                    var detail = await (from _order in _DbContext.Order.AsNoTracking()
                                        join a in _DbContext.Voucher.AsNoTracking() on _order.VoucherId equals a.Id into af
                                        from _voucher in af.DefaultIfEmpty()
                                        join b in _DbContext.Client.AsNoTracking() on _order.ClientId equals b.Id into bf
                                        from _client in bf.DefaultIfEmpty()
                                        where _order.Id == orderId
                                        select new OrderApiViewModel
                                        {
                                            Id = _order.Id,
                                            OrderNo = _order.OrderNo,
                                            OrderStatus = _order.OrderStatus,

                                            ClientId = _order.ClientId,
                                            UserId = _order.UserId,
                                            LabelId = _order.LabelId,
                                            ClientName = _order.ClientName,
                                            Email = _order.Email,
                                            Phone = _order.Phone,
                                            Address = _order.Address,

                                            RateCurrent = _order.RateCurrent,
                                            Discount = _order.Discount,

                                            PriceVnd = _order.AmountVnd, // priceVND sẽ là số tiền sau giảm bên hệ thống cũ
                                            PriceUsd = _order.PriceUsd,
                                            AmountUsd = _order.AmountUsd,
                                            AmountVnd = _order.AmountVnd,

                                            TotalDiscount2ndUsd = _order.TotalDiscount2ndUsd,
                                            TotalDiscount2ndVnd = _order.TotalDiscount2ndVnd,
                                            TotalDiscountVoucherUsd = _order.TotalDiscountVoucherUsd,
                                            TotalDiscountVoucherVnd = _order.TotalDiscountVoucherVnd,
                                            TotalShippingFeeUsd = _order.TotalShippingFeeUsd,
                                            TotalShippingFeeVnd = _order.TotalShippingFeeVnd,

                                            VoucherId = _order.VoucherId,
                                            Voucher = _voucher.Code,
                                            VoucherDescription = _voucher.Description,

                                            CreatedOn = _order.CreatedOn,

                                            PaymentStatus = _order.PaymentStatus,
                                            PaymentType = _order.PaymentType,
                                            PaymentDate = _order.PaymentDate,
                                            Note = _order.Note,

                                            UtmSource = _order.UtmSource,
                                            UtmMedium = _order.UtmMedium,
                                            UtmFirstTime = _order.UtmFirstTime,
                                            UtmCampaign = _order.UtmCampaign,
                                            TrackingId = _order.TrackingId,

                                            ClientMapId = _client.ClientMapId ?? 0,
                                            Province = _provinceId,
                                            District = _districtId,
                                            Ward = _wardId

                                        }).FirstOrDefaultAsync();
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetail - OrderDAL: " + ex);
                return null;
            }
        }

        public async Task<long> UpdateOrderMapId(long OrderId, long OrderMapId)
        {
            try
            {
                var model = await FindAsync(OrderId);
                model.OrderMapId = OrderMapId;
                await UpdateAsync(model);
                return OrderId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderMapId - OrderDAL: " + ex);
                return 0;
            }
        }

        /// <summary>
        /// Lấy ra số lần voucher được dùng. Trừ những đơn HỦY
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public int GetTotalVoucherUse(long voucher_id, string email_client)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Order.AsNoTracking().Where(x => x.VoucherId == voucher_id && x.Email == (email_client.IndexOf("@") == -1 ? x.Email : email_client) && x.OrderStatus != (Int16)OrderStatus.CANCEL_ORDER);

                    return detail.Count();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalVoucherUse - OrderDAL:(email = " + email_client + ", voucher_id =" + voucher_id + ") " + ex);
                return -1;
            }
        }

        public object GetOrderListByClientId(int clientId, string orderNo, int orderStatus = -1, int curentPage = 1, int pageSize = 10)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[5];
                objParam[0] = new SqlParameter("@ClientId", clientId);
                objParam[1] = new SqlParameter("@OrderNo", orderNo ?? string.Empty);
                objParam[2] = new SqlParameter("@OrderStatus", orderStatus);
                objParam[3] = new SqlParameter("@CurentPage", curentPage);
                objParam[4] = new SqlParameter("@PageSize", pageSize);
                var dataTable = _DbWorker.GetDataTable(ProcedureConstants.ORDER_GetListByClientId, objParam);

                var _TotalRow = 0;
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    _TotalRow = Convert.ToInt32(dataTable.Rows[0]["TotalRow"]);
                }

                var _dataList = (from dr in dataTable.AsEnumerable()
                                 select new
                                 {
                                     Id = Convert.ToInt64(!dr["Id"].Equals(DBNull.Value) ? dr["Id"] : 0),
                                     OrderNo = dr["OrderNo"].ToString(),
                                     CreatedOn = Convert.ToDateTime(!dr["CreatedOn"].Equals(DBNull.Value) ? dr["CreatedOn"] : DateTime.MinValue),
                                     OrderStatus = Convert.ToInt32(!dr["OrderStatus"].Equals(DBNull.Value) ? dr["OrderStatus"] : 0),
                                     OrderStatusName = dr["OrderStatusName"].ToString(),
                                     AmountVnd = Convert.ToDouble(!dr["AmountVnd"].Equals(DBNull.Value) ? dr["AmountVnd"] : 0),
                                     ProductCode = dr["ProductCode"].ToString(),
                                     Title = dr["Title"].ToString(),
                                     Quantity = Convert.ToInt32(!dr["Quantity"].Equals(DBNull.Value) ? dr["Quantity"] : 0),
                                     ImageThumb = dr["ImageThumb"].ToString()
                                     //LinkSource = dr["LinkSource"].ToString()
                                 }).GroupBy(s => new
                                 {
                                     s.Id,
                                     s.OrderNo,
                                     s.CreatedOn,
                                     s.OrderStatus,
                                     s.OrderStatusName,
                                     s.AmountVnd
                                 }).Select(s => new
                                 {
                                     Id = s.Key.Id,
                                     OrderNo = s.Key.OrderNo,
                                     CreatedOn = s.Key.CreatedOn,
                                     OrderStatus = s.Key.OrderStatus,
                                     OrderStatusName = s.Key.OrderStatusName,
                                     AmountVnd = Convert.ToDecimal(s.Key.AmountVnd).ToString("#,##0.00"),
                                     Product = s.Select(m => new
                                     {
                                         ProductCode = m.ProductCode,
                                         Title = m.Title,
                                         Quantity = m.Quantity,
                                         ImageThumb = m.ImageThumb
                                         //LinkSource = m.LinkSource
                                     })
                                 }).ToList();

                return new
                {
                    totalOrder = _TotalRow,
                    curentPage = curentPage,
                    pageSize = pageSize,
                    dataList = _dataList
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public object GetFeOrderDetailById(int OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                var dataTable = _DbWorker.GetDataSet(ProcedureConstants.ORDER_FE_GetDetailById, objParam);

                var order = (from dr in dataTable.Tables[0].AsEnumerable()
                             select new
                             {
                                 Id = Convert.ToInt64(!dr["Id"].Equals(DBNull.Value) ? dr["Id"] : 0),
                                 ClientId = Convert.ToInt64(!dr["ClientId"].Equals(DBNull.Value) ? dr["ClientId"] : 0),
                                 OrderNo = dr["OrderNo"].ToString(),
                                 CreatedOn = Convert.ToDateTime(!dr["CreatedOn"].Equals(DBNull.Value) ? dr["CreatedOn"] : DateTime.MinValue),
                                 OrderStatus = Convert.ToInt32(!dr["OrderStatus"].Equals(DBNull.Value) ? dr["OrderStatus"] : 0),
                                 PaymentStatus = Convert.ToInt16(!dr["PaymentStatus"].Equals(DBNull.Value) ? dr["PaymentStatus"] : 1),
                                 OrderStatusName = dr["OrderStatusName"].ToString(),
                                 AmountVnd = Convert.ToDouble(!dr["AmountVnd"].Equals(DBNull.Value) ? dr["AmountVnd"] : 0),
                                 TotalDiscount2ndVnd = Convert.ToDouble(!dr["TotalDiscount2ndVnd"].Equals(DBNull.Value) ? dr["TotalDiscount2ndVnd"] : 0),
                                 TotalDiscountVoucherVnd = Convert.ToDouble(!dr["TotalDiscountVoucherVnd"].Equals(DBNull.Value) ? dr["TotalDiscountVoucherVnd"] : 0),
                                 PaymentType = Convert.ToInt32(!dr["PaymentType"].Equals(DBNull.Value) ? dr["PaymentType"] : 0),
                                 PaymentTypeName = dr["PaymentTypeName"].ToString(),
                                 ClientName = dr["ClientName"].ToString(),
                                 Address = dr["Address"].ToString(),
                                 Phone = dr["Phone"].ToString(),
                                 StoreName = dr["StoreName"].ToString(),
                                 RateCurrent = Convert.ToDouble(!dr["RateCurrent"].Equals(DBNull.Value) ? dr["RateCurrent"] : 0),
                                 Note = dr["Note"].ToString(),
                                 addressId = Convert.ToInt32(dr["AddressId"])
                             }).FirstOrDefault();

                var products = (from dr in dataTable.Tables[1].AsEnumerable()
                                select new
                                {
                                    ImageThumb = dr["ImageThumb"].ToString(),
                                    ProductCode = dr["ProductCode"].ToString(),
                                    Title = dr["Title"].ToString(),
                                    Price = Convert.ToDouble(!dr["Price"].Equals(DBNull.Value) ? dr["Price"] : 0),
                                    Quantity = Convert.ToInt32(!dr["Quantity"].Equals(DBNull.Value) ? dr["Quantity"] : 0),
                                    FirstPoundFee = Convert.ToDouble(!dr["FirstPoundFee"].Equals(DBNull.Value) ? dr["FirstPoundFee"] : 0),
                                    NextPoundFee = Convert.ToDouble(!dr["NextPoundFee"].Equals(DBNull.Value) ? dr["NextPoundFee"] : 0),
                                    LuxuryFee = Convert.ToDouble(!dr["LuxuryFee"].Equals(DBNull.Value) ? dr["LuxuryFee"] : 0),
                                    Weight = Convert.ToDouble(!dr["Weight"].Equals(DBNull.Value) ? dr["Weight"] : 0),
                                    Path = dr["Path"].ToString(),
                                    SellerName = dr["SellerName"].ToString(),
                                    LinkSource = dr["LinkSource"].ToString()
                                }).ToList();


                string _PaymentTypeName = string.Empty;

                switch (order.PaymentType)
                {
                    case 3:
                    case 5:
                        _PaymentTypeName = "Thanh toán bằng thẻ ATM";
                        break;
                    case 4:
                    case 6:
                        _PaymentTypeName = "Thanh toán bằng thẻ VISA";
                        break;
                }

                var data = new
                {
                    Id = order.Id,
                    ClientId = order.ClientId,
                    OrderNo = order.OrderNo,
                    CreatedOn = order.CreatedOn,
                    OrderStatus = order.OrderStatus,
                    OrderStatusName = order.OrderStatusName,
                    AmountVnd = order.AmountVnd,
                    TotalDiscount2ndVnd = order.TotalDiscount2ndVnd,
                    TotalDiscountVoucherVnd = order.TotalDiscountVoucherVnd,
                    PaymentType = order.PaymentType,
                    PaymentTypeName = !string.IsNullOrEmpty(_PaymentTypeName) ? _PaymentTypeName : order.PaymentTypeName,
                    ClientName = order.ClientName,
                    Address = order.Address,
                    Phone = order.Phone,
                    StoreName = order.StoreName,
                    RateCurrent = order.RateCurrent,
                    PaymentStatus = order.PaymentStatus,
                    Note = order.Note,
                    AddressId = order.addressId,
                    ProductList = products.Select(s => new
                    {
                        ImageThumb = s.ImageThumb,
                        ProductCode = s.ProductCode,
                        Title = s.Title,
                        Price = s.Price * order.RateCurrent,
                        Quantity = s.Quantity,
                        FirstPoundFee = s.FirstPoundFee,
                        NextPoundFee = s.NextPoundFee,
                        LuxuryFee = s.LuxuryFee,
                        Weight = s.Weight,
                        Path = s.Path,
                        SellerName = s.SellerName,
                        LinkSource = s.LinkSource,
                        Cost = (s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity * order.RateCurrent
                    })
                };
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetFeOrderDetailById - OrderDAL: " + ex);
                return null;
            }

        }

        public object GetLastestRecordByClientID(int ClientId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientId", ClientId);
                var dataTable = _DbWorker.GetDataSet(ProcedureConstants.Order_FE_SelectLatestOrderbyClientID, objParam);

                var order = (from dr in dataTable.Tables[0].AsEnumerable()
                             select new
                             {
                                 Id = Convert.ToInt64(!dr["Id"].Equals(DBNull.Value) ? dr["Id"] : 0),
                                 OrderNo = dr["OrderNo"].ToString(),
                                 CreatedOn = Convert.ToDateTime(!dr["CreatedOn"].Equals(DBNull.Value) ? dr["CreatedOn"] : DateTime.MinValue),
                                 OrderStatus = Convert.ToInt32(!dr["OrderStatus"].Equals(DBNull.Value) ? dr["OrderStatus"] : 0),
                                 OrderStatusName = dr["OrderStatusName"].ToString(),
                                 UpdateLast = Convert.ToDateTime(!dr["UpdateLast"].Equals(DBNull.Value) ? dr["UpdateLast"] : DateTime.MinValue),
                                 AmountVnd = Convert.ToDouble(!dr["AmountVnd"].Equals(DBNull.Value) ? dr["AmountVnd"] : 0),
                                 PaymentType = Convert.ToInt32(!dr["PaymentType"].Equals(DBNull.Value) ? dr["PaymentType"] : 0),
                                 PaymentTypeName = dr["PaymentTypeName"].ToString(),
                                 PaymentStatus = Convert.ToInt16(!dr["PaymentStatus"].Equals(DBNull.Value) ? dr["PaymentStatus"] : 1)
                             }).FirstOrDefault();
                if (order != null) // cuonglv bổ sung đoạn check null
                {
                    var data = new
                    {
                        Id = order.Id,
                        OrderNo = order.OrderNo,
                        CreatedOn = order.CreatedOn.ToString("dd/MM/yyyy HH:mm"),
                        CreatedOnDate = order.CreatedOn.ToString("dd/MM/yyyy"),
                        CreatedOnTime = order.CreatedOn.ToString("HH:MM"),
                        OrderStatus = order.OrderStatus,
                        OrderStatusName = order.OrderStatusName,
                        AmountVnd = Convert.ToDecimal(order.AmountVnd).ToString("#,##0.00"),
                        PaymentType = order.PaymentType,
                        PaymentTypeName = order.PaymentTypeName,
                        PaymentStatus = order.PaymentStatus
                    };
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLastestRecordByClientID - OrderDAL: " + ex);
                return null;
            }
        }

        public object GetOrderCountByClientID(int ClientId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientId", ClientId);
                var dataTable = _DbWorker.GetDataSet(ProcedureConstants.Order_FE_GetOrderCountByClientID, objParam);

                var order = (from dr in dataTable.Tables[0].AsEnumerable()
                             select new
                             {
                                 AllOders = Convert.ToInt64(!dr["AllOders"].Equals(DBNull.Value) ? dr["AllOders"] : 0),
                                 WaitToReceiveCount = Convert.ToInt64(!dr["WaitToReceiveCount"].Equals(DBNull.Value) ? dr["WaitToReceiveCount"] : 0),
                                 WaitForPaymentCount = Convert.ToInt64(!dr["WaitForPaymentCount"].Equals(DBNull.Value) ? dr["WaitForPaymentCount"] : 0),
                                 ReceivedOrderCount = Convert.ToInt64(!dr["ReceivedOrderCount"].Equals(DBNull.Value) ? dr["ReceivedOrderCount"] : 0),
                                 FailedOrderCount = Convert.ToInt64(!dr["FailedOrderCount"].Equals(DBNull.Value) ? dr["FailedOrderCount"] : 0),

                             }).FirstOrDefault();
                var data = new
                {
                    AllOders = order.AllOders,
                    WaitToReceiveCount = order.WaitToReceiveCount,
                    WaitForPaymentCount = order.WaitForPaymentCount,
                    ReceivedOrderCount = order.ReceivedOrderCount,
                    FailedOrderCount = order.FailedOrderCount
                };
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<long> UpdatePaymentReChecOut(long order_id, string receiver_name, string address, string phone, short pay_type)
        {
            try
            {
                var model = await FindAsync(order_id);

                model.ClientName = receiver_name; // người nhận hàng
                model.Address = address; // địa chỉ nhận hàng
                model.Phone = phone; // phone người nhận
                model.PaymentType = pay_type; // hình thức thanh toán
                model.UpdateLast = DateTime.Now;
                await UpdateAsync(model);
                return order_id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePaymentReChecOut - OrderDAL: " + ex);
                return -1;
            }
        }

        public async Task<Order> getTotalOrderByEmail(string email)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Order.FirstOrDefaultAsync(s => s.Email.ToUpper() == email.Trim().ToUpper());
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getTotalOrderByEmail - OrderDAL: " + ex);
                return null;
            }
        }

        #region api for south-us
        public async Task<OrderAppModel> GetOrderDetailByOrderNo(string order_no)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var order = await _DbContext.Order.AsNoTracking().Where(s => s.OrderNo == order_no)
                        .Join(_DbContext.AllCode.AsNoTracking().Where(s => s.Type == AllCodeType.ORDER_STATUS),
                         ord => ord.OrderStatus,
                         ost => ost.CodeValue,
                         (ord, ost) => new
                         {
                             id = ord.Id,
                             order_no = ord.OrderNo,
                             create_date = ord.CreatedOn,
                             total_amount = ord.AmountUsd,
                             total_amount_vnd = ord.AmountVnd,
                             recipient_name = ord.ClientName,
                             recipient_phone = ord.Phone,
                             recipient_address = ord.Address,
                             order_status = ord.OrderStatus,
                             rate_current = ord.RateCurrent,
                             order_status_name = ost != null ? ost.Description : string.Empty
                         }).FirstOrDefaultAsync();

                    var order_detail = await _DbContext.OrderItem.AsNoTracking().Where(s => s.OrderId == order.id).Join(_DbContext.Product.AsNoTracking(),
                        oitem => oitem.ProductId,
                        prod => prod.Id,
                        (oitem, prod) => new
                        {
                            product_code = prod != null ? prod.ProductCode : string.Empty,
                            product_name = prod != null ? prod.Title : string.Empty,
                            price = oitem.Price,
                            quantity = oitem.Quantity,
                            weight = oitem.Weight,
                            shipping_fee = (oitem.FirstPoundFee + oitem.NextPoundFee + oitem.LuxuryFee) * oitem.Quantity,
                            shipping_fee_vnd = (oitem.FirstPoundFee + oitem.NextPoundFee + oitem.LuxuryFee) * oitem.Quantity * order.rate_current,
                            amount = (oitem.Price + oitem.FirstPoundFee + oitem.NextPoundFee + oitem.LuxuryFee) * oitem.Quantity,
                            amount_vnd = (oitem.Price + oitem.FirstPoundFee + oitem.NextPoundFee + oitem.LuxuryFee) * oitem.Quantity * order.rate_current
                        }).ToListAsync();

                    return new OrderAppModel()
                    {
                        order = order,
                        order_detail = order_detail
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetailByOrderNo - OrderDAL: " + ex);
                return new OrderAppModel();
            }
        }

        public async Task<object> GetOrderListByClientPhone(string client_phone)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return await _DbContext.Order.AsNoTracking().Where(s => s.Phone == client_phone)
                        .Join(_DbContext.AllCode.AsNoTracking().Where(s => s.Type == AllCodeType.ORDER_STATUS),
                         ord => ord.OrderStatus,
                         ost => ost.CodeValue,
                         (s, t) => new
                         {
                             order_no = s.OrderNo,
                             phone = s.Phone,
                             create_date = s.CreatedOn,
                             // total_amount = s.AmountUsd,
                             total_amount_vnd = s.AmountVnd,
                             status = t.Description,
                             // rate_current = s.RateCurrent
                         }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderListByClientPhone - OrderDAL: " + ex);
                return new List<object>();
            }
        }
        public async Task<object> GetOrderTrackingByOrderNo(string order_no)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.OrderProgress.AsNoTracking().Where(s => s.OrderNo == order_no).Join(_DbContext.AllCode.AsNoTracking().Where(s => s.Type == AllCodeType.ORDER_STATUS),
                         prog => prog.OrderStatus,
                         ost => ost.CodeValue,
                         (prog, ost) => new
                         {
                             order_no = prog.OrderNo,
                             create_date = prog.CreateDate,
                             order_status = ost != null ? ost.Description : string.Empty
                         }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderTrackingByOrderNo - OrderDAL: " + ex);
                return new List<object>();
            }
        }
        #endregion
        public async Task<string> UpdateOrderStatus(string order_no, int order_status_update)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var order = await _DbContext.Order.FirstOrDefaultAsync(s => s.OrderNo == order_no);
                    if (order == null || order.OrderNo == null || order.OrderNo != order_no)
                    {
                        return "Không tìm thấy đơn hàng " + order_no;
                    }
                    if (order.OrderStatus == (int)OrderStatus.CLIENT_TRANSPORT_ORDER || order.OrderStatus == (int)OrderStatus.VN_STORAGE_ORDER || order.OrderStatus == (int)OrderStatus.VN_TRANSPORT_STORAGE_ORDER)
                    {
                        order.OrderStatus = order_status_update;
                        await UpdateAsync(order);
                        return null;
                    }
                    return "Đơn hàng này không thể chuyển đổi status";

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderStatus - OrderDAL: " + ex);
            }
            return "Error on Excution";
        }
        public async Task<List<AffOrder>> GetAffiliateOrders(DateTime time_gte, DateTime time_lte, List<string> utm_source)
        {
            List<AffOrder> list = null;
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    list = new List<AffOrder>();
                    foreach (var utm in utm_source)
                    {
                        var orders = await _DbContext.Order.Where(s => (DateTime)s.CreatedOn >= time_gte && (DateTime)s.CreatedOn <= time_lte && s.UtmSource.Trim().ToLower().Contains(utm.Trim().ToLower()) && s.PaymentStatus == 1).ToListAsync();
                        if (orders != null && orders.Count > 0)
                        {
                            foreach (var order in orders)
                            {
                                if (order.Id > 0)
                                {
                                    list.Add(new AffOrder()
                                    {
                                        aff_name = order.UtmSource,
                                        order_id = order.Id
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAffiliateOrders - OrderDAL: " + ex);
            }
            return list;
        }
        public List<OrderLogShippingDateViewModel> GetOrderShippingLogToday()
        {
            List<OrderLogShippingDateViewModel> result = null;
            try
            {
                var dataTable = _DbWorker.GetDataSet(ProcedureConstants.Order_GetShippingExpectedDays);

                var orders = (from dr in dataTable.Tables[0].AsEnumerable()
                              select new OrderLogShippingDateViewModel
                              {
                                  Id = Convert.ToInt64(!dr["Id"].Equals(DBNull.Value) ? dr["Id"] : 0),
                                  OrderNo = dr["OrderNo"].ToString(),
                                  LabelId = Convert.ToInt32(!dr["LabelId"].Equals(DBNull.Value) ? dr["LabelId"] : 0),
                                  OrderStatus = Convert.ToInt32(!dr["OrderStatus"].Equals(DBNull.Value) ? dr["OrderStatus"] : 0),
                                  OrderStatusName = dr["OrderStatusName"].ToString(),
                                  CreatedOn = Convert.ToDateTime(!dr["CreatedOn"].Equals(DBNull.Value) ? dr["CreatedOn"] : DateTime.MinValue),
                                  PaymentDate = Convert.ToDateTime(!dr["PaymentDate"].Equals(DBNull.Value) ? dr["PaymentDate"] : DateTime.MinValue),
                                  LastestOrderProgressDay = Convert.ToInt32(!dr["LastestOrderProgressDay"].Equals(DBNull.Value) ? dr["LastestOrderProgressDay"] : DateTime.MinValue),
                                  TotalOrderProgressDay = Convert.ToInt32(!dr["TotalOrderProgressDay"].Equals(DBNull.Value) ? dr["TotalOrderProgressDay"] : DateTime.MinValue)
                              });

                result = orders.ToList();
                //return orders;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderShippingLogToday - OrderDAL: " + ex);

            }
            return result;

        }
        public async Task<OrderViewModel> CheckOrderDetail(long orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await (from _order in _DbContext.Order.AsNoTracking()
                                        join a in _DbContext.Voucher.AsNoTracking() on _order.VoucherId equals a.Id into af
                                        from _voucher in af.DefaultIfEmpty()
                                        where _order.Id == orderId
                                        select new OrderViewModel
                                        {
                                            Id = _order.Id,
                                            OrderNo = _order.OrderNo,
                                            ClientName = _order.ClientName,

                                            OrderStatus = _order.OrderStatus,
                                            LabelId=_order.LabelId,
                                            PaymentType = _order.PaymentType,
                                            CreatedOn = _order.CreatedOn,
                                            PaymentDate = _order.PaymentDate,
                                            AmountVnd = _order.AmountVnd,

                                            OrderMapId = _order.OrderMapId,
                                            ClientId = _order.ClientId,
                                            AddressId = _order.AddressId
                                        }).FirstOrDefaultAsync();
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetail - OrderDAL: " + ex);
                return null;
            }
        }
    }
}
