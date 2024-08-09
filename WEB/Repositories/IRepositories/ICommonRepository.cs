using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ICommonRepository
    {
        Task<List<Province>> GetProvinceList();
        Task<List<District>> GetDistrictListByProvinceId(string provinceId);
        Task<List<Ward>> GetWardListByDistrictId(string districtId);
        Task<List<AllCode>> GetAllCodeByType(string type);
    }
}
