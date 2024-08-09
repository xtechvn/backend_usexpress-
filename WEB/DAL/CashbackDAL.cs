using DAL.Generic;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CashbackDAL : GenericService<Cashback>
    {
        public CashbackDAL(string connection) : base(connection)
        { 
        }

        public async Task<List<CashbackViewModel>> GetListByOrderId(long OrderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = await (from _cashback in _DbContext.Cashback.AsNoTracking()
                                      join a in _DbContext.User.AsNoTracking() on _cashback.UserId equals a.Id into af
                                      from _user in af.DefaultIfEmpty()
                                      where _cashback.OrderId == OrderId
                                      select new CashbackViewModel
                                      {
                                          Id = _cashback.Id,
                                          Amount = _cashback.Amount,
                                          CashbackDate = _cashback.CashbackDate,
                                          OrderId = _cashback.OrderId,
                                          UserId = _cashback.UserId,
                                          UserName = _user.UserName,
                                          Note = _cashback.Note,
                                          CreatedOn = _cashback.CreatedOn,
                                          ModifiedOn = _cashback.ModifiedOn
                                      }).ToListAsync();
                    return data;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<double> GetOrderCashbackAmount(long orderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Cashback.Where(s => s.OrderId == orderId).SumAsync(s => s.Amount);
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
