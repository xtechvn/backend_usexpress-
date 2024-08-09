using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAutomaticPurchaseHistoryRepository
    {
        public Task<AutomaticPurchaseHistory> GetById(long id);
        public Task<List<AutomaticPurchaseHistory>> GetByAutomaticPurchaseId(long purchase_id);
        public Task<int> AddNewHistory(AutomaticPurchaseHistory new_item);
        public Task<List<AutomaticPurchaseHistory>> GetByAutomaticPurchaseHistoryByOrderCode(string order_code);

    }
}
