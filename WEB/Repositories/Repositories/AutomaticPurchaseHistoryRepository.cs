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
    public class AutomaticPurchaseHistoryRepository : IAutomaticPurchaseHistoryRepository
    {
        private readonly AutomaticPurchaseHistoryDAL _automaticPurchaseHistoryDAL;

        public AutomaticPurchaseHistoryRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _automaticPurchaseHistoryDAL = new AutomaticPurchaseHistoryDAL(dataBaseConfig.Value.SqlServer.ConnectionString);

        }
        public Task<int> AddNewHistory(AutomaticPurchaseHistory new_item)
        {
            return _automaticPurchaseHistoryDAL.AddNewHistory(new_item);
        }

        public Task<List<AutomaticPurchaseHistory>> GetByAutomaticPurchaseHistoryByOrderCode(string order_code)
        {
            return _automaticPurchaseHistoryDAL.GetByAutomaticPurchaseHistoryByOrderCode(order_code);
        }

        public Task<List<AutomaticPurchaseHistory>> GetByAutomaticPurchaseId(long purchase_id)
        {
            return _automaticPurchaseHistoryDAL.GetByAutomaticPurchaseId(purchase_id);
        }

        public Task<AutomaticPurchaseHistory> GetById(long id)
        {
            return _automaticPurchaseHistoryDAL.GetById(id);
        }
    }
}
