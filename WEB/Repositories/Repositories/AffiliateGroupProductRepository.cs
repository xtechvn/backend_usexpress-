using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class AffiliateGroupProductRepository : IAffiliateGroupProductRepository
    {
        private readonly AffiliateGroupProductDAL _AffiliateGroupProductDAL;
        public AffiliateGroupProductRepository(IOptions<DataBaseConfig> dataBaseConfig)        
        {            
            _AffiliateGroupProductDAL = new AffiliateGroupProductDAL(dataBaseConfig.Value.SqlServer.ConnectionString);            
        }
        public async Task<List<AffiliateGroupProduct>> GetAllAffiliateGroupProduct()
        {
            try
            {
                var detail = await _AffiliateGroupProductDAL.GetAllAffiliateGroupProduct();
                return detail;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllAffiliateGroupProduct - AffiliateGroupProductRepository: " + ex);
                return null;
            }
        }
    }
}
