using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    /// <summary>
    /// Create By: CuongLv
    /// </summary>
    public class SpecialIndustryDAL : GenericService<IndustrySpecialLuxury>
    {
        private static DbWorker _DbWorker;
        public SpecialIndustryDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<List<IndustrySpecialViewModel>> getSpecicalLuxuryBySpecialType(int SpecialType)
        {
            try
            {
                DateTime current_date = DateTime.Now;

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var result = await (from n in _DbContext.IndustrySpecialLuxury.AsNoTracking()
                                        where n.SpecialType == SpecialType
                                        select new IndustrySpecialViewModel
                                        {
                                            Id = n.Id,
                                            SpecialType = n.SpecialType,
                                            Price = n.Price,
                                            Status = n.Status,
                                            IsAllowDiscountCompare = n.IsAllowDiscountCompare ?? false,
                                            GroupLabelId = n.GroupLabelId
                                        }).ToListAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getSpecicalLuxuryBySpecialType - SpecialIndustryDAL: " + ex);
                return null;
            }
        }

    }
}
