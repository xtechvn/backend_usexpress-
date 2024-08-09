using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ICashbackRepository
    {
        Task<List<CashbackViewModel>> GetListByOrderId(long orderId);
        Task<long> Create(Cashback model);
        Task<long> Update(Cashback model);
        Task<int> Delete(long Id);
    }
}
