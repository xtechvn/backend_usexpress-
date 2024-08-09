using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IProductClassificationRepository
    {
        Task<List<ProductClassification>> GetAll();
        Task<ProductClassification> GetById(int Id);
        Task<ProductClassification> GetByLink(string link);
        Task<List<ProductClassification>> GetByProductGroupId(int id);
        Task<List<ProductClassification>> GetByCapgianId(int id);
        Task<int> Create(ProductClassification model);
        Task<int> CreateItem(ProductClassification model);
        Task<int> Update(ProductClassification model);
        Task<int> UpdateItem(ProductClassification model);
        Task Delete(int id);
        GenericViewModel<CampaignAdsViewModel> GetPagingList(string fromTime, string toTime, int currentPage,
            List<int> listLabelId, string strLink, string listCampaignId, int pageSize, int status = -1);
    }
}
