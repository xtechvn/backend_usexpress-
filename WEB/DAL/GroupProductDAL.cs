using DAL.Generic;
using Entities.Models;
using Entities.ViewModels.GroupProducts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class GroupProductDAL : GenericService<GroupProduct>
    {
        public GroupProductDAL(string connection) : base(connection)
        {
        }

        /// <summary>
        /// Delete Group Product
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>
        ///  0 : errors
        /// -1 : is used
        /// -2 : has child
        /// >0 : success
        /// </returns>
        public async Task<int> DeleteGroupProduct(int Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var IsUseForProductClassification = _DbContext.ProductClassification.Any(s => s.GroupIdChoice == Id);
                    var IsHasChild = _DbContext.GroupProduct.Any(s => s.ParentId == Id);

                    if (IsUseForProductClassification)
                    {
                        return -1;
                    }

                    if (IsHasChild)
                    {
                        return -2;
                    }

                    var entity = await FindAsync(Id);
                    _DbContext.GroupProduct.Remove(entity);
                    await _DbContext.SaveChangesAsync();
                    return Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteGroupProduct - GroupProductDAL: " + ex);
                return 0;
            }
        }

        public async Task<List<int?>> GetListGroupProductCrawled()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.GroupProduct.Where(s => s.LinkCount > 0).Select(s => s.IsAutoCrawler).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListCrawlLink - GroupProductDAL: " + ex);
                return null;
            }
        }

        public async Task<List<GroupProduct>> getCategoryDetailByCategoryId(int[] category_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var group_product = _DbContext.GroupProduct.AsNoTracking().Where(s => category_id.Contains(s.Id)).ToListAsync();

                    return await group_product;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteGroupProduct - GroupProductDAL: " + ex);
                return null;
            }
        }
        public async Task<List<GroupProductStore>> GetGroupProductStoresByGroupProductId(int groupProductId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.GroupProductStore.Where(s => s.GroupProductId == groupProductId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetGroupProductStores - GroupProductDAL: " + ex);
                return null;
            }
        }
        public async Task<long> UpsertGroupProductStore(int groupProductId, List<GroupProductStore> models)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var ListExistData = await _DbContext.GroupProductStore.Where(s => s.GroupProductId == groupProductId).ToListAsync();

                            if (ListExistData != null && ListExistData.Count > 0)
                            {
                                foreach (var item in ListExistData)
                                {
                                    _DbContext.GroupProductStore.Remove(item);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            if (models != null && models.Count > 0)
                            {
                                foreach (var item in models)
                                {
                                    await _DbContext.GroupProductStore.AddAsync(item);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }

                            transaction.Commit();
                            return groupProductId;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogHelper.InsertLogTelegram("GetGroupProductStores - GroupProductDAL: " + ex.Message.ToString());
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetGroupProductStores - GroupProductDAL: " + ex.Message.ToString());
                return 0;
            }
        }
        public async Task<List<GroupProduct>> getCategoryDetailByCampaignId(int campaign_id, int skip, int take)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var model = await (from n in _DbContext.CampaignGroupProduct.AsNoTracking()
                                       join a in _DbContext.GroupProduct.AsNoTracking() on n.GroupProductId equals a.Id into af
                                       from j in af.DefaultIfEmpty()
                                       orderby n.OrderBox
                                       where n.CampaignId == campaign_id
                                       select new GroupProduct
                                       {
                                           Id = j.Id,
                                           Name = j.Name,
                                           Path = j.Path,
                                           ImagePath = j.ImagePath,
                                           Description = j.Description

                                       }).Skip(skip).Take(take).ToListAsync();
                    return model;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getCategoryDetailByCampaignId - GroupProductDAL: " + ex);
                return null;
            }
        }
        public async Task<int> UpdateAutoCrawler(int id, int type)
        {
            try
            {
                var model = await FindAsync(id);
                model.IsAutoCrawler = type;
                await UpdateAsync(model);
                if (type == 0)
                {
                    await DeleteGroupAffByCateId(id);
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteGroupProduct - UpdateAutoCrawler: " + ex);
                return 0;
            }
        }

        public async Task<int> UpdateAffiliateCategory(int cateId, int affId, int type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (type == 1)
                    {
                        var model = new AffiliateGroupProduct
                        {
                            GroupProductId = cateId,
                            AffType = affId
                        };
                        await _DbContext.AffiliateGroupProduct.AddAsync(model);
                        await _DbContext.SaveChangesAsync();
                    }
                    else
                    {
                        var model = await _DbContext.AffiliateGroupProduct.FirstOrDefaultAsync(s => s.GroupProductId == cateId && s.AffType == affId);
                        if (model != null)
                        {
                            _DbContext.AffiliateGroupProduct.Remove(model);
                            await _DbContext.SaveChangesAsync();
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAffiliateCategory: " + ex);
                return 0;
            }
        }

        public async Task<List<AffiliateGroupProduct>> GetGroupAffById(int CateId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.AffiliateGroupProduct.AsNoTracking().Where(s => s.GroupProductId == CateId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetGroupAffById - GroupProductDAL: " + ex);
                return null;
            }
        }

        public async Task DeleteGroupAffByCateId(int CateId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var ListObj = await _DbContext.AffiliateGroupProduct.AsNoTracking().Where(s => s.GroupProductId == CateId).ToListAsync();
                    if (ListObj != null && ListObj.Count > 0)
                    {
                        _DbContext.AffiliateGroupProduct.RemoveRange(ListObj);
                        await _DbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteGroupAffByCateId - GroupProductDAL: " + ex);
            }
        }

        public async Task<List<GroupProduct>> GetActiveCrawl()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.GroupProduct.AsNoTracking().Where(s => s.IsAutoCrawler == 1).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetGroupProductStores - GroupProductDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// cuonglv
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<List<GroupProduct>> getAllGroupProduct()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var group_product = _DbContext.GroupProduct.AsNoTracking().Where(s => s.Status == (int)StatusType.BINH_THUONG).ToListAsync();

                    return await group_product;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getAllGroupProduct - GroupProductDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// cuonglv
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<GroupProduct> getDetailByPath(string path)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var group_product = await _DbContext.GroupProduct.AsNoTracking().FirstOrDefaultAsync(s => s.Status == (int)StatusType.BINH_THUONG && s.Path == path.Trim());
                    return group_product;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getDetailByPath - GroupProductDAL: " + ex);
                return null;
            }
        }

        public async Task<List<GroupProductFeaturedViewModel>> GetGroupProductFeatureds(string img_domain, string position)
        {
            List<GroupProductFeaturedViewModel> result = null;
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    List<GroupProduct> group_product = null;
                    switch (position)
                    {
                        case "header":
                            {
                                group_product = await _DbContext.GroupProduct.AsNoTracking().Where(x => x.IsShowHeader == true).ToListAsync();
                            }
                            break;
                        case "footer":
                            {
                                group_product = await _DbContext.GroupProduct.AsNoTracking().Where(x => x.IsShowFooter == true).ToListAsync();
                            }
                            break;
                        default:
                            {
                                group_product = await _DbContext.GroupProduct.AsNoTracking().Where(x => x.IsShowHeader == true).ToListAsync();
                            }
                            break;
                    }
                    result = new List<GroupProductFeaturedViewModel>();
                    foreach (var group in group_product)
                    {
                        result.Add(new GroupProductFeaturedViewModel()
                        {
                            name = group.Name,
                            path = group.Path + "-cat",
                            image = img_domain + group.ImagePath
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetGroupProductFeatureds - GroupProductDAL: " + ex);
            }
            return result;
        }


        public async Task<List<LocationProduct>> getProductList(int group_product_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = await _DbContext.LocationProduct.AsNoTracking().Where(x => x.GroupProductId == group_product_id).ToListAsync();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getProductList: " + ex);
                return null;
            }
        }
    }
}
