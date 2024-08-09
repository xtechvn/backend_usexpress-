using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities.Contants;

namespace Repositories.Repositories
{
    
    public class OrderProgessRepository : IOrderProgressRepository
    {
        private readonly OrderProgressDAL _orderProgressDAL;

        public OrderProgessRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _orderProgressDAL = new OrderProgressDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<List<OrderProgress>> GetOrderProgressesByOrderNoAsync(string order_no)
        {
            try
            {
                return await _orderProgressDAL.getOrderProgressByOrderNo(order_no);
            } catch(Exception)
            {
                return null;
            }
        }
        public async Task<List<OrderProgress>> GetAll()
        {
            try
            {
                return await _orderProgressDAL.GetAllAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> SetOrderProgreess(OrderProgress data)
        {
            try
            {
                var result= await  _orderProgressDAL.CreateAsync(data);
                if (result > 0) return (int)ResponseType.SUCCESS;
                else return (int)ResponseType.FAILED;
                    
            }
            catch (Exception)
            {
                return (int)ResponseType.ERROR;
            }
        }
    }
}
