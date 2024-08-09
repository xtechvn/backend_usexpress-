using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class ProductDAL : GenericService<Product>
    {
        private static DbWorker _DbWorker;
        public ProductDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<Product> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Product.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ProductDAL: " + ex);
                return null;
            }
        }
        public async Task<Product> GetById(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Product.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ProductDAL: " + ex);
                return null;
            }
        }
        public async Task<Product> GetByProductMapId(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Product.AsNoTracking().FirstOrDefaultAsync(x => x.ProductMapId == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByProductMapId - ProductDAL: " + ex);
                return null;
            }
        }
        public async Task<Product> GetByProductCode(string productCode, int storeId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Product.AsNoTracking().FirstOrDefaultAsync(x => x.ProductCode
                    == productCode && x.LabelId == storeId);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByProductCode - ProductDAL: " + ex);
                return null;
            }
        }
        public async Task<Product> GetByProductCode(string productCode)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Product.AsNoTracking().FirstOrDefaultAsync(x => x.ProductCode
                    == productCode);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByProductCode - ProductDAL: " + ex);
                return null;
            }
        }
        public async Task<long> CreateItem(Product model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Product.Add(model);
                    await _DbContext.SaveChangesAsync();
                    return model.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateItem - ProductDAL: " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateItem(Product model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.Product.Update(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateItem - ProductDAL: " + ex);
                return -1;
            }
        }

        public DataTable GetDataTableBoughtProductQuantity(string ArrProductCode)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ArrProductCode", ArrProductCode ?? string.Empty);
                return _DbWorker.GetDataTable(ProcedureConstants.PRODUCT_GetBoughtQuantity, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetBoughtProductQuantity - ProductDAL: " + ex);
            }
            return null;
        }

        public DataTable GetDataTableInterestedProduct(int PageIndex, int PageSize)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@CurentPage", PageIndex);
                objParam[1] = new SqlParameter("@PageSize", PageSize);
                return _DbWorker.GetDataTable(ProcedureConstants.PRODUCT_GetInterestedProduct, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDataTableInterestedProduct - ProductDAL: " + ex);
            }
            return null;
        }
        public DataTable GetDataTableInterestedProductTotalRecord()
        {
            try
            {
                return _DbWorker.GetDataTable(ProcedureConstants.PRODUCT_GetInterestedProduct_TotalRecord);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDataTableInterestedProductTotalRecord - ProductDAL: " + ex);
            }
            return null;
        }
        public async Task<List<PriceLevelViewModel>> getPriceLevelByLabelId(int LabelId)
        {
            try
            {
                DateTime current_date = DateTime.Now;

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var result = await (from n in _DbContext.PriceProductLevel.AsNoTracking()
                                        where (("," + n.LabelId.Trim() + ",").IndexOf("," + LabelId.ToString().Trim() + ",") >= 0) && (current_date >= n.FromDate && current_date <= n.ToDate)
                                        select new PriceLevelViewModel
                                        {
                                            Offset = n.Offset,
                                            Limit = n.Limit,
                                            LabelId = n.LabelId,
                                            Price = n.Price,
                                            FeeType = n.FeeType ?? -1,
                                            Discount = n.Discount ?? 0,
                                            Note = n.Note
                                        }).ToListAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getPriceLevelByLabelId - ProductDAL: " + ex);
                return null;
            }
        }

        //public async Task<List<PriceLevelViewModel>> getSpecical(int LabelId)
        //{
        //    try
        //    {
        //        DateTime current_date = DateTime.Now;

        //        using (var _DbContext = new EntityDataContext(_connection))
        //        {
        //            var result = await (from n in _DbContext.PriceProductLevel.AsNoTracking()
        //                                where (("," + LabelId.ToString().Trim() + ",").IndexOf("," + n.LabelId.Trim() + ",") >= 0) && (current_date >= n.FromDate && current_date <= n.ToDate)
        //                                select new PriceLevelViewModel
        //                                {
        //                                    Offset = n.Offset,
        //                                    Limit = n.Limit,
        //                                    LabelId = n.LabelId,
        //                                    Price = n.Price,
        //                                    FeeType = n.FeeType ?? -1,
        //                                    Discount = n.Discount ?? 0,
        //                                    Note = n.Note
        //                                }).ToListAsync();
        //            return result;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("getPriceLevelByLabelId - PriceProductLevelDAL: " + ex);
        //        return null;
        //    }
        //}

    }
}
