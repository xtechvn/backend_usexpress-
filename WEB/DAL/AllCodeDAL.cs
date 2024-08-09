using DAL.Generic;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class AllCodeDAL : GenericService<AllCode>
    {
        public AllCodeDAL(string connection) : base(connection) { }

        public List<AllCode> GetListByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Set<AllCode>().Where(n => n.Type == type).ToList();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListByType - AllCodeDAL. " + ex);
                return null;
            }
        }

        public AllCode GetByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Set<AllCode>().Where(n => n.Type == type).FirstOrDefault();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByType - AllCodeDAL. " + ex);
                return null;
            }
        }
        public async Task<AllCode> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AllCode.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ProductClassificationDAL: " + ex.Message);
                return null;
            }
        }
        public async Task<short> GetLastestCodeValueByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCode.AsNoTracking().Where(x => x.Type == type).OrderByDescending(x => x.CodeValue).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        return  detail.CodeValue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLastestCodeValueByType - ProductClassificationDAL: " + ex.Message);
            }
            return -1;
        }
        public async Task<short> GetLastestOrderNoByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCode.AsNoTracking().Where(x => x.Type == type).OrderByDescending(x => x.OrderNo).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        return detail.OrderNo!=null?(short)detail.OrderNo: (short)-1;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLastestOrderNoByType - ProductClassificationDAL: " + ex.Message);
            }
            return -1;
        }
        public async Task<AllCode> GetIDIfValueExists(string type, string description)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCode.AsNoTracking().Where(x => x.Type == type && x.Description==description).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLastestOrderNoByType - ProductClassificationDAL: " + ex.Message);
            }
            return null;
        }
        public async Task<List<AllCode>> GetListSortByName(string type_name)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCode.AsNoTracking().Where(n => n.Type == type_name).OrderBy(x=>x.Description).ToListAsync();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListByType - AllCodeDAL. " + ex);
                return null;
            }
        }
    }
}
