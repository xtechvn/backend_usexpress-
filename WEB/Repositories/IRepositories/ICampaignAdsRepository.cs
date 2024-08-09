using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ICampaignAdsRepository
    {
        List<CampaignAds> GetListAll();
        Task<List<CampaignAds>> GetListAllAsync();
        Task<CampaignAds> GetById(int Id);
        Task<int> Create(CampaignAds model);
        Task<int> Update(CampaignAds model);
        Task<int> UpdateData(CampaignAds model);
        Task<int> Delete(int id);
        Task<List<CampaignAds>> GetSuggestionList(string name);
        Task<int> Upsert(int Id, string Name);
        Task<List<int>> GetListGroupProductIdByCampaignId(int CampaignId);
        Task<int> MultipleInsertCampaignGroupProduct(int Id, List<int> ArrGroupProduct);
        Task<CampaignGroupProduct> DetailCampaignGroupProduct(int CampaignId, int GroupProductId);
        Task<int> SaveInfoCampaignGroupProduct(CampaignGroupProduct model);
        Task<int> getCampaignIdByCategoryId(int category_id);
    }
}
