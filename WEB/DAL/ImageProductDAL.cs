using DAL.Generic;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class ImageProductDAL : GenericService<ImageProduct>
    {
        public ImageProductDAL(string connection) : base(connection)
        {
        }

        public async Task<ImageProduct> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ImageProduct.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ImageProductDAL: " + ex);
                return null;
            }
        }
        public async Task<long> CreateItem(ImageProduct model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.ImageProduct.Add(model);
                    await _DbContext.SaveChangesAsync();
                    return model.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateItem - ImageProductDAL: " + ex);
                return -1;
            }
        }

        public async Task<long> UpdateItem(ImageProduct model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    _DbContext.ImageProduct.Update(model);
                    await _DbContext.SaveChangesAsync();
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateItem - ImageProductDAL: " + ex);
                return -1;
            }
        }
    }
}
