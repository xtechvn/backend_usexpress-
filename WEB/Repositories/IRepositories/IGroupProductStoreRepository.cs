using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IGroupProductStoreRepository
    {
        Task<List<GroupProductStore>> GetAll();
    }
}
