using DAL.Generic;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DAL
{
    public class OrderProgressDAL : GenericService<OrderProgress>
    {
        public OrderProgressDAL(string connection) : base(connection)
        {
        }

        public async Task<List<OrderProgress>> getOrderProgressByOrderNo(string order_no)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await (from _orderprogress in _DbContext.OrderProgress.AsNoTracking()
                                        where _orderprogress.OrderNo == order_no
                                        select new OrderProgress
                                        {
                                            //Id = _orderprogress.Id,
                                            OrderNo = _orderprogress.OrderNo,
                                            OrderStatus = _orderprogress.OrderStatus,
                                            CreateDate = _orderprogress.CreateDate
                                        }).ToListAsync();
                    
                    return detail;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
