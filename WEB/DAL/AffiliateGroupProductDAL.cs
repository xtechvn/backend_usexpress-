using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Utilities;

namespace DAL
{
    public class AffiliateGroupProductDAL : GenericService<AffiliateGroupProduct>
    {
        public AffiliateGroupProductDAL(string connection) : base(connection)
        {
        }

        public async Task<List<AffiliateGroupProduct>> GetAllAffiliateGroupProduct()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var models = await _DbContext.AffiliateGroupProduct.AsNoTracking().ToListAsync();  
                                   
                    return models;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AffiliateGroupProduct - AffiliateGroupProductDAL: " + ex);
                return null;
            }
        }

    }
}
