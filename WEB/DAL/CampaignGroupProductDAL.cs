using DAL.Generic;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CampaignGroupProductDAL : GenericService<CampaignGroupProduct>
    {
        public CampaignGroupProductDAL(string connection) : base(connection)
        {
        }

        public async Task<List<int>> GetListGroupProductIdByCampaignId(int CampaignId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.CampaignGroupProduct.Where(m => m.CampaignId == CampaignId).Select(s => s.GroupProductId).ToListAsync();
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> MultipleInsertCampaignGroupProduct(int CampaignId, List<int> ArrGroupProduct)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var ListDeleted = await _DbContext.CampaignGroupProduct.Where(s => s.CampaignId == CampaignId && !ArrGroupProduct.Contains(s.GroupProductId)).ToListAsync();
                            if (ListDeleted != null && ListDeleted.Count > 0)
                            {
                                foreach (var item in ListDeleted)
                                {
                                    var deleteModel = await _DbContext.CampaignGroupProduct.FindAsync(item.Id);
                                    _DbContext.CampaignGroupProduct.Remove(deleteModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            if (ArrGroupProduct != null && ArrGroupProduct.Count > 0)
                            {
                                var max_order_box = _DbContext.CampaignGroupProduct.Where(s => s.CampaignId == CampaignId).Max(s => s.OrderBox);
                                var counter = max_order_box != null ? (max_order_box + 1) : 0;
                                foreach (var item in ArrGroupProduct)
                                {
                                    var itemModel = await _DbContext.CampaignGroupProduct.Where(s => s.GroupProductId == item && s.CampaignId == CampaignId).FirstOrDefaultAsync();
                                    if (itemModel == null)
                                    {
                                        var model = new CampaignGroupProduct
                                        {
                                            CampaignId = CampaignId,
                                            GroupProductId = item,
                                            OrderBox = counter,
                                            Status = 0
                                        };
                                        await _DbContext.CampaignGroupProduct.AddAsync(model);
                                        await _DbContext.SaveChangesAsync();

                                        counter++;
                                    }
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<CampaignGroupProduct> GetDetailByCampaignAndGroupProduct(int CampaignId, int GroupProductId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.CampaignGroupProduct.Where(m => m.CampaignId == CampaignId && m.GroupProductId == GroupProductId).FirstOrDefaultAsync();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
