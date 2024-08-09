using Aspose.Cells;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Affiliate;
using Entities.ViewModels.Orders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using static Utilities.Contants.OrderConstants;

namespace Repositories.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAL _OrderDAL;
        private readonly OrderItemDAL _OrderItemDAL;
        private readonly VoucherDAL _VoucherDAL;
        private readonly ClientDAL _ClientDAL;
        private readonly NoteDAL _NoteDAL;
        private readonly PaymentDAL _PaymentDAL;
        private readonly CashbackDAL _CashbackDAL;
        private readonly LabelDAL _LabelDAL;
        private readonly AddressClientDAL _AddressClientDAL;

        public OrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _OrderDAL = new OrderDAL(_StrConnection);
            _OrderItemDAL = new OrderItemDAL(_StrConnection);
            _ClientDAL = new ClientDAL(_StrConnection);
            _VoucherDAL = new VoucherDAL(_StrConnection);
            _NoteDAL = new NoteDAL(_StrConnection);
            _PaymentDAL = new PaymentDAL(_StrConnection);
            _CashbackDAL = new CashbackDAL(_StrConnection);
            _LabelDAL = new LabelDAL(_StrConnection);
        }

        public async Task<long> CreateOrder(OrderViewModel order, List<OrderItemViewModel> orderItems, List<NoteModel> notes)
        {
            try
            {
                long rs = 0;
                long _VoucherId = -1;

                if (!string.IsNullOrEmpty(order.Voucher))
                {
                    var VoucherModel = await _VoucherDAL.FindByVoucherCode(order.Voucher);
                    if (VoucherModel != null)
                    {
                        _VoucherId = VoucherModel.Id;
                    }
                }

                var _OrderModel = await _OrderDAL.FindByOrderNo(order.OrderNo);

                if (_OrderModel != null)
                {
                    _OrderModel.PaymentType = order.PaymentType;
                    _OrderModel.PaymentStatus = order.PaymentStatus;
                    _OrderModel.OrderStatus = order.OrderStatus;
                    _OrderModel.Note = order.Note;
                    _OrderModel.ClientName = order.ClientName;
                    _OrderModel.Address = order.Address;
                    _OrderModel.Phone = order.Phone;
                    _OrderModel.Email = order.Email;
                    _OrderModel.VoucherId = _VoucherId;
                    _OrderModel.UpdateLast = order.CreatedOn;
                    _OrderModel.PaymentDate = order.PaymentDate;
                    await _OrderDAL.UpdateAsync(_OrderModel);

                    // UpSert Note for order and order item
                    await _NoteDAL.MultipleInsertAsync(notes, _OrderModel.Id);
                    rs = _OrderModel.Id;
                }
                else
                {
                    double _TotalShippingFeeUsd = order.TotalShippingFeeUsd ?? 0;
                    double _TotalShippingFeeVnd = order.TotalShippingFeeVnd ?? 0;

                    if (orderItems != null && orderItems.Count > 0)
                    {
                        _TotalShippingFeeUsd = orderItems.Sum(s => (s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity);
                        _TotalShippingFeeVnd = _TotalShippingFeeUsd * (double)order.RateCurrent;
                    }

                    var _orderModel = new Order
                    {
                        ClientId = order.ClientId,
                        UserId = order.UserId,
                        LabelId = order.LabelId,
                        OrderNo = order.OrderNo,
                        ClientName = order.ClientName,
                        Email = order.Email,
                        Phone = order.Phone,
                        Address = order.Address,
                        CreatedOn = order.CreatedOn,
                        UpdateLast = order.CreatedOn,
                        RateCurrent = order.RateCurrent,
                        PriceVnd = order.PriceVnd,
                        AmountVnd = order.AmountVnd,
                        TotalDiscount2ndVnd = order.TotalDiscount2ndVnd,
                        TotalShippingFeeVnd = _TotalShippingFeeVnd,
                        TotalShippingFeeUsd = _TotalShippingFeeUsd,
                        TotalDiscountVoucherVnd = order.TotalDiscountVoucherVnd,
                        VoucherId = string.IsNullOrEmpty(order.Voucher) ? -1 : _VoucherId,
                        VoucherName = order.VoucherName == null ? _OrderModel.VoucherName : order.VoucherName, // cuonglv moi bo sung. truong nay luu danh sach voucher phan cach dau phay
                        Discount = order.Discount,
                        PriceUsd = order.PriceUsd,
                        AmountUsd = order.AmountUsd,
                        TotalDiscount2ndUsd = order.TotalDiscount2ndUsd,
                        TotalDiscountVoucherUsd = order.TotalDiscountVoucherUsd,
                        Note = order.Note,
                        PaymentType = order.PaymentType,
                        PaymentStatus = order.PaymentStatus,
                        PaymentDate = order.PaymentDate,
                        OrderStatus = order.OrderStatus,
                        TrackingId = order.TrackingId,
                        UtmMedium = order.UtmMedium,
                        UtmCampaign = order.UtmCampaign,
                        UtmFirstTime = order.UtmFirstTime,
                        UtmSource = order.UtmSource,
                        Version = 2,
                        AddressId = order.AddressId
                    };

                    rs = await _OrderDAL.CreateAsync(_orderModel);

                    if (rs > 0)
                    {
                        // Update order count for client
                        var clientModel = await _ClientDAL.FindAsync(_orderModel.ClientId);
                        if (clientModel != null)
                        {
                            clientModel.TotalOrder = clientModel.TotalOrder == null ? 1 : (clientModel.TotalOrder + 1);
                            await _ClientDAL.UpdateAsync(clientModel);
                        }

                        #region UpSert order item list
                        var ListOrderItem = orderItems.Select(item => new OrderItem
                        {
                            ProductId = item.ProductId,
                            OrderId = rs,
                            Price = item.Price,
                            FirstPoundFee = item.FirstPoundFee,
                            DiscountShippingFirstPound = item.DiscountShippingFirstPound,
                            NextPoundFee = item.NextPoundFee,
                            LuxuryFee = item.LuxuryFee,
                            ShippingFeeUs = item.ShippingFeeUs,
                            Quantity = item.Quantity,
                            CreateOn = DateTime.Now,
                            UpdateLast = DateTime.Now,
                            ImageThumb = item.ProductImage,
                            OrderItempMapId = item.OrderItemMapId,
                            SpecialLuxuryId = item.SpecialLuxuryId,
                            Weight = item.Weight
                        }).ToList();

                        await _OrderItemDAL.MultipleInsertAsync(ListOrderItem);

                        // Update product quantity in Elasticsearch 

                        #endregion

                    }
                }

                if (rs > 0) // cuonglv update: chỉ cho đơn được tạo mới từ us old mới cho cập nhật lại orderdetail
                {
                    #region Upsert first-time payment
                    if (order.PaymentStatus == (int)Payment_Status.DA_THANH_TOAN)
                    {
                        var IsCreated = false;
                        var PaymentFirstModel = await _PaymentDAL.GetFirstPaymentOrder(rs);
                        if (PaymentFirstModel != null)
                        {
                            if (PaymentFirstModel.Amount != order.AmountVnd)
                            {
                                IsCreated = true;
                                await _PaymentDAL.DeleteAsync(PaymentFirstModel.Id);
                            }
                        }
                        else
                        {
                            IsCreated = true;
                        }

                        if (IsCreated)
                        {
                            var paymentModel = new Payment()
                            {
                                OrderId = rs,
                                Amount = (double)order.AmountVnd,
                                PaymentDate = (DateTime)order.PaymentDate,
                                PaymentType = (int)order.PaymentType,
                                UserId = 36, // admin
                                CreatedOn = DateTime.Now,
                                ModifiedOn = DateTime.Now
                            };
                            await _PaymentDAL.CreateAsync(paymentModel);
                        }
                    }
                    #endregion
                }

                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateOrder in OrderRepository" + ex);
                return 0;
            }
        }

        public async Task<OrderViewModel> GetOrderDetail(long Id)
        {
            try
            {
                return await _OrderDAL.GetOrderDetail(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetail in OrderRepository" + ex);
                return null;
            }
        }

        public async Task<List<OrderItemViewModel>> GetOrderItemList(long Id)
        {
            try
            {
                return await _OrderDAL.GetOrderItemList(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderItemList in OrderRepository" + ex);
                return null;
            }
        }

        public GenericViewModel<OrderGridModel> GetPagingList(OrderSearchModel searchModel, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<OrderGridModel>();

            try
            {
                DataTable dt = _OrderDAL.GetPagingList(searchModel, currentPage, pageSize);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model.ListData = (from row in dt.AsEnumerable()
                                      select new OrderGridModel
                                      {
                                          Id = Convert.ToInt64(row["Id"]),
                                          OrderNo = row["OrderNo"].ToString(),
                                          ClientId = Convert.ToInt64(!row["ClientId"].Equals(DBNull.Value) ? row["ClientId"] : 0),
                                          ClientName = row["ClientName"].ToString(),
                                          Email = row["Email"].ToString(),
                                          Phone = row["Phone"].ToString(),
                                          Address = row["Address"].ToString(),
                                          CreatedOn = Convert.ToDateTime(!row["CreatedOn"].Equals(DBNull.Value) ? row["CreatedOn"] : null),
                                          RateCurrent = Convert.ToDouble(!row["RateCurrent"].Equals(DBNull.Value) ? row["RateCurrent"] : 0),
                                          PriceUsd = Convert.ToDouble(!row["PriceUsd"].Equals(DBNull.Value) ? row["PriceUsd"] : 0),
                                          PriceVnd = Convert.ToDouble(!row["PriceVnd"].Equals(DBNull.Value) ? row["PriceVnd"] : 0),
                                          Discount = Convert.ToDouble(!row["Discount"].Equals(DBNull.Value) ? row["Discount"] : 0),
                                          AmountUsd = Convert.ToDouble(!row["AmountUsd"].Equals(DBNull.Value) ? row["AmountUsd"] : 0),
                                          AmountVnd = Convert.ToDouble(!row["AmountVnd"].Equals(DBNull.Value) ? row["AmountVnd"] : 0),
                                          PaymentDate = Convert.ToDateTime(!row["PaymentDate"].Equals(DBNull.Value) ? row["PaymentDate"] : null),
                                          TotalDiscount2ndUsd = Convert.ToDouble(!row["TotalDiscount2ndUsd"].Equals(DBNull.Value) ? row["TotalDiscount2ndUsd"] : 0),
                                          TotalDiscount2ndVnd = Convert.ToDouble(!row["TotalDiscount2ndVnd"].Equals(DBNull.Value) ? row["TotalDiscount2ndVnd"] : 0),
                                          TotalDiscountVoucherUsd = Convert.ToDouble(!row["TotalDiscountVoucherUsd"].Equals(DBNull.Value) ? row["TotalDiscountVoucherUsd"] : 0),
                                          TotalDiscountVoucherVnd = Convert.ToDouble(!row["TotalDiscountVoucherVnd"].Equals(DBNull.Value) ? row["TotalDiscountVoucherVnd"] : 0),
                                          TotalShippingFeeUsd = Convert.ToDouble(!row["TotalShippingFeeUsd"].Equals(DBNull.Value) ? row["TotalShippingFeeUsd"] : 0),
                                          TotalShippingFeeVnd = Convert.ToDouble(!row["TotalShippingFeeVnd"].Equals(DBNull.Value) ? row["TotalShippingFeeVnd"] : 0),
                                          TrackingId = row["TrackingId"].ToString(),
                                          UtmSource = row["UtmSource"].ToString(),
                                          VoucherCode = row["VoucherCode"].ToString(),
                                          OrderStatusName = row["OrderStatusName"].ToString(),
                                          PaymentStatusName = row["PaymentStatusName"].ToString(),
                                          PaymentTypeName = row["PaymentTypeName"].ToString()
                                      }).ToList();
                    model.CurrentPage = currentPage;
                    model.PageSize = pageSize;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / pageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList in OrderRepository" + ex);
            }
            return model;
        }

        /// <summary>
        /// Report Order
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public string ReportOrder(OrderSearchModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                DataSet dataSet = _OrderDAL.GetOrderReport(searchModel);
                var ListOrder = new List<OrderReportModel>();
                var ListOrderItem = new List<OrderItemReportModel>();

                if (dataSet != null)
                {
                    ListOrder = (from dr in dataSet.Tables[0].AsEnumerable()
                                 select new OrderReportModel
                                 {
                                     OrderId = Convert.ToInt64(dr["OrderId"].Equals(DBNull.Value) ? 0 : dr["OrderId"]),
                                     OrderNo = dr["OrderNo"].ToString(),
                                     TotalDiscount2ndVnd = Convert.ToDouble(dr["TotalDiscount2ndVnd"].Equals(DBNull.Value) ? 0 : dr["TotalDiscount2ndVnd"]),
                                     TotalDiscountVoucherVnd = Convert.ToDouble(dr["TotalDiscountVoucherVnd"].Equals(DBNull.Value) ? 0 : dr["TotalDiscountVoucherVnd"]),
                                     PaymentAmount = Convert.ToDouble(dr["PaymentAmount"].Equals(DBNull.Value) ? 0 : dr["PaymentAmount"]),
                                     CashbackAmount = Convert.ToDouble(dr["CashbackAmount"].Equals(DBNull.Value) ? 0 : dr["CashbackAmount"]),
                                     OrderAmount = Convert.ToDouble(dr["OrderAmount"].Equals(DBNull.Value) ? 0 : dr["OrderAmount"]),
                                     RateCurrent = Convert.ToDouble(dr["RateCurrent"].Equals(DBNull.Value) ? 0 : dr["RateCurrent"]),
                                     PaymentDate = Convert.ToDateTime(dr["PaymentDate"].Equals(DBNull.Value) ? DateTime.MinValue : dr["PaymentDate"]),
                                     StoreName = dr["StoreName"].ToString(),
                                     UtmSource = dr["UtmSource"].ToString(),
                                     UtmMedium = dr["UtmMedium"].ToString(),
                                     OrderStatusName = dr["OrderStatusName"].ToString(),
                                     USExpressAff = Convert.ToInt32(dr["USExpressAff"].Equals(DBNull.Value) ? 0 : dr["USExpressAff"]),
                                     CustomerName = dr["CustomerName"].ToString(),
                                     Address = dr["Address"].ToString(),
                                     USExpressAffEmail = _ClientDAL.GetClientEmailByRefferalCode(dr["UtmMedium"].ToString())
                                 }).ToList();

                    ListOrderItem = (from dr in dataSet.Tables[1].AsEnumerable()
                                     select new OrderItemReportModel
                                     {
                                         Id = Convert.ToInt64(dr["Id"].Equals(DBNull.Value) ? 0 : dr["Id"]),
                                         OrderId = Convert.ToInt64(dr["OrderId"].Equals(DBNull.Value) ? 0 : dr["OrderId"]),
                                         Price = Convert.ToDouble(dr["Price"].Equals(DBNull.Value) ? 0 : dr["Price"]),
                                         Quantity = Convert.ToInt32(dr["Quantity"].Equals(DBNull.Value) ? 0 : dr["Quantity"]),
                                         Weight = Convert.ToDouble(dr["Weight"].Equals(DBNull.Value) ? 0 : dr["Weight"]),
                                         FirstPoundFee = Convert.ToDouble(dr["FirstPoundFee"].Equals(DBNull.Value) ? 0 : dr["FirstPoundFee"]),
                                         NextPoundFee = Convert.ToDouble(dr["NextPoundFee"].Equals(DBNull.Value) ? 0 : dr["NextPoundFee"]),
                                         LuxuryFee = Convert.ToDouble(dr["LuxuryFee"].Equals(DBNull.Value) ? 0 : dr["LuxuryFee"])
                                     }).ToList();
                }

                if (ListOrder.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Báo cáo đơn hàng";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 28);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set header value
                    ws.Cells["A1"].PutValue("Mã đơn");
                    ws.Cells["B1"].PutValue("Số Lượng SP");
                    ws.Cells["C1"].PutValue("Cân nặng (Pounds)");
                    ws.Cells["D1"].PutValue("Giá bán ($)");
                    ws.Cells["E1"].PutValue("Tổng Phí mua hộ ($)");
                    ws.Cells["F1"].PutValue("Thành tiền ($)");
                    ws.Cells["G1"].PutValue("Rate");
                    ws.Cells["H1"].PutValue("Thành tiền (VNĐ)");
                    ws.Cells["I1"].PutValue("Thành tiền đã trừ Voucher($)");
                    ws.Cells["J1"].PutValue("Số tiền giảm trên phí mua hộ (VNĐ)");
                    ws.Cells["K1"].PutValue("Giảm giá của Voucher (VNĐ)");
                    ws.Cells["L1"].PutValue("Giảm giá của Voucher ($)");
                    ws.Cells["M1"].PutValue("Tổng tiền được giảm giá ($)");
                    ws.Cells["N1"].PutValue("Tổng số lần thanh toán");
                    ws.Cells["O1"].PutValue("Tổng hoàn tiền");
                    ws.Cells["P1"].PutValue("Tổng giá trị đơn hàng");
                    ws.Cells["Q1"].PutValue("Ngày thanh toán");
                    ws.Cells["R1"].PutValue("Store name");
                    ws.Cells["S1"].PutValue("UTM Source");
                    ws.Cells["T1"].PutValue("UTM Medium");
                    ws.Cells["U1"].PutValue("Trạng thái đơn hàng");
                    ws.Cells["V1"].PutValue("USExpress Affiliate");
                    ws.Cells["W1"].PutValue("Customer Name");
                    ws.Cells["X1"].PutValue("Address");
                    ws.Cells["Y1"].PutValue("USExpress Affiliate Email");
                    ws.Cells["Z1"].PutValue("Phí mua hộ pound đầu tiên ($)");
                    ws.Cells["AA1"].PutValue("Phí mua hộ pound tiếp theo ($)");
                    ws.Cells["AB1"].PutValue("Phí phụ thu sản phẩm đặc biệt ($)");

                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, ListOrder.Count + 1, 28);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignRightStyle = ws.Cells["C2"].GetStyle();
                    alignRightStyle.HorizontalAlignment = TextAlignmentType.Right;
                    alignRightStyle.VerticalAlignment = TextAlignmentType.Center;

                    Style centerStyle = ws.Cells["A2"].GetStyle();
                    alignRightStyle.HorizontalAlignment = TextAlignmentType.Center;
                    alignRightStyle.VerticalAlignment = TextAlignmentType.Center;

                    Style currencyStyle = ws.Cells["D2"].GetStyle();
                    currencyStyle.Custom = "#,##0.00";
                    currencyStyle.HorizontalAlignment = TextAlignmentType.Right;
                    currencyStyle.VerticalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["M2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    double Total_Quantity = 0;
                    double Total_Weight = 0;
                    double Total_Price = 0;
                    double Total_ShippingFee = 0;
                    double Total_AmountUSD = 0;
                    double Total_AmountWithoutVoucherUSD = 0;
                    double Total_Amount_Discount = 0;
                    double Total_AmountVND = 0;
                    double Total_Discount2ndVnd = 0;
                    double Total_DiscountVoucherVnd = 0;
                    double Total_DiscountVoucherUSD = 0;
                    double Total_PaymentAmount = 0;
                    double Total_CashbackAmount = 0;
                    double Total_OrderAmount = 0;
                    double Total_FirstPoundFee = 0;
                    double Total_NextPoundFee = 0;
                    double Total_SpecialFee = 0;

                    foreach (var item in ListOrder)
                    {
                        var ListItem = ListOrderItem.Where(s => s.OrderId == item.OrderId).ToList();

                        double _Price = 0;
                        double _ShippingFee = 0;
                        double _AmountUSD = 0;
                        double _AmountVND = 0;
                        double _first_pound_fee = 0;
                        double _next_pound_fee = 0;
                        double _special_industry_fee = 0;
                        RowIndex++;
                        var RowOrderBegin = RowIndex;

                        ws.Cells["A" + RowIndex].PutValue(item.OrderNo);

                        if (ListItem != null && ListItem.Count > 0)
                        {
                            _Price = Math.Round(ListItem.Sum(s => s.Price * s.Quantity), 2);
                            _ShippingFee = Math.Round(ListItem.Sum(s => (s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity), 2);
                            _AmountUSD = Math.Round(ListItem.Sum(s => (s.Price + s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity), 2);
                            _AmountVND = Math.Round(ListItem.Sum(s => (s.Price + s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity * item.RateCurrent));
                            _first_pound_fee = ListItem.Sum(s => (s.FirstPoundFee) * s.Quantity);
                            _next_pound_fee = ListItem.Sum(s => (s.NextPoundFee) * s.Quantity);
                            _special_industry_fee = ListItem.Sum(s => (s.LuxuryFee) * s.Quantity);

                            Total_Quantity += ListItem.Sum(s => s.Quantity);
                            ws.Cells["B" + RowIndex].PutValue(ListItem.Sum(s => s.Quantity));
                            ws.Cells["B" + RowIndex].SetStyle(numberStyle);

                            Total_Weight += ListItem.Sum(s => s.Weight);
                            ws.Cells["C" + RowIndex].PutValue(StringHelpers.FormatWeight(ListItem.Sum(s => s.Weight)));
                            ws.Cells["C" + RowIndex].SetStyle(numberStyle);

                            Total_Price += _Price;
                            ws.Cells["D" + RowIndex].PutValue(_Price);
                            ws.Cells["D" + RowIndex].SetStyle(currencyStyle);

                            Total_ShippingFee += _ShippingFee;
                            ws.Cells["E" + RowIndex].PutValue(_ShippingFee);
                            ws.Cells["E" + RowIndex].SetStyle(currencyStyle);

                            Total_AmountUSD += _AmountUSD;
                            ws.Cells["F" + RowIndex].PutValue(_AmountUSD);
                            ws.Cells["F" + RowIndex].SetStyle(currencyStyle);

                            ws.Cells["G" + RowIndex].PutValue(item.RateCurrent);
                            ws.Cells["G" + RowIndex].SetStyle(numberStyle);

                            Total_AmountVND += _AmountVND;
                            ws.Cells["H" + RowIndex].PutValue(_AmountVND);
                            ws.Cells["H" + RowIndex].SetStyle(numberStyle);
                        }


                        Total_AmountWithoutVoucherUSD += (_AmountUSD - (item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent);
                        ws.Cells["I" + RowIndex].PutValue(Math.Round(_AmountUSD - (item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent, 2));
                        ws.Cells["I" + RowIndex].SetStyle(currencyStyle);

                        Total_Discount2ndVnd += item.TotalDiscount2ndVnd;
                        ws.Cells["J" + RowIndex].PutValue(item.TotalDiscount2ndVnd);
                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);

                        Total_DiscountVoucherVnd += item.TotalDiscountVoucherVnd;
                        ws.Cells["K" + RowIndex].PutValue(item.TotalDiscountVoucherVnd);
                        ws.Cells["K" + RowIndex].SetStyle(numberStyle);

                        Total_DiscountVoucherUSD += item.TotalDiscountVoucherVnd / item.RateCurrent;
                        ws.Cells["L" + RowIndex].PutValue(Math.Round(item.TotalDiscountVoucherVnd / item.RateCurrent, 2));
                        ws.Cells["L" + RowIndex].SetStyle(currencyStyle);


                        Total_Amount_Discount += (item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent;
                        ws.Cells["M" + RowIndex].PutValue(Math.Round((item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent, 2));
                        ws.Cells["M" + RowIndex].SetStyle(currencyStyle);

                        Total_PaymentAmount += item.PaymentAmount;
                        ws.Cells["N" + RowIndex].PutValue(item.PaymentAmount);
                        ws.Cells["N" + RowIndex].SetStyle(numberStyle);

                        Total_CashbackAmount += item.CashbackAmount;
                        ws.Cells["O" + RowIndex].PutValue(item.CashbackAmount);
                        ws.Cells["O" + RowIndex].SetStyle(numberStyle);

                        var _OrderAmount = item.PaymentAmount > 0 ? (item.PaymentAmount - item.CashbackAmount) : (_AmountVND - item.TotalDiscount2ndVnd - item.TotalDiscountVoucherVnd);
                        Total_OrderAmount += _OrderAmount;
                        ws.Cells["P" + RowIndex].PutValue(_OrderAmount);
                        ws.Cells["P" + RowIndex].SetStyle(numberStyle);

                        ws.Cells["Q" + RowIndex].PutValue(item.PaymentDate.Year > 2016 ? item.PaymentDate.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                        ws.Cells["R" + RowIndex].PutValue(item.StoreName);
                        ws.Cells["S" + RowIndex].PutValue(item.UtmSource);
                        ws.Cells["T" + RowIndex].PutValue(item.UtmMedium);
                        ws.Cells["U" + RowIndex].PutValue(item.OrderStatusName);
                        ws.Cells["V" + RowIndex].PutValue(item.USExpressAff == 1 ? true : false);
                        ws.Cells["W" + RowIndex].PutValue(item.CustomerName);
                        ws.Cells["X" + RowIndex].PutValue(item.Address);
                        ws.Cells["Y" + RowIndex].PutValue(item.USExpressAffEmail);

                        Total_FirstPoundFee += _first_pound_fee;
                        ws.Cells["Z" + RowIndex].PutValue(_first_pound_fee);
                        ws.Cells["Z" + RowIndex].SetStyle(currencyStyle);

                        Total_NextPoundFee += _next_pound_fee;
                        ws.Cells["AA" + RowIndex].PutValue(_next_pound_fee);
                        ws.Cells["AA" + RowIndex].SetStyle(currencyStyle);

                        Total_SpecialFee += _special_industry_fee;
                        ws.Cells["AB" + RowIndex].PutValue(_special_industry_fee);
                        ws.Cells["AB" + RowIndex].SetStyle(currencyStyle);
                    }

                    #region total row
                    RowIndex++;

                    numberStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    currencyStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    alignRightStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    centerStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    ws.Cells["A" + RowIndex].PutValue("TỔNG CỘNG");
                    ws.Cells["A" + RowIndex].SetStyle(centerStyle);

                    ws.Cells["B" + RowIndex].PutValue(Total_Quantity);
                    ws.Cells["B" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["C" + RowIndex].PutValue(StringHelpers.FormatWeight(Total_Weight));
                    ws.Cells["C" + RowIndex].SetStyle(alignRightStyle);

                    ws.Cells["D" + RowIndex].PutValue(Total_Price);
                    ws.Cells["D" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["E" + RowIndex].PutValue(Total_ShippingFee);
                    ws.Cells["E" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["F" + RowIndex].PutValue(Total_AmountUSD);
                    ws.Cells["F" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["H" + RowIndex].PutValue(Total_AmountVND);
                    ws.Cells["H" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["I" + RowIndex].PutValue(Total_AmountWithoutVoucherUSD);
                    ws.Cells["I" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["J" + RowIndex].PutValue(Total_Discount2ndVnd);
                    ws.Cells["J" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["K" + RowIndex].PutValue(Total_DiscountVoucherVnd);
                    ws.Cells["K" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["L" + RowIndex].PutValue(Total_DiscountVoucherUSD);
                    ws.Cells["L" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["M" + RowIndex].PutValue(Total_Amount_Discount);
                    ws.Cells["M" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["N" + RowIndex].PutValue(Total_PaymentAmount);
                    ws.Cells["N" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["O" + RowIndex].PutValue(Total_CashbackAmount);
                    ws.Cells["O" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["P" + RowIndex].PutValue(Total_OrderAmount);
                    ws.Cells["P" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["Z" + RowIndex].PutValue(Total_FirstPoundFee);
                    ws.Cells["Z" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["AA" + RowIndex].PutValue(Total_NextPoundFee);
                    ws.Cells["AA" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["AB" + RowIndex].PutValue(Total_SpecialFee);
                    ws.Cells["AB" + RowIndex].SetStyle(currencyStyle);
                    #endregion

                    ws.AutoFitColumns();

                    #endregion

                    ws.Cells.InsertColumn(4);
                    ws.Cells.InsertColumn(5);
                    ws.Cells.InsertColumn(6);
                    ws.Cells.CopyColumn(ws.Cells, ws.Cells.Columns[28].Index, ws.Cells.Columns[4].Index);
                    ws.Cells.CopyColumn(ws.Cells, ws.Cells.Columns[29].Index, ws.Cells.Columns[5].Index);
                    ws.Cells.CopyColumn(ws.Cells, ws.Cells.Columns[30].Index, ws.Cells.Columns[6].Index);
                    ws.Cells.DeleteColumn(28);
                    ws.Cells.DeleteColumn(28);
                    ws.Cells.DeleteColumn(28);

                    wb.Save(FilePath);
                    pathResult = FilePath;
                }

            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("ReportOrder - OrderRepository: " + ex);
            }
            return pathResult;
        }
        public string ExportOrderExpected(string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                DataSet dataSet = _OrderDAL.GetOrderExpectedReport();
                var ListOrder = new List<OrderExpectedExportModel>();
                var ListOrderItem = new List<OrderItemReportModel>();
                var ListLastestProgress = new List<OrderLastestProgressExportModel>();
                if (dataSet != null)
                {
                    ListOrder = (from dr in dataSet.Tables[0].AsEnumerable()
                                 select new OrderExpectedExportModel
                                 {
                                     OrderId = Convert.ToInt64(dr["OrderId"].Equals(DBNull.Value) ? 0 : dr["OrderId"]),
                                     OrderNo = dr["OrderNo"].ToString(),
                                     TotalDiscount2ndVnd = Convert.ToDouble(dr["TotalDiscount2ndVnd"].Equals(DBNull.Value) ? 0 : dr["TotalDiscount2ndVnd"]),
                                     TotalDiscountVoucherVnd = Convert.ToDouble(dr["TotalDiscountVoucherVnd"].Equals(DBNull.Value) ? 0 : dr["TotalDiscountVoucherVnd"]),
                                     PaymentAmount = Convert.ToDouble(dr["PaymentAmount"].Equals(DBNull.Value) ? 0 : dr["PaymentAmount"]),
                                     CashbackAmount = Convert.ToDouble(dr["CashbackAmount"].Equals(DBNull.Value) ? 0 : dr["CashbackAmount"]),
                                     OrderAmount = Convert.ToDouble(dr["OrderAmount"].Equals(DBNull.Value) ? 0 : dr["OrderAmount"]),
                                     RateCurrent = Convert.ToDouble(dr["RateCurrent"].Equals(DBNull.Value) ? 0 : dr["RateCurrent"]),
                                     PaymentDate = Convert.ToDateTime(dr["PaymentDate"].Equals(DBNull.Value) ? DateTime.MinValue : dr["PaymentDate"]),
                                     StoreName = dr["StoreName"].ToString(),
                                     UtmSource = dr["UtmSource"].ToString(),
                                     UtmMedium = dr["UtmMedium"].ToString(),
                                     OrderStatusName = dr["OrderStatusName"].ToString(),
                                     USExpressAff = Convert.ToInt32(dr["USExpressAff"].Equals(DBNull.Value) ? 0 : dr["USExpressAff"]),
                                     CustomerName = dr["CustomerName"].ToString(),
                                     Address = dr["Address"].ToString(),
                                     USExpressAffEmail = _ClientDAL.GetClientEmailByRefferalCode(dr["UtmMedium"].ToString()),
                                     OrderStatus = Convert.ToInt32(dr["OrderStatus"].Equals(DBNull.Value) ? 0 : dr["OrderStatus"]),
                                     Email= dr["Email"].ToString()
                                 }).ToList();

                    ListOrderItem = (from dr in dataSet.Tables[1].AsEnumerable()
                                     select new OrderItemReportModel
                                     {
                                         Id = Convert.ToInt64(dr["Id"].Equals(DBNull.Value) ? 0 : dr["Id"]),
                                         OrderId = Convert.ToInt64(dr["OrderId"].Equals(DBNull.Value) ? 0 : dr["OrderId"]),
                                         Price = Convert.ToDouble(dr["Price"].Equals(DBNull.Value) ? 0 : dr["Price"]),
                                         Quantity = Convert.ToInt32(dr["Quantity"].Equals(DBNull.Value) ? 0 : dr["Quantity"]),
                                         Weight = Convert.ToDouble(dr["Weight"].Equals(DBNull.Value) ? 0 : dr["Weight"]),
                                         FirstPoundFee = Convert.ToDouble(dr["FirstPoundFee"].Equals(DBNull.Value) ? 0 : dr["FirstPoundFee"]),
                                         NextPoundFee = Convert.ToDouble(dr["NextPoundFee"].Equals(DBNull.Value) ? 0 : dr["NextPoundFee"]),
                                         LuxuryFee = Convert.ToDouble(dr["LuxuryFee"].Equals(DBNull.Value) ? 0 : dr["LuxuryFee"])
                                     }).ToList();
                    ListLastestProgress = (from dr in dataSet.Tables[2].AsEnumerable()
                                     select new OrderLastestProgressExportModel
                                     {
                                        OrderNo= dr["OrderNo"].ToString(),
                                        LastestOrderProgressDay = Convert.ToInt32(dr["LastestOrderProgressDay"].Equals(DBNull.Value) ? 0 : dr["LastestOrderProgressDay"]),
                                        TotalOrderProgressDay = Convert.ToInt32(dr["TotalOrderProgressDay"].Equals(DBNull.Value) ? 0 : dr["TotalOrderProgressDay"]),

                                     }).ToList();
                }

                if (ListOrder.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Báo cáo đơn hàng";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 31);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set header value
                    ws.Cells["A1"].PutValue("Mã đơn");
                    ws.Cells["B1"].PutValue("Số Lượng SP");
                    ws.Cells["C1"].PutValue("Cân nặng (Pounds)");
                    ws.Cells["D1"].PutValue("Giá bán ($)");
                    ws.Cells["E1"].PutValue("Tổng Phí mua hộ ($)");
                    ws.Cells["F1"].PutValue("Thành tiền ($)");
                    ws.Cells["G1"].PutValue("Rate");
                    ws.Cells["H1"].PutValue("Thành tiền (VNĐ)");
                    ws.Cells["I1"].PutValue("Thành tiền đã trừ Voucher($)");
                    ws.Cells["J1"].PutValue("Số tiền giảm trên phí mua hộ (VNĐ)");
                    ws.Cells["K1"].PutValue("Giảm giá của Voucher (VNĐ)");
                    ws.Cells["L1"].PutValue("Giảm giá của Voucher ($)");
                    ws.Cells["M1"].PutValue("Tổng tiền được giảm giá ($)");
                    ws.Cells["N1"].PutValue("Tổng số lần thanh toán");
                    ws.Cells["O1"].PutValue("Tổng hoàn tiền");
                    ws.Cells["P1"].PutValue("Tổng giá trị đơn hàng");
                    ws.Cells["Q1"].PutValue("Ngày thanh toán");
                    ws.Cells["R1"].PutValue("Store name");
                    ws.Cells["S1"].PutValue("UTM Source");
                    ws.Cells["T1"].PutValue("UTM Medium");
                    ws.Cells["U1"].PutValue("Trạng thái đơn hàng");
                    ws.Cells["V1"].PutValue("USExpress Affiliate");
                    ws.Cells["W1"].PutValue("Customer Name");
                    ws.Cells["X1"].PutValue("Address");
                    ws.Cells["Y1"].PutValue("USExpress Affiliate Email");
                    ws.Cells["Z1"].PutValue("Phí mua hộ pound đầu tiên ($)");
                    ws.Cells["AA1"].PutValue("Phí mua hộ pound tiếp theo ($)");
                    ws.Cells["AB1"].PutValue("Phí phụ thu sản phẩm đặc biệt ($)");
                    ws.Cells["AC1"].PutValue("Số ngày trễ hạn tính từ trạng thái mới nhất (Cột U)");
                    ws.Cells["AD1"].PutValue("Tổng số ngày trễ hạn tính từ lúc thanh toán");
                    ws.Cells["AE1"].PutValue("Email");

                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, ListOrder.Count + 1, 31);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignRightStyle = ws.Cells["C2"].GetStyle();
                    alignRightStyle.HorizontalAlignment = TextAlignmentType.Right;
                    alignRightStyle.VerticalAlignment = TextAlignmentType.Center;

                    Style centerStyle = ws.Cells["A2"].GetStyle();
                    alignRightStyle.HorizontalAlignment = TextAlignmentType.Center;
                    alignRightStyle.VerticalAlignment = TextAlignmentType.Center;

                    Style currencyStyle = ws.Cells["D2"].GetStyle();
                    currencyStyle.Custom = "#,##0.00";
                    currencyStyle.HorizontalAlignment = TextAlignmentType.Right;
                    currencyStyle.VerticalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["M2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    double Total_Quantity = 0;
                    double Total_Weight = 0;
                    double Total_Price = 0;
                    double Total_ShippingFee = 0;
                    double Total_AmountUSD = 0;
                    double Total_AmountWithoutVoucherUSD = 0;
                    double Total_Amount_Discount = 0;
                    double Total_AmountVND = 0;
                    double Total_Discount2ndVnd = 0;
                    double Total_DiscountVoucherVnd = 0;
                    double Total_DiscountVoucherUSD = 0;
                    double Total_PaymentAmount = 0;
                    double Total_CashbackAmount = 0;
                    double Total_OrderAmount = 0;
                    double Total_FirstPoundFee = 0;
                    double Total_NextPoundFee = 0;
                    double Total_SpecialFee = 0;

                    foreach (var item in ListOrder)
                    {

                        var ListItem = ListOrderItem.Where(s => s.OrderId == item.OrderId).ToList();
                        var LastestProgress = ListLastestProgress.Where(s => s.OrderNo.Trim() == item.OrderNo.Trim()).FirstOrDefault();

                        double _Price = 0;
                        double _ShippingFee = 0;
                        double _AmountUSD = 0;
                        double _AmountVND = 0;
                        double _first_pound_fee = 0;
                        double _next_pound_fee = 0;
                        double _special_industry_fee = 0;
                        var RowOrderBegin = RowIndex;

                        //-- Exprire Day Cal:
                        int totaldayexprire = 0, lasteststatusexprire = 0;
                        if (LastestProgress != null && LastestProgress.OrderNo.Trim() != "")
                        {
                            switch (item.OrderStatus)
                            {
                                case 6:
                                    {
                                        lasteststatusexprire = (LastestProgress.LastestOrderProgressDay - 6) <= 0 ? 0 : (LastestProgress.LastestOrderProgressDay - 6);

                                    }
                                    break;
                                case 13:
                                    {
                                        lasteststatusexprire = (LastestProgress.LastestOrderProgressDay - 1) <= 0 ? 0 : (LastestProgress.LastestOrderProgressDay - 1);

                                    }
                                    break;
                                case 7:
                                    {
                                        lasteststatusexprire = (LastestProgress.LastestOrderProgressDay - 7) <= 0 ? 0 : (LastestProgress.LastestOrderProgressDay - 7);

                                    }
                                    break;
                                case 10:
                                    {
                                        lasteststatusexprire = (LastestProgress.LastestOrderProgressDay - 5) <= 0 ? 0 : (LastestProgress.LastestOrderProgressDay - 5);

                                    }
                                    break;
                                case 11:
                                    {
                                        lasteststatusexprire = (LastestProgress.LastestOrderProgressDay - 5) <= 0 ? 0 : (LastestProgress.LastestOrderProgressDay - 5);

                                    }
                                    break;
                                case 16:
                                    {
                                        lasteststatusexprire = (LastestProgress.LastestOrderProgressDay - 7) <= 0 ? 0 : (LastestProgress.LastestOrderProgressDay - 7);

                                    }
                                    break;
                                default:
                                    {

                                    }
                                    break;
                            }
                            totaldayexprire = (LastestProgress.TotalOrderProgressDay - 14) <= 0 ? 0 : (LastestProgress.TotalOrderProgressDay - 14);
                            if (totaldayexprire <= 0 && lasteststatusexprire <= 0)
                            {
                                continue;
                            }
                            RowIndex++;
                            ws.Cells["A" + RowIndex].PutValue(item.OrderNo);
                            if (ListItem != null && ListItem.Count > 0)
                            {
                                _Price = Math.Round(ListItem.Sum(s => s.Price * s.Quantity), 2);
                                _ShippingFee = Math.Round(ListItem.Sum(s => (s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity), 2);
                                _AmountUSD = Math.Round(ListItem.Sum(s => (s.Price + s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity), 2);
                                _AmountVND = Math.Round(ListItem.Sum(s => (s.Price + s.FirstPoundFee + s.NextPoundFee + s.LuxuryFee) * s.Quantity * item.RateCurrent));
                                _first_pound_fee = ListItem.Sum(s => (s.FirstPoundFee) * s.Quantity);
                                _next_pound_fee = ListItem.Sum(s => (s.NextPoundFee) * s.Quantity);
                                _special_industry_fee = ListItem.Sum(s => (s.LuxuryFee) * s.Quantity);

                                Total_Quantity += ListItem.Sum(s => s.Quantity);
                                ws.Cells["B" + RowIndex].PutValue(ListItem.Sum(s => s.Quantity));
                                ws.Cells["B" + RowIndex].SetStyle(numberStyle);

                                Total_Weight += ListItem.Sum(s => s.Weight);
                                ws.Cells["C" + RowIndex].PutValue(StringHelpers.FormatWeight(ListItem.Sum(s => s.Weight)));
                                ws.Cells["C" + RowIndex].SetStyle(numberStyle);

                                Total_Price += _Price;
                                ws.Cells["D" + RowIndex].PutValue(_Price);
                                ws.Cells["D" + RowIndex].SetStyle(currencyStyle);

                                Total_ShippingFee += _ShippingFee;
                                ws.Cells["E" + RowIndex].PutValue(_ShippingFee);
                                ws.Cells["E" + RowIndex].SetStyle(currencyStyle);

                                Total_AmountUSD += _AmountUSD;
                                ws.Cells["F" + RowIndex].PutValue(_AmountUSD);
                                ws.Cells["F" + RowIndex].SetStyle(currencyStyle);

                                ws.Cells["G" + RowIndex].PutValue(item.RateCurrent);
                                ws.Cells["G" + RowIndex].SetStyle(numberStyle);

                                Total_AmountVND += _AmountVND;
                                ws.Cells["H" + RowIndex].PutValue(_AmountVND);
                                ws.Cells["H" + RowIndex].SetStyle(numberStyle);
                            }


                            Total_AmountWithoutVoucherUSD += (_AmountUSD - (item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent);
                            ws.Cells["I" + RowIndex].PutValue(Math.Round(_AmountUSD - (item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent, 2));
                            ws.Cells["I" + RowIndex].SetStyle(currencyStyle);

                            Total_Discount2ndVnd += item.TotalDiscount2ndVnd;
                            ws.Cells["J" + RowIndex].PutValue(item.TotalDiscount2ndVnd);
                            ws.Cells["J" + RowIndex].SetStyle(numberStyle);

                            Total_DiscountVoucherVnd += item.TotalDiscountVoucherVnd;
                            ws.Cells["K" + RowIndex].PutValue(item.TotalDiscountVoucherVnd);
                            ws.Cells["K" + RowIndex].SetStyle(numberStyle);

                            Total_DiscountVoucherUSD += item.TotalDiscountVoucherVnd / item.RateCurrent;
                            ws.Cells["L" + RowIndex].PutValue(Math.Round(item.TotalDiscountVoucherVnd / item.RateCurrent, 2));
                            ws.Cells["L" + RowIndex].SetStyle(currencyStyle);


                            Total_Amount_Discount += (item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent;
                            ws.Cells["M" + RowIndex].PutValue(Math.Round((item.TotalDiscountVoucherVnd + item.TotalDiscount2ndVnd) / item.RateCurrent, 2));
                            ws.Cells["M" + RowIndex].SetStyle(currencyStyle);

                            Total_PaymentAmount += item.PaymentAmount;
                            ws.Cells["N" + RowIndex].PutValue(item.PaymentAmount);
                            ws.Cells["N" + RowIndex].SetStyle(numberStyle);

                            Total_CashbackAmount += item.CashbackAmount;
                            ws.Cells["O" + RowIndex].PutValue(item.CashbackAmount);
                            ws.Cells["O" + RowIndex].SetStyle(numberStyle);

                            var _OrderAmount = item.PaymentAmount > 0 ? (item.PaymentAmount - item.CashbackAmount) : (_AmountVND - item.TotalDiscount2ndVnd - item.TotalDiscountVoucherVnd);
                            Total_OrderAmount += _OrderAmount;
                            ws.Cells["P" + RowIndex].PutValue(_OrderAmount);
                            ws.Cells["P" + RowIndex].SetStyle(numberStyle);

                            ws.Cells["Q" + RowIndex].PutValue(item.PaymentDate.Year > 2016 ? item.PaymentDate.ToString("dd/MM/yyyy HH:mm") : string.Empty);
                            ws.Cells["R" + RowIndex].PutValue(item.StoreName);
                            ws.Cells["S" + RowIndex].PutValue(item.UtmSource);
                            ws.Cells["T" + RowIndex].PutValue(item.UtmMedium);
                            ws.Cells["U" + RowIndex].PutValue(item.OrderStatusName);
                            ws.Cells["V" + RowIndex].PutValue(item.USExpressAff == 1 ? true : false);
                            ws.Cells["W" + RowIndex].PutValue(item.CustomerName);
                            ws.Cells["X" + RowIndex].PutValue(item.Address);
                            ws.Cells["Y" + RowIndex].PutValue(item.USExpressAffEmail);

                            Total_FirstPoundFee += _first_pound_fee;
                            ws.Cells["Z" + RowIndex].PutValue(_first_pound_fee);
                            ws.Cells["Z" + RowIndex].SetStyle(currencyStyle);

                            Total_NextPoundFee += _next_pound_fee;
                            ws.Cells["AA" + RowIndex].PutValue(_next_pound_fee);
                            ws.Cells["AA" + RowIndex].SetStyle(currencyStyle);

                            Total_SpecialFee += _special_industry_fee;
                            ws.Cells["AB" + RowIndex].PutValue(_special_industry_fee);
                            ws.Cells["AB" + RowIndex].SetStyle(currencyStyle);


                            ws.Cells["AC" + RowIndex].PutValue(lasteststatusexprire);
                            ws.Cells["AC" + RowIndex].SetStyle(numberStyle);

                            ws.Cells["AD" + RowIndex].PutValue(totaldayexprire);
                            ws.Cells["AD" + RowIndex].SetStyle(numberStyle);

                            ws.Cells["AE" + RowIndex].PutValue(item.Email);

                        }

                    }

                    #region total row
                    RowIndex++;

                    numberStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    currencyStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    alignRightStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    centerStyle.Font.IsBold = true;
                    numberStyle.BackgroundColor = Color.FromArgb(240, 248, 255);

                    ws.Cells["A" + RowIndex].PutValue("TỔNG CỘNG");
                    ws.Cells["A" + RowIndex].SetStyle(centerStyle);

                    ws.Cells["B" + RowIndex].PutValue(Total_Quantity);
                    ws.Cells["B" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["C" + RowIndex].PutValue(StringHelpers.FormatWeight(Total_Weight));
                    ws.Cells["C" + RowIndex].SetStyle(alignRightStyle);

                    ws.Cells["D" + RowIndex].PutValue(Total_Price);
                    ws.Cells["D" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["E" + RowIndex].PutValue(Total_ShippingFee);
                    ws.Cells["E" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["F" + RowIndex].PutValue(Total_AmountUSD);
                    ws.Cells["F" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["H" + RowIndex].PutValue(Total_AmountVND);
                    ws.Cells["H" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["I" + RowIndex].PutValue(Total_AmountWithoutVoucherUSD);
                    ws.Cells["I" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["J" + RowIndex].PutValue(Total_Discount2ndVnd);
                    ws.Cells["J" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["K" + RowIndex].PutValue(Total_DiscountVoucherVnd);
                    ws.Cells["K" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["L" + RowIndex].PutValue(Total_DiscountVoucherUSD);
                    ws.Cells["L" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["M" + RowIndex].PutValue(Total_Amount_Discount);
                    ws.Cells["M" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["N" + RowIndex].PutValue(Total_PaymentAmount);
                    ws.Cells["N" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["O" + RowIndex].PutValue(Total_CashbackAmount);
                    ws.Cells["O" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["P" + RowIndex].PutValue(Total_OrderAmount);
                    ws.Cells["P" + RowIndex].SetStyle(numberStyle);

                    ws.Cells["Z" + RowIndex].PutValue(Total_FirstPoundFee);
                    ws.Cells["Z" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["AA" + RowIndex].PutValue(Total_NextPoundFee);
                    ws.Cells["AA" + RowIndex].SetStyle(currencyStyle);

                    ws.Cells["AB" + RowIndex].PutValue(Total_SpecialFee);
                    ws.Cells["AB" + RowIndex].SetStyle(currencyStyle);
                    #endregion

                    ws.AutoFitColumns();

                    #endregion

                    ws.Cells.InsertColumn(4);
                    ws.Cells.InsertColumn(5);
                    ws.Cells.InsertColumn(6);
                    ws.Cells.CopyColumn(ws.Cells, ws.Cells.Columns[28].Index, ws.Cells.Columns[4].Index);
                    ws.Cells.CopyColumn(ws.Cells, ws.Cells.Columns[29].Index, ws.Cells.Columns[5].Index);
                    ws.Cells.CopyColumn(ws.Cells, ws.Cells.Columns[30].Index, ws.Cells.Columns[6].Index);
                    ws.Cells.DeleteColumn(28);
                    ws.Cells.DeleteColumn(28);
                    ws.Cells.DeleteColumn(28);

                    wb.Save(FilePath);
                    pathResult = FilePath;
                }

            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("ReportOrder - OrderRepository: " + ex);
            }
            return pathResult;
        }

        public List<ChartRevenuViewModel> GetRevenuByDateRange(OrderSearchModel searchModel, bool isNow = true)
        {
            var model = new List<ChartRevenuViewModel>();
            try
            {
                DataTable dt = _OrderDAL.GetRevenuByDateRange(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (isNow)
                    {
                        model = (from row in dt.AsEnumerable()
                                 select new ChartRevenuViewModel
                                 {
                                     Date = Convert.ToDateTime(!row["Date"].
                                     Equals(DBNull.Value) ? row["Date"] : null),
                                     OrderCount = Convert.ToInt32(row["OrderCount"]),
                                     TotalRevenu = Math.Round(Convert.ToDouble(row["Amount"])),
                                     TotalShipFee = Math.Round(Convert.ToDouble(row["ShipFee"]))
                                 }).ToList();
                    }
                    else
                    {
                        model = (from row in dt.AsEnumerable()
                                 select new ChartRevenuViewModel
                                 {
                                     DatePass = Convert.ToDateTime(!row["Date"].
                                     Equals(DBNull.Value) ? row["Date"] : null),
                                     OrderCountPass = Convert.ToInt32(row["OrderCount"]),
                                     TotalRevenuPass = Math.Round(Convert.ToDouble(row["Amount"])),
                                     TotalShipFeePass = Math.Round(Convert.ToDouble(row["ShipFee"]))
                                 }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenuByDateRange in OrderRepository" + ex);
            }
            return model;
        }
        public List<ChartRevenuViewModel> GetLabelRevenuByDateRange(OrderSearchModel searchModel)
        {
            var model = new List<ChartRevenuViewModel>();
            try
            {
                DataTable dt = _OrderDAL.GetLabelRevenuByDateRange(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model = (from row in dt.AsEnumerable()
                             select new ChartRevenuViewModel
                             {
                                 TotalRevenu = Math.Round(Convert.ToDouble(row["Amount"])),
                                 StoreName = Convert.ToString(row["StoreName"]),
                             }).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLabelRevenuByDateRange in OrderRepository" + ex);
            }
            return model;
        }
        public List<ChartRevenuViewModel> GetLabelQuantityByDateRange(OrderSearchModel searchModel)
        {
            var model = new List<ChartRevenuViewModel>();
            try
            {
                DataTable dt = _OrderDAL.GetLabelQuantityByDateRange(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model = (from row in dt.AsEnumerable()
                             select new ChartRevenuViewModel
                             {
                                 OrderCount = Convert.ToInt32(row["OrderCount"]),
                                 StoreName = Convert.ToString(row["StoreName"]),
                             }).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLabelRevenuByDateRange in OrderRepository" + ex);
            }
            return model;
        }
        public double GetRevenuDay()
        {
            double? percent = 0;
            try
            {
                percent = _OrderDAL.GetRevenuDay();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList in OrderRepository" + ex);
            }
            return percent != null ? percent.Value : 0;
        }

        public async Task<object> GetOrderSuggestionList(string orderNo)
        {
            return await _OrderDAL.GetOrderSuggestionList(orderNo);
        }

        public async Task<long> FindOrderIdByOrderNo(string orderNo)
        {
            long rs = 0;
            try
            {
                var order = await _OrderDAL.FindByOrderNo(orderNo);
                if (order != null)
                {
                    rs = order.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindOrderIdByOrderNo in OrderRepository" + ex);
            }
            return rs;
        }

        public async Task<Order> FindOrderByOrderId(long orderId)
        {
            Order order = new Order();
            try
            {
                var rs = await _OrderDAL.FindAsync(orderId);
                if (rs != null)
                {
                    order = rs;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindOrderIdByOrderNo in OrderRepository" + ex);
            }
            return order;
        }

        public RevenueViewModel SummaryRevenuToday()
        {
            try
            {
                return _OrderDAL.SummaryRevenuToday();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SummaryRevenuToday in OrderRepository" + ex);
                return null;
            }
        }

        public RevenueViewModel SummaryRevenuTodayTemp()
        {
            try
            {
                return _OrderDAL.SummaryRevenuTodayTemp();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SummaryRevenuTodayTemp in OrderRepository" + ex);
                return null;
            }
        }

        public async Task<List<OrderGridModel>> GetOrderListByClientId(long ClientId)
        {
            try
            {
                return await _OrderDAL.GetOrderListByClientId(ClientId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<RevenueMinMax> GetMinMaxOrderAmount()
        {
            try
            {
                return await _OrderDAL.GetMinMaxAmountOrder();
            }
            catch
            {
                return null;
            }
        }

        public async Task<double> GetOrderTotalAmount(long Id)
        {
            var OrderModel = await _OrderDAL.FindAsync(Id);
            var PaymentAmount = await _PaymentDAL.GetOrderPaymentAmount(Id);
            var CashbackAmount = await _CashbackDAL.GetOrderCashbackAmount(Id);
            return (double)(PaymentAmount > 0 ? (PaymentAmount - CashbackAmount) : OrderModel.AmountVnd);
        }

        public async Task<OrderViewModel> GetOrderDetailByContractNo(string orderNo)
        {
            try
            {
                return await _OrderDAL.GetOrderDetail(orderNo);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetailByContractNo(orderNo) in OrderRepository" + ex);
                return null;
            }
        }

        public async Task<string> BuildOrderNo(int label_id)
        {
            var rd = new Random();
            Dictionary<int, string> months = new Dictionary<int, string> { { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "K" }, { 10, "L" }, { 11, "M" }, { 12, "N" } };
            try
            {
                var label_detail = await _LabelDAL.getLabelDetailById(label_id);
                DateTime current = DateTime.Now;
                DateTime startDate = new DateTime(current.Year, current.Month, current.Day, 0, 0, 0);

                // Lấy ra tổng số đơn trong ngày tính đến thời điểm hiện tại
                int total_order = await _OrderDAL.getTotalOrderByCurrentDate();

                string orderCode = ((current.Day * 10) + (total_order + 1)).ToString("D3");

                string order_no_final = label_detail.PrefixOrderCode + "-"
                        + current.Year.ToString().Substring(current.Year.ToString().Length - 1, 1)
                        + months[current.Month]
                        + current.Day.ToString("D2") // cụm ngày tháng
                        + orderCode; // cụm id


                // kiem tra don nay co trong db chua. Neu co roi tinh lai
                var order_detail = await _OrderDAL.FindByOrderNo(order_no_final);
                if (order_detail != null)
                {
                    total_order += rd.Next(1, 9);
                    orderCode = ((current.Day * 10) + (total_order + 1)).ToString("D3");
                    order_no_final = label_detail.PrefixOrderCode + "-"
                        + current.Year.ToString().Substring(current.Year.ToString().Length - 1, 1)
                        + months[current.Month]
                        + current.Day.ToString("D2") // cụm ngày tháng
                        + orderCode; // cụm id
                }

                return order_no_final;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BuildOrderNo(label_id=" + label_id + ") in OrderRepository" + ex);
                string unixTimestamp = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
                Dictionary<int, string> types = new Dictionary<int, string> { { 1, "AM" }, { 2, "CC" }, { 3, "BB" }, { 4, "NR" }, { 5, "HL" }, { 6, "SP" }, { 7, "JS" }, { 8, "VS" } };

                return "U" + types[label_id] + "-" // cụm 1
                          + DateTime.Now.Year.ToString().Substring(3, 1)
                          + months[DateTime.Now.Month]
                          + unixTimestamp.Substring(unixTimestamp.Length - 3, unixTimestamp.Length - 1);// cụm ngày tháng
            }
        }

        public async Task<long> Update(OrderViewModel model)
        {
            try
            {
                await _OrderDAL.UpdateAsync(model);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - OrderRepository. Ex= " + ex);
                return -1;
            }
        }

        public async Task<OrderApiViewModel> GetOrderDetailForApi(long Id)
        {
            try
            {
                return await _OrderDAL.GetOrderByIdForAPI(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetailForApi in OrderRepository" + ex);
                return null;
            }
        }

        public async Task<long> UpdateOrderMapId(long order_id, long order_map_id)
        {
            try
            {
                return await _OrderDAL.UpdateOrderMapId(order_id, order_map_id);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("UpdateOrderMapId - OrderRepository" + ex);
                return -1;
            }
        }
        public Task<OrderViewModel> GetOrderDetail(string orderNo)
        {
            throw new NotImplementedException();
        }

        public long GetTotalVoucherUse(long voucher_id, string email_client)
        {
            try
            {
                return _OrderDAL.GetTotalVoucherUse(voucher_id, email_client);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("[orderRepo-] GetTotalVoucherUse - OrderRepository" + ex);
                return 0;
            }
        }
        public object GetOrderListFEByClientId(int clientID, string keyword, int order_status, int current_page, int page_size)
        {
            try
            {
                return _OrderDAL.GetOrderListByClientId(clientID, keyword, order_status, current_page, page_size);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram(" GetOrderListByClientId - OrderRepository" + ex);
                return null;
            }
        }
        public object GetOrderDetailFEByID(int OrderId)
        {
            try
            {
                return _OrderDAL.GetFeOrderDetailById(OrderId);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram(" GetFeOrderDetailById - OrderRepository" + ex);
                return 0;
            }
        }
        public object GetFELastestRecordByClientID(int ClientId)
        {
            try
            {
                return _OrderDAL.GetLastestRecordByClientID(ClientId);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram(" GetLastestRecordByClientID - OrderRepository" + ex);
                return 0;
            }
        }
        public object GetFEOrderCountByClientID(int ClientId)
        {
            try
            {
                return _OrderDAL.GetOrderCountByClientID(ClientId);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram(" GetLastestRecordByClientID - OrderRepository" + ex);
                return 0;
            }
        }

        public long GetTotalErrorOrderCount()
        {
            return _OrderDAL.GetTotalErrorOrderCount();
        }

        public async Task<bool> UpdatePaymentReCheckOut(long order_id, int address_id, short pay_type)
        {
            try
            {
                var address_detail = await _ClientDAL.GetAddressReceiverByAddressId(address_id);
                string receiver_name = address_detail.Count() > 0 ? address_detail[0].ReceiverName : "N/A";
                string full_address = address_detail.Count() > 0 ? address_detail[0].FullAddress : "N/A";
                string phone = address_detail.Count() > 0 ? address_detail[0].Phone : "N/A";

                var order_id_rs = await _OrderDAL.UpdatePaymentReChecOut(order_id, receiver_name, full_address, phone, pay_type);
                return (order_id_rs > 0);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ORDERRESPONITORY] UpdatePaymentReCheckOut - OrderRepository" + ex);
                return false;
            }
        }

        public async Task<bool> updateAdressReceiver(string full_address, string phone, string receiver_name, long order_id)
        {
            try
            {
                var order = await _OrderDAL.FindAsync(order_id);
                order.Address = full_address;
                order.Phone = phone;
                order.ClientName = receiver_name;
                await _OrderDAL.UpdateAsync(order);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ORDERRESPONITORY] updateAdressReceiver - OrderRepository" + ex);
                return false;
            }
        }

        public async Task<int> GetTotalReturningClientInDay()
        {
            try
            {
                var ListOrderToday = await _OrderDAL.GetByConditionAsync(s => s.CreatedOn.Value.Date == DateTime.Now.Date);
                if (ListOrderToday != null && ListOrderToday.Count > 0)
                {
                    var ListClientId = ListOrderToday.Select(s => s.ClientId).Distinct();
                    var ListReturningClient = await _ClientDAL.GetByConditionAsync(s => ListClientId.Contains(s.Id) && s.JoinDate.Date < DateTime.Now.Date);

                    if (ListReturningClient != null && ListReturningClient.Count > 0)
                    {
                        return ListReturningClient.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ORDERRESPONITORY] GetTotalReturningClientInDay - OrderRepository" + ex);
            }
            return 0;
        }

        public async Task<long> GetTotalPaymentClientInDay()
        {
            try
            {
                var ListOrderToday = await _OrderDAL.GetByConditionAsync(s => s.PaymentStatus == 0 && s.PaymentDate.Value.Date == DateTime.Now.Date);
                if (ListOrderToday != null && ListOrderToday.Count > 0)
                {
                    var ListClientId = ListOrderToday.Select(s => s.ClientId).Distinct();

                    if (ListClientId != null && ListClientId.Count() > 0)
                    {
                        return ListClientId.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ORDERRESPONITORY] GetTotalPaymentClientInDay - OrderRepository" + ex);
            }
            return 0;
        }


        public async Task<Order> FindAsync(long Id)
        {
            return await _OrderDAL.FindAsync(Id);
        }
        public async Task<string> UpdateOrderStatus(string order_no, int order_status)
        {
            return await _OrderDAL.UpdateOrderStatus(order_no, order_status);
        }

        public async Task<long> UpdateAsync(Order entity)
        {
            try
            {
                await _OrderDAL.UpdateAsync(entity);
                return entity.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ORDERRESPONITORY] UpdateAsync - OrderRepository" + ex);
                return 0;
            }

        }

        public async Task<List<OrderGridModel>> GetOrderListByReferralId(string ReferralId)
        {
            try
            {
                return await _OrderDAL.GetOrderListByReferralId(ReferralId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[ORDERRESPONITORY] GetOrderListByReferralId - OrderRepository" + ex);
                return null;
            }
        }


        public async Task<double> getTotalOrderByEmail(string email)
        {
            try
            {
                return _OrderDAL.getTotalOrderByEmail(email).Result.AmountVnd ?? 0;
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram(" GetFeOrderDetailById - OrderRepository" + ex);
                return 0;
            }
        }


        public async Task<OrderAppModel> GetOrderDetailByOrderNo(string order_no)
        {
            try
            {
                return await _OrderDAL.GetOrderDetailByOrderNo(order_no);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<object> GetOrderListByClientPhone(string client_phone)
        {
            try
            {
                return await _OrderDAL.GetOrderListByClientPhone(client_phone);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<object> GetOrderTrackingByOrderNo(string order_no)
        {
            try
            {
                return await _OrderDAL.GetOrderTrackingByOrderNo(order_no);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<AffOrder>> GetAffiliateOrderItems(DateTime time_start, DateTime time_end, List<string> utm_source)
        {
            try
            {
                return await _OrderDAL.GetAffiliateOrders(time_start, time_end, utm_source);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<OrderLogShippingDateViewModel> GetOrderShippingLogToday()
        {
            return _OrderDAL.GetOrderShippingLogToday();
        }

        public Task<OrderViewModel> CheckOrderDetail(long Id)
        {
            return _OrderDAL.CheckOrderDetail(Id);
        }
    }
}
