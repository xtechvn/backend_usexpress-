using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.AutomaticPurchase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAutomaticPurchaseAmzRepository
    {
        public Task<AutomaticPurchaseAmz> GetById(long id);
        public Task<List<AutomaticPurchaseAmz>> GetNewPurchaseItems();
        public Task<List<AutomaticPurchaseAmz>> GetRetryPurchaseItems();
        public Task<int> UpdatePurchaseDetail(AutomaticPurchaseAmz new_detail);
        public Task<int> AddNewPurchaseDetail(long order_id);
        public Task<long> AddNewPurchaseDetail(AutomaticPurchaseAmz new_detail);
        public Task<long> GetIDByDetail(AutomaticPurchaseAmz new_detail);
        public Task<List<AutomaticPurchaseAmz>> GetTrackingList();
        public Task<GenericViewModel<AutomaticPurchaseAmzViewModel>> GetPagingList(AutomaticPurchaseSearchModel searchModel, int currentPage = 1, int pageSize = 20);
        public Task<long> AddOrUpdatePurchaseDetail(AutomaticPurchaseAmz new_detail);
        Task<List<AutomaticPurchaseAmz>> GetEstimatedDeliveryDateByOrderNo(string orderNo);
    }
}
