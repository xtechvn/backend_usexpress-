using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IPaymentRepository
    {
        Task<List<PaymentViewModel>> GetListByOrderId(long orderId);
        Task<long> Create(Payment model);
        Task<long> Update(Payment model);
        Task<int> Delete(long Id);
    }
}
