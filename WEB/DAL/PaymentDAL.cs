using DAL.Generic;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Contants;

namespace DAL
{
    public class PaymentDAL : GenericService<Payment>
    {
        public PaymentDAL(string connection) : base(connection)
        {
        }

        public async Task<List<PaymentViewModel>> GetListByOrderId(long OrderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = await (from _payment in _DbContext.Payment.AsNoTracking()
                                      join a in _DbContext.AllCode.Where(s => s.Type == AllCodeType.PAYMENT_TYPE) on _payment.PaymentType equals a.CodeValue into af
                                      from _paymentType in af.DefaultIfEmpty()
                                      where _payment.OrderId == OrderId
                                      select new PaymentViewModel
                                      {
                                          Id = _payment.Id,
                                          Amount = _payment.Amount,
                                          PaymentDate = _payment.PaymentDate,
                                          PaymentType = _payment.PaymentType,
                                          OrderId = _payment.OrderId,
                                          PaymentTypeName = _paymentType.Description,
                                          UserId = _payment.UserId,
                                          Note = _payment.Note,
                                          CreatedOn = _payment.CreatedOn,
                                          ModifiedOn = _payment.ModifiedOn
                                          //ProductId = _payment.ProductId,
                                          //ProductCode = _product.ProductCode,
                                      }).ToListAsync();
                    return data;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<double> GetOrderPaymentAmount(long orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Payment.Where(s => s.OrderId == orderId).SumAsync(s => s.Amount);
                }
            }
            catch
            {
                return 0;
            }
        }

        public async Task<Payment> GetFirstPaymentOrder(long orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Payment.Where(s => s.OrderId == orderId && s.UserId == 36).FirstOrDefaultAsync();
                }
            }
            catch
            {
                return null;
            }
        }

    }
}
