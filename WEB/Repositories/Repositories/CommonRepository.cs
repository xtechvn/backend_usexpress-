using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class CommonRepository : ICommonRepository
    {
        private readonly CommonDAL _CommonDAL;

        public CommonRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _CommonDAL = new CommonDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<Province>> GetProvinceList()
        {
            return await _CommonDAL.GetProvinceList();
        }

        public async Task<List<District>> GetDistrictListByProvinceId(string provinceId)
        {
            return await _CommonDAL.GetDistrictListByProvinceId(provinceId);
        }

        public async Task<List<Ward>> GetWardListByDistrictId(string districtId)
        {
            return await _CommonDAL.GetWardListByDistrictId(districtId);
        }

        public async Task<List<AllCode>> GetAllCodeByType(string type)
        {
            return await _CommonDAL.GetAllCodeListByType(type);
        }
    }
}
