using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly OrderItemDAL _OrderItemDAL;
        public OrderItemRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OrderItemDAL = new OrderItemDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<OrderItem> GetAll()
        {
            return _OrderItemDAL.GetAll();
        }

        public Task<OrderItem> GetById(int Id)
        {
            return _OrderItemDAL.GetById(Id);
        }

        public async Task<long> Create(OrderItemViewModel model)
        {
            OrderItem orderItem = new OrderItem();
            long rs = 0;
            orderItem.CreateOn = DateTime.Now;
            orderItem.FirstPoundFee = model.FirstPoundFee;
            orderItem.LuxuryFee = model.LuxuryFee;
            orderItem.NextPoundFee = model.NextPoundFee;
            orderItem.OrderId = model.OrderId;
            orderItem.Price = model.Price;
            orderItem.ProductId = model.ProductId;
            orderItem.Quantity = model.Quantity;
            orderItem.ShippingFeeUs = model.ShippingFeeUs;
            rs = await _OrderItemDAL.CreateAsync(orderItem);
            return rs;
        }

        public async Task<long> Update(OrderItemViewModel model)
        {
            OrderItem orderItem = new OrderItem();
            long rs = 0;
            orderItem.CreateOn = DateTime.Now;
            orderItem.FirstPoundFee = model.FirstPoundFee;
            orderItem.LuxuryFee = model.LuxuryFee;
            orderItem.NextPoundFee = model.NextPoundFee;
            orderItem.OrderId = model.OrderId;
            orderItem.Price = model.Price;
            orderItem.ProductId = model.ProductId;
            orderItem.Quantity = model.Quantity;
            orderItem.ShippingFeeUs = model.ShippingFeeUs;
            rs = await _OrderItemDAL.UpdateItem(orderItem);
            return rs;
        }

        public async Task<double> GetAllItemWeightByOrderID(long id)
        {
            return await _OrderItemDAL.GetAllItemWeightByOrderID(id) * 0.45359237;
        }
    }
}
