using Entities.Models;
using Entities.ViewModels.GroupProducts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IGroupProductRepository
    {
        Task<List<GroupProduct>> GetAll();
        Task<GroupProduct> GetById(int Id);
        Task<int> UpSert(GroupProduct model);
        Task<int> Delete(int id);
        Task<string> GetListTreeView(string name, int status);
        Task<string> GetListTreeViewCheckBox(int ParentId, int status, List<int> CheckedList = null, bool IsHasIconEdit = false);
        Task<List<GroupProduct>> getCategoryByParentId(int parent_id);
        Task<List<GroupProduct>> getCategoryDetailByCategoryId(int[] category_id);
        Task<List<GroupProduct>> getCategoryDetailByCampaignId(int campaign_id, int skip, int take);
        Task<List<GroupProductStore>> GetGroupProductStoresByGroupProductId(int GroupProductId);
        Task<long> UpsertGroupProductStores(int groupProductId, List<GroupProductStore> models);
        Task<int> UpdateAutoCrawler(int id, int type);
        Task<int> UpdateAffiliateCategory(int cateId, int affId, int type);
        Task<List<GroupProduct>> GetActiveCrawlGroupProducts();
        Task<List<GroupProduct>> getAllGroupProduct();
        Task<GroupProduct> getDetailByPath(string path);
        Task<List<int?>> GetListGroupProductCrawled();
        Task<string> GetHtmlHorizontalMenu(int ParentId);
        Task<int> GetRootParentId(int cateId);
        Task<string> GetGroupProductNameAsync(int cateID);
        Task<List<GroupProductFeaturedViewModel>> GetGroupProductFeatureds(string img_domain,string position);
        Task<List<LocationProduct>> getProductCodeByGroupId(int group_product_id);
    }
}
