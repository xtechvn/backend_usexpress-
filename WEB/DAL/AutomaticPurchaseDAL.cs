using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.AutomaticPurchase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class AutomaticPurchaseDAL : GenericService<AutomaticPurchaseAmz>
    {
        private static DbWorker _DbWorker;

        public AutomaticPurchaseDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<AutomaticPurchaseAmz> GetById(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AutomaticPurchaseAmz.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - AutomaticPurchaseDAL: " + ex);
                return null;
            }
        }

        public async Task<List<AutomaticPurchaseAmz>> GetListByPurchaseStatus(int purchase_status)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AutomaticPurchaseAmz.AsNoTracking().Where(x => x.PurchaseStatus == purchase_status).ToListAsync();
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetNewPurchaseItems - AutomaticPurchaseDAL: " + ex);
                return null;
            }
        }
        public async Task<List<AutomaticPurchaseAmz>> GetTrackingList()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AutomaticPurchaseAmz.AsNoTracking().Where(x => x.PurchaseStatus ==(int)AutomaticPurchaseStatus.PurchaseSuccess && x.DeliveryStatus!=(int)OrderDeliveryStatus.Delivered &&  x.DeliveryStatus!=(int)OrderDeliveryStatus.RefundPakage && x.DeliveryStatus != (int)OrderDeliveryStatus.CannotTracking).ToListAsync();
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetNewPurchaseItems - AutomaticPurchaseDAL: " + ex);
                return null;
            }
        }
        public async Task<int> UpdatePurchaseDetail(AutomaticPurchaseAmz new_detail)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AutomaticPurchaseAmz.Update(new_detail);
                    await _DbContext.SaveChangesAsync();
                }
                return (int)ResponseType.SUCCESS;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePurchaseDetail - AutomaticPurchaseDAL: " + ex);
                return (int)ResponseType.ERROR;
            }
        }
        public async Task<long> AddNewPurchaseDetail(AutomaticPurchaseAmz new_detail)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (new_detail.OrderCode != null && new_detail.OrderCode.Trim() != "")
                    {
                        var detail = await _DbContext.AutomaticPurchaseAmz.AsNoTracking().FirstOrDefaultAsync(x => x.OrderCode == new_detail.OrderCode && x.ProductCode == new_detail.ProductCode && x.Amount == new_detail.Amount);
                        if (detail != null)
                        {
                            return detail.Id;
                        }
                    }
                    new_detail.ProductCode = new_detail.ProductCode.ToUpper();
                    new_detail.CreateDate = DateTime.Now;
                    new_detail.UpdateLast = DateTime.Now;
                    var add_new = _DbContext.AutomaticPurchaseAmz.Add(new_detail);
                    await _DbContext.SaveChangesAsync();
                }
                return new_detail.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewPurchaseDetail - AutomaticPurchaseDAL: " + ex);
                return -1;
            }
        }
        public async Task<long> GetIDByDetail(AutomaticPurchaseAmz new_detail)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (new_detail.OrderCode != null && new_detail.OrderCode.Trim() != "")
                    {
                        var detail = await _DbContext.AutomaticPurchaseAmz.AsNoTracking().FirstOrDefaultAsync(x => x.OrderCode == new_detail.OrderCode && x.ProductCode == new_detail.ProductCode && x.Amount == new_detail.Amount && x.PurchaseStatus != (int)AutomaticPurchaseStatus.New);
                        if (detail != null)
                        {
                            return detail.Id;
                        }
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewPurchaseDetail - GetIDByDetail: " + ex);
                return -2;
            }
        }
        public async Task<long> AddOrUpdatePurchaseDetail(AutomaticPurchaseAmz new_detail)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    bool is_new = true;
                    if (new_detail.OrderCode != null && new_detail.OrderCode.Trim() != "")
                    {
                        var detail = await _DbContext.AutomaticPurchaseAmz.AsNoTracking().FirstOrDefaultAsync(x => x.OrderCode == new_detail.OrderCode && x.ProductCode==new_detail.ProductCode && x.Amount == new_detail.Amount);
                        if (detail != null)
                        {
                            new_detail.Id = detail.Id;
                            var update = _DbContext.AutomaticPurchaseAmz.Update(new_detail);
                            is_new = false;
                        }
                    }
                    if (is_new)
                    {
                        new_detail.ProductCode = new_detail.ProductCode.ToUpper();
                        new_detail.CreateDate = DateTime.Now;
                        new_detail.UpdateLast = DateTime.Now;
                        var add_new = _DbContext.AutomaticPurchaseAmz.Add(new_detail);
                    }
                    await _DbContext.SaveChangesAsync();
                }
                return new_detail.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewPurchaseDetail - AutomaticPurchaseDAL: " + ex);
                return -1;
            }
        }
        public GenericViewModel<AutomaticPurchaseAmzViewModel> GetPagingList(AutomaticPurchaseSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<AutomaticPurchaseAmzViewModel>();

            try
            {
                DateTime _FromDate = DateTime.MinValue;
                DateTime _ToDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(searchModel.FromDate))
                {
                    _FromDate = DateTime.ParseExact(searchModel.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(searchModel.ToDate))
                {
                    _ToDate = DateTime.ParseExact(searchModel.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                SqlParameter[] objParam = new SqlParameter[8];
                objParam[0] = new SqlParameter("@Id", searchModel.Id.ToString() ?? string.Empty);

                if (_FromDate != DateTime.MinValue)
                    objParam[1] = new SqlParameter("@FromDate", _FromDate);
                else
                    objParam[1] = new SqlParameter("@FromDate", DBNull.Value);

                if (_ToDate != DateTime.MinValue)
                    objParam[2] = new SqlParameter("@ToDate", _ToDate);
                else
                    objParam[2] = new SqlParameter("@ToDate", DBNull.Value);

                objParam[3] = new SqlParameter("@OrderCode", searchModel.OrderNo ?? string.Empty);
                objParam[4] = new SqlParameter("@ProductCode", searchModel.ProductCode ?? string.Empty);
                objParam[5] = new SqlParameter("@PurchaseStatus", searchModel.PurchaseStatus.ToString() ?? string.Empty);
                objParam[6] = new SqlParameter("@CurentPage", currentPage.ToString() ?? string.Empty);
                objParam[7] = new SqlParameter("@PageSize", pageSize.ToString() ?? string.Empty);


                var dt = _DbWorker.GetDataTable(ProcedureConstants.AUTOMATIC_PURCHASE_SEARCH, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model.ListData = (from row in dt.AsEnumerable()
                                      select new AutomaticPurchaseAmzViewModel
                                      {
                                          Id = Convert.ToInt64(row["Id"].ToString()),
                                          Amount = Convert.ToDouble(row["Amount"]),
                                          PurchaseStatus = Convert.ToInt32(!row["PurchaseStatus"].Equals(DBNull.Value) ? row["PurchaseStatus"] : -1),
                                          AutomaticPurchaseStatusName = row["AutomaticPurchaseStatusName"].ToString(),
                                          CreateDate = Convert.ToDateTime(!row["CreateDate"].Equals(DBNull.Value) ? row["CreateDate"] : null),
                                          DeliveryMessage = row["DeliveryMessage"].ToString(),
                                          DeliveryStatus = Convert.ToInt32(!row["DeliveryStatus"].Equals(DBNull.Value) ? row["DeliveryStatus"] : -1),
                                          ManualNote = row["ManualNote"].ToString(),
                                          OrderCode = row["OrderCode"].ToString(),
                                          OrderDetailUrl = row["OrderDetailUrl"].ToString(),
                                          OrderedSuccessUrl = row["OrderedSuccessUrl"].ToString(),
                                          OrderEstimatedDeliveryDate = Convert.ToDateTime(!row["OrderEstimatedDeliveryDate"].Equals(DBNull.Value) ? row["OrderEstimatedDeliveryDate"] : null),
                                          OrderId = Convert.ToInt64(!row["OrderId"].Equals(DBNull.Value) ? row["OrderId"] : 0),
                                          OrderMappingId = row["OrderMappingId"].ToString(),
                                          ProductCode = row["ProductCode"].ToString(),
                                          PurchasedOrderId = row["PurchasedOrderId"].ToString(),
                                          PurchasedSellerName = row["PurchasedSellerName"].ToString(),
                                          PurchasedSellerStoreUrl = row["PurchasedSellerStoreUrl"].ToString(),
                                          PurchaseMessage = row["PurchaseMessage"].ToString(),
                                          PurchaseUrl = row["PurchaseUrl"].ToString(),
                                          Quanity = Convert.ToInt32(!row["Quanity"].Equals(DBNull.Value) ? row["Quanity"] : -1),
                                          Screenshot = row["Screenshot"].ToString(),
                                          UpdateLast = Convert.ToDateTime(!row["UpdateLast"].Equals(DBNull.Value) ? row["UpdateLast"] : null)
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
    }
}
