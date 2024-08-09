using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class CommonDAL
    {
        private readonly string _connection;
        public CommonDAL(string connection)
        {
            _connection = connection;
        }

        public async Task<List<Province>> GetProvinceList()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Province.AsNoTracking().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetProvinceList - CommonDAL: " + ex);
                return null;
            }
        }

        public async Task<List<District>> GetDistrictListByProvinceId(string provinceId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.District.Where(s => s.ProvinceId == provinceId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDistrictListByProvinceId - CommonDAL: " + ex);
                return null;
            }
        }

        public async Task<List<Ward>> GetWardListByDistrictId(string districtId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Ward.Where(s => s.DistrictId == districtId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetWardListByDistrictId - CommonDAL: " + ex);
                return null;
            }
        }

        public async Task<List<AllCode>> GetAllCodeListByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.AllCode.Where(s => s.Type == type).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllCodeListByType - CommonDAL: " + ex);
                return null;
            }
        }

    }
}
