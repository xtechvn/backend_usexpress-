using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IOrderProgressRepository
    {
        Task<List<OrderProgress>> GetOrderProgressesByOrderNoAsync(string order_no);
        Task<int> SetOrderProgreess(OrderProgress data);
        Task<List<OrderProgress>> GetAll();
    }
}
