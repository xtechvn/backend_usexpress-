using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IOrderItemRepository
    {
        List<OrderItem> GetAll();
        Task<OrderItem> GetById(int Id);
        Task<long> Create(OrderItemViewModel model);
        Task<long> Update(OrderItemViewModel model);
        Task<int> Delete(int id);
        Task<double> GetAllItemWeightByOrderID(long id);

    }
}
