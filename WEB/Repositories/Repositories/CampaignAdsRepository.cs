using DAL;
using Entities.ConfigModels;
using Entities.Models;
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
    public class CampaignAdsRepository : ICampaignAdsRepository
    {
        // private readonly ILogger<CampaignAdsRepository> _logger;
        private readonly CampaignAdsDAL _CampaignAdsDAL;
        private readonly CampaignGroupProductDAL _CampaignGroupProductDAL;

        public CampaignAdsRepository(IOptions<DataBaseConfig> dataBaseConfig)
        //public CampaignAdsRepository(IOptions<DataBaseConfig> dataBaseConfig,ILogger<CampaignAdsRepository> logger)
        {
            //  _logger = logger;
            _CampaignAdsDAL = new CampaignAdsDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _CampaignGroupProductDAL = new CampaignGroupProductDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<int> Create(CampaignAds model)
        {
            try
            {
                var entity = new CampaignAds()
                {
                    CampaignName = model.CampaignName,
                    EndDate = model.EndDate,
                    Note = model.Note,
                    StartDate = model.StartDate,
                    Type = model.Type
                };
                await _CampaignAdsDAL.CreateAsync(entity);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Create - CampaignAdsRepository" + ex);
                return -1;
            }
        }

        public Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<CampaignAds> GetById(int Id)
        {
            return _CampaignAdsDAL.FindAsync(Id);
        }

        public async Task<int> Update(CampaignAds model)
        {
            try
            {
                var entity = await _CampaignAdsDAL.FindAsync(model.Id);
                entity.CampaignName = model.CampaignName;
                entity.EndDate = model.EndDate;
                entity.Note = model.Note;
                entity.StartDate = model.StartDate;
                entity.Type = model.Type;
                entity.ScriptSocial = model.ScriptSocial;
                await _CampaignAdsDAL.UpdateAsync(entity);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - CampaignAdsRepository" + ex);
                return -1;
            }
        }

        public async Task<int> UpdateData(CampaignAds model)
        {
            try
            {
                await _CampaignAdsDAL.UpdateAsync(model);
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - CampaignAdsRepository" + ex);
                return -1;
            }
        }

        public List<CampaignAds> GetListAll()
        {
            return _CampaignAdsDAL.GetAll();
        }
        public async Task<List<CampaignAds>> GetSuggestionList(string name)
        {
            List<CampaignAds> data = new List<CampaignAds>();
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var ListCampaignAds = await _CampaignAdsDAL.GetAllAsync();

                    data = ListCampaignAds.Where(s => StringHelpers.ConvertStringToNoSymbol(s.CampaignName.Trim().ToLower())
                    .Contains(StringHelpers.ConvertStringToNoSymbol(name.Trim().ToLower()))).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSuggestionList - CampaignAdsRepository" + ex);
                // _logger.LogError(ex.Message);
            }
            return data;
        }

        public async Task<List<CampaignAds>> GetListAllAsync()
        {
            return await _CampaignAdsDAL.GetAllAsync();
        }

        public async Task<int> Upsert(int Id, string Name)
        {
            try
            {
                return await _CampaignAdsDAL.Upsert(Id, Name);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<int>> GetListGroupProductIdByCampaignId(int CampaignId)
        {
            return await _CampaignGroupProductDAL.GetListGroupProductIdByCampaignId(CampaignId);
        }

        public async Task<int> MultipleInsertCampaignGroupProduct(int Id, List<int> ArrGroupProduct)
        {
            return await _CampaignGroupProductDAL.MultipleInsertCampaignGroupProduct(Id, ArrGroupProduct);
        }

        public async Task<CampaignGroupProduct> DetailCampaignGroupProduct(int CampaignId, int GroupProductId)
        {
            return await _CampaignGroupProductDAL.GetDetailByCampaignAndGroupProduct(CampaignId, GroupProductId);
        }

        public async Task<int> SaveInfoCampaignGroupProduct(CampaignGroupProduct model)
        {
            try
            {
                var modelData = await _CampaignGroupProductDAL.FindAsync(model.Id);

                var list_campaign_group = await _CampaignGroupProductDAL.GetByConditionAsync(s => s.CampaignId == modelData.CampaignId && s.OrderBox == model.OrderBox);
                var campaign_group_order_model = list_campaign_group.FirstOrDefault();
                if (campaign_group_order_model != null)
                {
                    campaign_group_order_model.OrderBox = modelData.OrderBox;
                    await _CampaignGroupProductDAL.UpdateAsync(campaign_group_order_model);
                }

                modelData.Status = model.Status;
                modelData.OrderBox = model.OrderBox;
                await _CampaignGroupProductDAL.UpdateAsync(modelData);

                return modelData.Id;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Cuonglv: fr
        /// </summary>
        /// <param name="category_id"></param>
        /// <returns></returns>
        public async Task<int> getCampaignIdByCategoryId(int category_id)
        {
            try
            {
                var campaign_id = await _CampaignAdsDAL.getCampaignIdByCategoryId(category_id);
                return campaign_id;
            }
            catch (Exception)
            {
                return -1;
            }
        }


    }
}
