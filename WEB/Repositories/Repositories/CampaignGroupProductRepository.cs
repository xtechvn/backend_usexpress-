using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class CampaignGroupProductRepository : ICampaignGroupProduct
    {
        private readonly CampaignGroupProductDAL _campaignGroupProductDAL;

        public CampaignGroupProductRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _campaignGroupProductDAL = new CampaignGroupProductDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task< List<CampaignGroupProduct>> GetAll()
        {
            return await _campaignGroupProductDAL.GetAllAsync();
        }
    }
}
