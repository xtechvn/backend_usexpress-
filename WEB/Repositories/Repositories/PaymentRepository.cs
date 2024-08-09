using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDAL _PaymentDAL;
        public PaymentRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _PaymentDAL = new PaymentDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<long> Create(Payment model)
        {
            try
            {
                return await _PaymentDAL.CreateAsync(model);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> Delete(long Id)
        {
            try
            {
                await _PaymentDAL.DeleteAsync(Id);
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<PaymentViewModel>> GetListByOrderId(long orderId)
        {
            try
            {
                return await _PaymentDAL.GetListByOrderId(orderId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<long> Update(Payment model)
        {
            try
            {
                await _PaymentDAL.UpdateAsync(model);
                return model.Id;
            }
            catch
            {
                return 0;
            }
        }
    }
}
