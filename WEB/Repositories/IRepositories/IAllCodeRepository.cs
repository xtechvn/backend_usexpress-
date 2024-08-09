using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAllCodeRepository
    {
        List<AllCode> GetAll();
        List<AllCode> GetListAllAsync();
        Task<AllCode> GetById(int Id);
        Task<long> Create(AllCode model);
        Task<long> Update(AllCode model);
        Task<long> Delete(int id);
        List<AllCode> GetListByType(string type);
        AllCode GetByType(string type);
        Task<short> GetLastestCodeValueByType(string type);
        Task<short> GetLastestOrderNoByType(string type);
        Task<AllCode> GetIDIfValueExists(string type, string description);
        Task<List<AllCode>> GetListSortByName(string type_name);
    }
}
