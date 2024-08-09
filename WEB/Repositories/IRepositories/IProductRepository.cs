using Entities.Models;
using Entities.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IProductRepository
    {
        /// <summary>
        /// Tìm kiếm trong Elasticsearch
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
       // List<ProductViewModel> SearchProduct(string query, string index_name, int top);
        List<Product> GetAll();
        Task<Product> GetById(int Id);
        Task<long> Create(ProductViewModel model);
        Task<long> Update(ProductViewModel model);
        Task<int> Delete(int id);
        Task<Product> GetByProductMapId(int Id);
        Task<Product> GetByProductCode(string productCode, int labelId);
        Task<Product> GetByProductCode(string productCode);
        Task<Dictionary<string, double>> getShippingFee(int label_id, ProductBuyerViewModel product_buyer);
        Task<GenericViewModel<ProductViewModel>> GetProductPagingList(ProductFilterModel filter);
        object GetListBoughtProductQuantity(string arrProductCode);

        Task<bool> UpdateProductQuantityAndGroupOnElastic(IEnumerable<ESModifyProductModel> arrProduct);
        DataTable GetInterestedProduct(int PageIndex, int PageSize);
        DataTable GetInterestedProductTotalRecord();
    }
}
