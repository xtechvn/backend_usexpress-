using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class AutomaticPurchaseHistoryDAL : GenericService<AutomaticPurchaseHistory>
    {
        private static DbWorker _DbWorker;

        public AutomaticPurchaseHistoryDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<AutomaticPurchaseHistory> GetById(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AutomaticPurchaseHistory.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - AutomaticPurchaseHistoryDAL: " + ex);
                return null;
            }
        }
        public async Task<List<AutomaticPurchaseHistory>> GetByAutomaticPurchaseId(long purchase_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AutomaticPurchaseHistory.AsNoTracking().Where(x => x.AutomaticPurchaseId == purchase_id).ToListAsync();
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByAutomaticPurchaseId - AutomaticPurchaseHistoryDAL: " + ex);
                return null;
            }
        }
        public async Task<List<AutomaticPurchaseHistory>> GetByAutomaticPurchaseHistoryByOrderCode(string order_code)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var buy_list= await _DbContext.AutomaticPurchaseAmz.AsNoTracking().Where(x => x.OrderCode == order_code).ToListAsync();
                    if(buy_list!=null && buy_list.Count > 0)
                    {
                        var history=new List<AutomaticPurchaseHistory>();
                        foreach (var buy in buy_list)
                        {
                            var item = await _DbContext.AutomaticPurchaseHistory.AsNoTracking().Where(x => x.AutomaticPurchaseId == buy.Id).ToListAsync();
                            if(item==null || item.Count < 1)
                            {
                                continue;
                            }
                            history.AddRange(item);
                        }
                        return history;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByAutomaticPurchaseHistoryByOrderCode - AutomaticPurchaseHistoryDAL: " + ex);
                return null;
            }
        }
        public async Task<int> AddNewHistory(AutomaticPurchaseHistory new_item)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (new_item.Id > 0)
                    {
                        var detail = await _DbContext.AutomaticPurchaseHistory.AsNoTracking().FirstOrDefaultAsync(x => x.Id == new_item.Id);
                        if (detail != null)
                        {
                            return (int)ResponseType.EXISTS;
                        }
                    }
                     _DbContext.AutomaticPurchaseHistory.Add(new_item);
                    await _DbContext.SaveChangesAsync();

                }
                return (int)ResponseType.SUCCESS;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - AutomaticPurchaseHistoryDAL: " + ex);
                return (int)ResponseType.ERROR;
            }
        }
    }
}
