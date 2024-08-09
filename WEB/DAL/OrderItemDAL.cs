using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class OrderItemDAL : GenericService<OrderItem>
    {
        public OrderItemDAL(string connection) : base(connection)
        {
        }

        public async Task<OrderItem> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.OrderItem.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - OrderItemDAL: " + ex);
                return null;
            }
        }
        public async Task<List<OrderItem>> GetByOrderId(long order_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.OrderItem.AsNoTracking().Where(x => x.OrderId == order_id).ToListAsync();
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderId - OrderItemDAL: " + ex);
                return null;
            }
        }
        public async Task<long> MultipleInsertAsync(List<OrderItem> orderItems)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    using (var transaction = _DbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var item in orderItems)
                            {
                                var orderItemModel = await _DbContext.OrderItem.FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.OrderId == item.OrderId);
                                if (orderItemModel != null)
                                {
                                    orderItemModel.Price = item.Price;
                                    orderItemModel.DiscountShippingFirstPound = item.DiscountShippingFirstPound;
                                    orderItemModel.FirstPoundFee = item.FirstPoundFee;
                                    orderItemModel.NextPoundFee = item.NextPoundFee;
                                    orderItemModel.LuxuryFee = item.LuxuryFee;
                                    orderItemModel.ShippingFeeUs = item.ShippingFeeUs;
                                    orderItemModel.Quantity = item.Quantity;
                                    orderItemModel.ImageThumb = item.ImageThumb;
                                    orderItemModel.UpdateLast = item.UpdateLast;
                                    orderItemModel.OrderItempMapId = item.OrderItempMapId;

                                    _DbContext.OrderItem.Update(orderItemModel);
                                    await _DbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    await _DbContext.OrderItem.AddAsync(item);
                                    await _DbContext.SaveChangesAsync();
                                }
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogHelper.InsertLogTelegram("MultipleInsertAsync - OrderItemDAL: " + ex);
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<long> UpdateItem(OrderItem model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.OrderItem.Update(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateItem - OrderItemDAL: " + ex);
                return -1;
            }
        }
        public async Task<double> GetAllItemWeightByOrderID(long id)
        {
            double weight = 0;
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.OrderItem.AsNoTracking().Where(x => x.OrderId == id).ToListAsync();
                    foreach(var i in detail)
                    {
                        weight += ((double)i.Weight* (int)i.Quantity)<=0?0: (double)i.Weight * (int)i.Quantity;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllItemWeightByOrderID - OrderItemDAL: " + ex);
            }
            return weight;

        }
    }
}
