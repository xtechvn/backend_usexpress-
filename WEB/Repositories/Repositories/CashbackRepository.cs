using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class CashbackRepository : ICashbackRepository
    {
        private readonly CashbackDAL _CashbackDAL;

        public CashbackRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _CashbackDAL = new CashbackDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<long> Create(Cashback model)
        {
            try
            {
                return await _CashbackDAL.CreateAsync(model);
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
                await _CashbackDAL.DeleteAsync(Id);
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<CashbackViewModel>> GetListByOrderId(long orderId)
        {
            try
            {
                return await _CashbackDAL.GetListByOrderId(orderId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<long> Update(Cashback model)
        {
            try
            {
                await _CashbackDAL.UpdateAsync(model);
                return model.Id;
            }
            catch
            {
                return 0;
            }
        }
    }
}
