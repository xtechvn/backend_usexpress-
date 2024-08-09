using DAL.Generic;
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
    public class CampaignAdsDAL : GenericService<CampaignAds>
    {
        public CampaignAdsDAL(string connection) : base(connection)
        {
        }

        public async Task<int> Upsert(int Id, string Name)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var IsExist = _DbContext.CampaignAds.Any(s => s.Id != Id && s.CampaignName == Name);
                    if (IsExist)
                    {
                        return -2;
                    }
                }

                if (Id > 0)
                {
                    var model = await FindAsync(Id);
                    model.CampaignName = Name;
                    await UpdateAsync(model);
                    return model.Id;
                }
                else
                {
                    var model = new CampaignAds()
                    {
                        Id = Id,
                        CampaignName = Name,
                        Type = 1
                    };
                    return (int)await CreateAsync(model);
                }
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> getCampaignIdByCategoryId(int GroupProductId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var model = await _DbContext.CampaignGroupProduct.AsNoTracking().FirstOrDefaultAsync(x => x.GroupProductId == GroupProductId);

                    return model == null ? -1 : model.CampaignId;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CampaignAdsDAL - getCategoryDetailByCampaignId:  " + ex);
                return -1;
            }
        }
    }
}
