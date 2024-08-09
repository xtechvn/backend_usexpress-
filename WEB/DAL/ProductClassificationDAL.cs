using DAL.Generic;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL.Generic
{
    public class ProductClassificationDAL : GenericService<ProductClassification>
    {
        public ProductClassificationDAL(string connection) : base(connection)
        {
        }

        public async Task<ProductClassification> GetByLink(string link)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ProductClassification.AsNoTracking().FirstOrDefaultAsync(x => x.Link.ToLower() == link.Trim().ToLower());
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByLink - ProductClassificationDAL: " + ex.Message);
                return null;
            }
        }
        public async Task<List<ProductClassification>> GetByProductGroupId(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var listData = _DbContext.ProductClassification.AsNoTracking()
                        .Where(x => x.GroupIdChoice == id).ToListAsync();
                    return await listData;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByLink - ProductClassificationDAL: " + ex.Message);
                return null;
            }
        }
        public async Task<List<ProductClassification>> GetByCapgianId(int campaignId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var listData = _DbContext.ProductClassification.AsNoTracking()
                        .Where(x => x.CapaignId == campaignId).ToListAsync();
                    return await listData;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByLink - ProductClassificationDAL: " + ex.Message);
                return null;
            }
        }

        public async Task<ProductClassification> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ProductClassification.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ProductClassificationDAL: " + ex.Message);
                return null;
            }
        }

        public async Task<int> CreateItem(ProductClassification model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.ProductClassification.Add(model);
                    await _DbContext.SaveChangesAsync();
                    return model.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateItem - ProductClassificationDAL: " + ex.Message);
                return -1;
            }
        }

        public async Task<int> UpdateItem(ProductClassification model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.ProductClassification.Update(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateItem - ProductClassificationDAL: " + ex.Message);
                return -1;
            }
        }
        public List<CampaignAdsViewModel> GetPagingList(string fromTime, string toTime, int currentPage,
           List<int> lstLabelId, string strLink, string listCampaignId, int pageSize, out int totalRecord, int status = -1)
        {
            totalRecord = 0;
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var datalist = _DbContext.ProductClassification.AsQueryable();
                    var ListCampaign = _DbContext.CampaignAds.AsQueryable();

                    DateTime? fromDate = DateTime.MinValue, toDate = DateTime.MinValue;

                    if (!string.IsNullOrEmpty(fromTime)) fromDate = DateUtil.StringToDate(fromTime);
                    if (!string.IsNullOrEmpty(toTime)) toDate = DateUtil.StringToDate(toTime);

                    if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                    {
                        datalist = datalist.Where(s => (s.FromDate >= fromDate && s.FromDate <= toDate) || (s.ToDate >= fromDate && s.ToDate <= toDate));
                    }
                    else
                    {
                        if (fromDate != DateTime.MinValue) datalist = datalist.Where(s => s.FromDate >= fromDate);
                        if (toDate != DateTime.MinValue) datalist = datalist.Where(s => s.ToDate < toDate.Value.AddDays(1));
                    }

                    if (!string.IsNullOrEmpty(strLink))
                    {
                        datalist = datalist.Where(s => s.Link.Contains(strLink.Trim()) || s.LinkProductTarget.Contains(strLink.Trim()));
                    }

                    if (!string.IsNullOrEmpty(listCampaignId))
                    {
                        var ListCampaignId = GetListIdByCampaign(listCampaignId);
                        datalist = datalist.Where(s => ListCampaignId.Contains(s.CapaignId.Value));
                    }

                    if (status > -1)
                    {
                        datalist = datalist.Where(s => s.Status == status);
                    }
                    if (lstLabelId != null && lstLabelId.Count > 0)
                    {
                        datalist = datalist.Where(s => lstLabelId.Contains(s.LabelId.Value));
                    }
                    datalist = datalist.OrderByDescending(x => x.UpdateTime);
                    totalRecord = datalist.Count();
                    var data = datalist.Select(a => new CampaignAdsViewModel
                    {
                        Id = a.Id,
                        Link = a.Link,
                        Status = a.Status,
                        CapaignId = a.CapaignId,
                        CreateTime = a.CreateTime,
                        FromDate = a.FromDate,
                        GroupIdChoice = a.GroupIdChoice,
                        LinkProductTarget = a.LinkProductTarget,
                        LabelId = a.LabelId,
                        Note = a.Note,
                        ProductId = a.ProductId,
                        ProductName = a.ProductName,
                        ToDate = a.ToDate,
                    }).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ProductClassificationDAL: " + ex.Message);
            }
            return null;
        }
        public List<int> GetListIdByCampaign(string listCampaignId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var arrCampaignId = listCampaignId.Split(',').Select(s => int.Parse(s)).ToArray();
                    return _DbContext.CampaignAds.Where(s => arrCampaignId.Contains(s.Id)).Select(s => s.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListIdByCampaign - ProductClassificationDAL: " + ex);
                return new List<int>();
            }
        }

    }
}
