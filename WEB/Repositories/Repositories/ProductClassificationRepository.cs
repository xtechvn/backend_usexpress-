using DAL;
using DAL.Generic;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;


namespace Repositories.Repositories
{
    public class ProductClassificationRepository : IProductClassificationRepository
    {
        private readonly ProductClassificationDAL _ProductClassificationDAL;
        private readonly CampaignAdsDAL _CampaignAdsDAL;
        public ProductClassificationRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _ProductClassificationDAL = new ProductClassificationDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _CampaignAdsDAL = new CampaignAdsDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<int> Create(ProductClassification model)
        {
            try
            {
                var entity = new ProductClassification();
                entity.CreateTime = DateTime.Now;
                entity.FromDate = model.FromDate;
                entity.GroupIdChoice = model.GroupIdChoice;
                entity.LabelId = model.LabelId;
                entity.Link = model.Link;
                entity.Status = model.Status;
                entity.ToDate = model.ToDate;
                await _ProductClassificationDAL.CreateAsync(entity);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Create - ProductClassificationRepository: " + ex.Message);
                return -1;
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                await _ProductClassificationDAL.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Delete - ProductClassificationRepository: " + ex.Message);
            }
        }
        public async Task<List<ProductClassification>> GetAll()
        {
            return await _ProductClassificationDAL.GetAllAsync();
        }

        public Task<ProductClassification> GetById(int Id)
        {
            return _ProductClassificationDAL.GetById(Id);
        }

        public async Task<int> Update(ProductClassification model)
        {
            try
            {
                var entity = await _ProductClassificationDAL.GetById(model.Id);
                entity.FromDate = model.FromDate;
                entity.GroupIdChoice = model.GroupIdChoice;
                entity.LabelId = model.LabelId;
                entity.Link = model.Link;
                entity.Status = model.Status;
                entity.UpdateTime = DateTime.Now;
                entity.ToDate = model.ToDate;
                await _ProductClassificationDAL.UpdateAsync(entity);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - ProductClassificationRepository: " + ex.Message);
                return -1;
            }
        }

        public async Task<int> CreateItem(ProductClassification model)
        {
            try
            {
                var entity = new ProductClassification();
                entity.CreateTime = DateTime.Now;
                entity.FromDate = model.FromDate;
                entity.GroupIdChoice = model.GroupIdChoice;
                entity.LabelId = model.LabelId;
                entity.CapaignId = model.CapaignId;
                entity.Link = model.Link;
                entity.Status = model.Status;
                entity.ToDate = model.ToDate;
                entity.UserId = model.UserId;
                await _ProductClassificationDAL.CreateItem(entity);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateItem - ProductClassificationRepository: " + ex.Message);
                return -1;
            }
        }

        public async Task<int> UpdateItem(ProductClassification model)
        {
            try
            {
                var entity = await _ProductClassificationDAL.GetById(model.Id);
                entity.UpdateTime = DateTime.Now;
                entity.FromDate = model.FromDate;
                entity.GroupIdChoice = model.GroupIdChoice;
                entity.LabelId = model.LabelId;
                entity.CapaignId = model.CapaignId;
                entity.Link = model.Link;
                entity.Status = model.Status;
                entity.UserId = model.UserId;
                entity.ToDate = model.ToDate;
                await _ProductClassificationDAL.UpdateItem(entity);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateItem - ProductClassificationRepository: " + ex.Message);
                return -1;
            }
        }

        public Task<ProductClassification> GetByLink(string link)
        {
            return _ProductClassificationDAL.GetByLink(link);
        }

        public GenericViewModel<CampaignAdsViewModel> GetPagingList(string fromTime, string toTime, int currentPage, List<int> listLabelId,
           string strLink, string listCampaignId, int pageSize, int status = -1)
        {
            var model = new GenericViewModel<CampaignAdsViewModel>();
            int totalRecord = 0;
            try
            {
                model.ListData = _ProductClassificationDAL.GetPagingList(fromTime, toTime, currentPage, listLabelId, strLink,
                    listCampaignId, pageSize, out totalRecord, status);
                model.PageSize = pageSize;
                model.CurrentPage = currentPage;
                model.TotalRecord = totalRecord;
                model.TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ProductClassificationRepository: " + ex.Message);
            }
            return model;
        }

        public Task<List<ProductClassification>> GetByProductGroupId(int id)
        {
            return _ProductClassificationDAL.GetByProductGroupId(id);
        }
        public Task<List<ProductClassification>> GetByCapgianId(int campaignId)
        {
            return _ProductClassificationDAL.GetByCapgianId(campaignId);
        }
    }
}
