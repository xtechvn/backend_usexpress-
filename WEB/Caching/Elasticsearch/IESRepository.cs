using Entities.ViewModels;
using Entities.ViewModels.Product;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Caching.Elasticsearch
{
    public interface IESRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// string StrEsConfig = $"{elasticConfig.Host}:{elasticConfig.Port}";
        /// IESRepository<EsProductViewModel> _ESRepository = new ESRepository<EsProductViewModel>(StrEsConfig);
        /// var id = "0134190440_1";
        /// var Model = _ESRepository.FindById("product", id);
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        TEntity FindById(string indexName, object value, string field_name);

        int UpSert(TEntity entity, string indexName);
        Task<int> UpSertAsync(TEntity entity, string indexName);

        int UpSertMultiple(IEnumerable<TEntity> entity, string indexName);
        Task<int> UpSertMultipleAsync(IEnumerable<TEntity> entity, string indexName);
        ProductViewModel getProductDetailByCode(string index_name, string product_code, int label_id);
        bool DeleteProductByCode(string indexName, string document_id);
        List<ProductViewModel> searchProduct(string input_search, string index_name, int top);
        long GetIdentityProductEs(string index_name);
        Task<SearchEsEntitiesViewModel> getProductListByGroupProductId(string index_name, List<int> grp_id, int from, int size,string filter);
        Task<string> getListProductCodeNotExits(string index_name, List<string> lst_product_code_target,int group_id);
        int getTotalProductCrawlToday(string index_name,int label_type);
        Task<bool> DeleteProductByKey(string index_name, string product_code, int label_id);
    }
}
